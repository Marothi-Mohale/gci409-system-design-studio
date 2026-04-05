using System.ComponentModel.DataAnnotations;

namespace Gci409.Infrastructure.OpenAi;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public bool Enabled { get; set; }

    [Required]
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    public string? ApiKey { get; set; }

    [Required]
    public string RecommendationModel { get; set; } = "gpt-4.1-mini";

    [Required]
    public string GenerationModel { get; set; } = "gpt-4.1";

    [Range(0, 2)]
    public decimal Temperature { get; set; } = 0.2m;

    [Range(10, 300)]
    public int RequestTimeoutSeconds { get; set; } = 120;
}
