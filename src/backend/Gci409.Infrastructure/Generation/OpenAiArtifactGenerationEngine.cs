using System.Text;
using System.Text.Json;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Infrastructure.OpenAi;
using Microsoft.Extensions.Options;

namespace Gci409.Infrastructure.Generation;

internal sealed class OpenAiArtifactGenerationEngine(
    IOpenAiJsonClient openAiJsonClient,
    IOptions<OpenAiOptions> options) : IArtifactGenerationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyCollection<ArtifactDraft>> GenerateAsync(ArtifactGenerationInput input, CancellationToken cancellationToken = default)
    {
        var payload = await openAiJsonClient.CompleteJsonAsync(
            options.Value.GenerationModel,
            BuildSystemPrompt(),
            BuildUserPrompt(input),
            cancellationToken);

        var response = JsonSerializer.Deserialize<ArtifactGenerationResponsePayload>(payload, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI artifact generation response could not be parsed.");

        var artifacts = response.Artifacts?
            .Select(MapArtifact)
            .Where(x => x is not null)
            .Cast<ArtifactDraft>()
            .ToList()
            ?? [];

        if (artifacts.Count == 0)
        {
            throw new InvalidOperationException("OpenAI returned no usable design artifacts.");
        }

        return artifacts;
    }

    private static ArtifactDraft? MapArtifact(ArtifactPayload payload)
    {
        if (!Enum.TryParse<ArtifactKind>(payload.ArtifactKind, true, out var artifactKind))
        {
            return null;
        }

        var diagramType = ResolveDiagramType(artifactKind, payload.DiagramType);
        var primaryFormat = ResolvePrimaryFormat(artifactKind, payload.PrimaryFormat, diagramType);
        var title = string.IsNullOrWhiteSpace(payload.Title) ? artifactKind.ToString() : payload.Title.Trim();
        var summary = string.IsNullOrWhiteSpace(payload.Summary)
            ? $"OpenAI generated a {artifactKind} draft for the current project baseline."
            : payload.Summary.Trim();

        var representations = NormalizeRepresentations(payload.Representations, primaryFormat, payload.Content, diagramType);
        if (!representations.TryGetValue(primaryFormat.ToString(), out var content) || string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var representationsJson = JsonSerializer.Serialize(representations, JsonOptions);
        return new ArtifactDraft(
            artifactKind,
            title,
            summary,
            primaryFormat,
            content,
            representationsJson,
            diagramType);
    }

    private static Dictionary<string, string> NormalizeRepresentations(
        IReadOnlyDictionary<string, string>? rawRepresentations,
        OutputFormat primaryFormat,
        string? primaryContent,
        UmlDiagramType diagramType)
    {
        var representations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (rawRepresentations is not null)
        {
            foreach (var pair in rawRepresentations)
            {
                if (Enum.TryParse<OutputFormat>(pair.Key, true, out var format) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    representations[format.ToString()] = pair.Value.Trim();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(primaryContent))
        {
            representations[primaryFormat.ToString()] = primaryContent.Trim();
        }

        if (diagramType != UmlDiagramType.None)
        {
            if (!representations.ContainsKey(OutputFormat.PlantUml.ToString()) && primaryFormat == OutputFormat.PlantUml && !string.IsNullOrWhiteSpace(primaryContent))
            {
                representations[OutputFormat.PlantUml.ToString()] = primaryContent.Trim();
            }

            if (!representations.ContainsKey(OutputFormat.Mermaid.ToString()) && primaryFormat == OutputFormat.Mermaid && !string.IsNullOrWhiteSpace(primaryContent))
            {
                representations[OutputFormat.Mermaid.ToString()] = primaryContent.Trim();
            }
        }

        return representations;
    }

    private static OutputFormat ResolvePrimaryFormat(ArtifactKind artifactKind, string? primaryFormat, UmlDiagramType diagramType)
    {
        if (Enum.TryParse<OutputFormat>(primaryFormat, true, out var parsed))
        {
            return parsed;
        }

        if (diagramType != UmlDiagramType.None)
        {
            return OutputFormat.PlantUml;
        }

        return artifactKind switch
        {
            ArtifactKind.ContextDiagram or ArtifactKind.DataFlowDiagram or ArtifactKind.Erd => OutputFormat.Mermaid,
            _ => OutputFormat.Markdown
        };
    }

    private static UmlDiagramType ResolveDiagramType(ArtifactKind artifactKind, string? diagramType)
    {
        if (Enum.TryParse<UmlDiagramType>(diagramType, true, out var parsed))
        {
            return parsed;
        }

        return artifactKind switch
        {
            ArtifactKind.UseCaseDiagram => UmlDiagramType.UseCase,
            ArtifactKind.ClassDiagram => UmlDiagramType.Class,
            ArtifactKind.SequenceDiagram => UmlDiagramType.Sequence,
            ArtifactKind.ActivityDiagram => UmlDiagramType.Activity,
            ArtifactKind.ComponentDiagram => UmlDiagramType.Component,
            ArtifactKind.DeploymentDiagram => UmlDiagramType.Deployment,
            _ => UmlDiagramType.None
        };
    }

    private static string BuildSystemPrompt()
    {
        return """
You are a principal software architect producing enterprise-grade system design artifacts.
Generate concrete, implementation-useful documents and diagrams from the provided project requirements and constraints.
Return strictly valid JSON and nothing else.
For diagram artifacts, prefer syntactically valid notation over prose.
For narrative artifacts, produce practical, high-quality Markdown that an architecture or delivery team could review directly.
Make only minimal explicit assumptions when information is missing.
""";
    }

    private static string BuildUserPrompt(ArtifactGenerationInput input)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Generate one artifact for each requested artifact kind.");
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
        builder.AppendLine($"Requested artifact kinds: {string.Join(", ", input.ArtifactKinds.Distinct())}");
        builder.AppendLine($"Allowed artifact kinds: {string.Join(", ", Enum.GetNames<ArtifactKind>())}");
        builder.AppendLine($"Allowed diagram types: {string.Join(", ", Enum.GetNames<UmlDiagramType>())}");
        builder.AppendLine($"Allowed output formats: {string.Join(", ", Enum.GetNames<OutputFormat>())}");
        builder.AppendLine();
        builder.AppendLine("Artifact guidance:");
        builder.AppendLine("- UseCaseDiagram, ClassDiagram, SequenceDiagram, ActivityDiagram, ComponentDiagram, DeploymentDiagram: provide PlantUml and Mermaid representations when possible.");
        builder.AppendLine("- ContextDiagram, DataFlowDiagram, and Erd: provide Mermaid.");
        builder.AppendLine("- ArchitectureSummary, ModuleDecomposition, ApiDesignSuggestion, and DatabaseDesignSuggestion: provide rich Markdown with headings, design decisions, assumptions, and delivery notes.");
        builder.AppendLine("- Keep artifacts tied to the specific project domain rather than generic template text.");
        builder.AppendLine("- If requirements imply fintech, compliance, audit, or security concerns, reflect them directly.");
        builder.AppendLine();
        builder.AppendLine("Return JSON with this exact shape:");
        builder.AppendLine("""
{
  "artifacts": [
    {
      "artifactKind": "UseCaseDiagram",
      "title": "string",
      "summary": "2-4 sentence artifact summary with assumptions if any",
      "primaryFormat": "PlantUml",
      "content": "primary notation or markdown body",
      "diagramType": "UseCase",
      "representations": {
        "PlantUml": "@startuml ...",
        "Mermaid": "flowchart LR ..."
      }
    }
  ]
}
""");

        return builder.ToString();
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

    private sealed record ArtifactGenerationResponsePayload(IReadOnlyCollection<ArtifactPayload>? Artifacts);

    private sealed record ArtifactPayload(
        string? ArtifactKind,
        string? Title,
        string? Summary,
        string? PrimaryFormat,
        string? Content,
        string? DiagramType,
        IReadOnlyDictionary<string, string>? Representations);
}
