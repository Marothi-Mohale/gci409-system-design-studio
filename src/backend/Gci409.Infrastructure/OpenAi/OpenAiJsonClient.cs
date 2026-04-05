using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gci409.Infrastructure.OpenAi;

internal interface IOpenAiJsonClient
{
    bool IsConfigured { get; }

    Task<string> CompleteJsonAsync(string model, string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}

internal sealed class OpenAiJsonClient(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiJsonClient> logger) : IOpenAiJsonClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsConfigured => options.Value.Enabled && !string.IsNullOrWhiteSpace(options.Value.ApiKey);

    public async Task<string> CompleteJsonAsync(string model, string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("OpenAI generation is not configured.");
        }

        var requestBody = new ChatCompletionRequest(
            model,
            [
                new ChatCompletionMessage("system", systemPrompt),
                new ChatCompletionMessage("user", userPrompt)
            ],
            new ChatCompletionResponseFormat("json_object"),
            options.Value.Temperature);

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
        request.Content = JsonContent.Create(requestBody, options: JsonOptions);

        logger.LogInformation("Submitting OpenAI completion request to model {Model}.", model);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI request failed with status {(int)response.StatusCode}: {Truncate(payload)}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(payload, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI returned an unreadable response payload.");

        var content = completion.Choices.FirstOrDefault()?.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI returned no completion content.");
        }

        return NormalizeJsonPayload(content);
    }

    private static string NormalizeJsonPayload(string payload)
    {
        var trimmed = payload.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed[firstBrace..(lastBrace + 1)];
            }
        }

        return trimmed;
    }

    private static string Truncate(string value)
    {
        return value.Length <= 1_000 ? value : value[..1_000];
    }

    private sealed record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyCollection<ChatCompletionMessage> Messages,
        [property: JsonPropertyName("response_format")] ChatCompletionResponseFormat ResponseFormat,
        [property: JsonPropertyName("temperature")] decimal Temperature);

    private sealed record ChatCompletionMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatCompletionResponseFormat([property: JsonPropertyName("type")] string Type);

    private sealed record ChatCompletionResponse([property: JsonPropertyName("choices")] IReadOnlyCollection<ChatCompletionChoice> Choices);

    private sealed record ChatCompletionChoice([property: JsonPropertyName("message")] ChatCompletionMessage Message);
}
