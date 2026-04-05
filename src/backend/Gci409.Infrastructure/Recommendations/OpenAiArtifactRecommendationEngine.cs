using System.Text;
using System.Text.Json;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Recommendations;
using Gci409.Infrastructure.OpenAi;
using Microsoft.Extensions.Options;

namespace Gci409.Infrastructure.Recommendations;

internal sealed class OpenAiArtifactRecommendationEngine(
    IOpenAiJsonClient openAiJsonClient,
    IOptions<OpenAiOptions> options) : IArtifactRecommendationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyCollection<ArtifactRecommendationDraft>> RecommendAsync(RecommendationInput input, CancellationToken cancellationToken = default)
    {
        var payload = await openAiJsonClient.CompleteJsonAsync(
            options.Value.RecommendationModel,
            BuildSystemPrompt(),
            BuildUserPrompt(input),
            cancellationToken);

        var response = JsonSerializer.Deserialize<RecommendationResponsePayload>(payload, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI recommendation response could not be parsed.");

        var recommendations = response.Recommendations?
            .Select(MapRecommendation)
            .Where(x => x is not null)
            .Cast<ArtifactRecommendationDraft>()
            .OrderByDescending(x => x.ConfidenceScore)
            .ThenBy(x => x.ArtifactKind)
            .ToList()
            ?? [];

        if (recommendations.Count == 0)
        {
            throw new InvalidOperationException("OpenAI returned no usable artifact recommendations.");
        }

        return recommendations;
    }

    private static ArtifactRecommendationDraft? MapRecommendation(RecommendationItemPayload item)
    {
        if (!Enum.TryParse<ArtifactKind>(item.ArtifactKind, true, out var artifactKind))
        {
            return null;
        }

        if (!Enum.TryParse<RecommendationStrength>(item.Strength, true, out var strength))
        {
            strength = MapStrength(Clamp(item.ConfidenceScore ?? 0.5m));
        }

        var title = string.IsNullOrWhiteSpace(item.Title) ? $"{artifactKind} recommended" : item.Title.Trim();
        var rationale = string.IsNullOrWhiteSpace(item.Rationale)
            ? "OpenAI identified this artifact as relevant to the project inputs."
            : item.Rationale.Trim();

        return new ArtifactRecommendationDraft(
            artifactKind,
            title,
            rationale,
            Clamp(item.ConfidenceScore ?? 0.6m),
            strength);
    }

    private static string BuildSystemPrompt()
    {
        return """
You are a principal enterprise software architect.
Recommend the most relevant system design artifacts for a project based on its requirements and constraints.
Return strictly valid JSON and nothing else.
Your recommendations must be practical, delivery-oriented, and specific to the project context.
Avoid generic filler. Prefer artifacts that will help architecture, engineering, security, and delivery teams make concrete decisions.
""";
    }

    private static string BuildUserPrompt(RecommendationInput input)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Analyze the project and recommend the best artifacts to produce.");
        builder.AppendLine();
        builder.AppendLine($"Project: {input.ProjectName}");
        builder.AppendLine($"Requirement summary: {input.RequirementSummary}");
        builder.AppendLine();
        builder.AppendLine("Requirements:");
        AppendList(builder, input.RequirementDescriptions);
        builder.AppendLine();
        builder.AppendLine("Constraints:");
        AppendList(builder, input.ConstraintDescriptions);
        builder.AppendLine();
        builder.AppendLine($"Allowed artifact kinds: {string.Join(", ", Enum.GetNames<ArtifactKind>())}");
        builder.AppendLine("Recommendation guidance:");
        builder.AppendLine("- UseCaseDiagram when actors, business roles, and system interactions are central.");
        builder.AppendLine("- ClassDiagram when domain structure, entities, and relationships are central.");
        builder.AppendLine("- SequenceDiagram when time-ordered interactions or integration choreography matters.");
        builder.AppendLine("- ActivityDiagram when workflows, approvals, branching, or state transitions matter.");
        builder.AppendLine("- ComponentDiagram when modules, services, or boundaries matter.");
        builder.AppendLine("- DeploymentDiagram when infrastructure, hosting, or runtime topology matters.");
        builder.AppendLine("- ContextDiagram when external systems and system boundaries matter.");
        builder.AppendLine("- DataFlowDiagram when data movement matters.");
        builder.AppendLine("- Erd when relational data structure matters.");
        builder.AppendLine("- ArchitectureSummary is usually valuable for enterprise delivery.");
        builder.AppendLine();
        builder.AppendLine("Return JSON with this shape:");
        builder.AppendLine("""
{
  "recommendations": [
    {
      "artifactKind": "UseCaseDiagram",
      "title": "string",
      "rationale": "2-4 sentence explanation tied to the project inputs",
      "confidenceScore": 0.0,
      "strength": "Low|Medium|High"
    }
  ]
}
""");
        builder.AppendLine("Return between 4 and 10 recommendations when the project contains enough signal.");

        return builder.ToString();
    }

    private static RecommendationStrength MapStrength(decimal confidence)
    {
        return confidence switch
        {
            >= 0.8m => RecommendationStrength.High,
            >= 0.6m => RecommendationStrength.Medium,
            _ => RecommendationStrength.Low
        };
    }

    private static decimal Clamp(decimal value)
    {
        return value switch
        {
            < 0m => 0m,
            > 1m => 1m,
            _ => value
        };
    }

    private static void AppendList(StringBuilder builder, IReadOnlyCollection<string> items)
    {
        if (items.Count == 0)
        {
            builder.AppendLine("- None supplied");
            return;
        }

        foreach (var item in items)
        {
            builder.AppendLine($"- {item}");
        }
    }

    private sealed record RecommendationResponsePayload(IReadOnlyCollection<RecommendationItemPayload>? Recommendations);

    private sealed record RecommendationItemPayload(
        string? ArtifactKind,
        string? Title,
        string? Rationale,
        decimal? ConfidenceScore,
        string? Strength);
}
