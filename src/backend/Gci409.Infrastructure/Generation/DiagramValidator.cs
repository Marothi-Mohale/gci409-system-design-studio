using System.Text.RegularExpressions;
using Gci409.Domain.Artifacts;

namespace Gci409.Infrastructure.Generation;

/// <summary>
/// Validates generated diagrams for syntactic correctness and semantic quality.
/// Ensures PlantUml, Mermaid, and Markdown content meets minimum standards.
/// </summary>
internal sealed class DiagramValidator
{
    private static readonly Regex PlantUmlStartRegex = new(@"@start", RegexOptions.IgnoreCase);
    private static readonly Regex PlantUmlEndRegex = new(@"@end", RegexOptions.IgnoreCase);
    private static readonly Regex MermaidStartRegex = new(@"^(graph|flowchart|sequenceDiagram|classDiagram|stateDiagram|erDiagram|pie|gitGraph)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    public static ValidationResult Validate(ArtifactKind artifactKind, OutputFormat format, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ValidationResult.Invalid("Content is empty or whitespace.");
        }

        return format switch
        {
            OutputFormat.PlantUml => ValidatePlantUml(artifactKind, content),
            OutputFormat.Mermaid => ValidateMermaid(artifactKind, content),
            OutputFormat.Markdown => ValidateMarkdown(artifactKind, content),
            OutputFormat.Pdf or OutputFormat.Png => ValidationResult.Valid(), // Binary outputs validated elsewhere
            _ => ValidationResult.Invalid($"Unknown format: {format}")
        };
    }

    private static ValidationResult ValidatePlantUml(ArtifactKind artifactKind, string content)
    {
        var trimmed = content.Trim();

        if (!PlantUmlStartRegex.IsMatch(trimmed))
        {
            return ValidationResult.Invalid("PlantUml must start with @start directive (e.g., @startuml).");
        }

        if (!PlantUmlEndRegex.IsMatch(trimmed))
        {
            return ValidationResult.Invalid("PlantUml must end with @end directive (e.g., @enduml).");
        }

        var lines = trimmed.Split('\n');
        if (lines.Length < 3)
        {
            return ValidationResult.Invalid("PlantUml content too short; needs meaningful content between @start and @end.");
        }

        // Validate diagram-type-specific syntax
        var diagramType = InferDiagramType(artifactKind);
        if (diagramType == UmlDiagramType.None)
        {
            return ValidationResult.Valid(); // Not a UML diagram
        }

        var diagramSpecificCheck = ValidateDiagramTypeContent(diagramType, trimmed);
        if (!diagramSpecificCheck.IsValid)
        {
            return diagramSpecificCheck;
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateMermaid(ArtifactKind artifactKind, string content)
    {
        var trimmed = content.Trim();

        if (!MermaidStartRegex.IsMatch(trimmed))
        {
            return ValidationResult.Invalid("Mermaid must start with diagram type (graph, flowchart, sequenceDiagram, classDiagram, etc.).");
        }

        var lines = trimmed.Split('\n');
        if (lines.Length < 2)
        {
            return ValidationResult.Invalid("Mermaid content too short; needs more than just the diagram type.");
        }

        // Validate common syntax errors
        if (trimmed.Contains("{{") && !trimmed.Contains("}}"))
        {
            return ValidationResult.Invalid("Unmatched double braces in Mermaid syntax.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateMarkdown(ArtifactKind artifactKind, string content)
    {
        var trimmed = content.Trim();

        if (trimmed.Length < 20)
        {
            return ValidationResult.Invalid("Markdown content too short; needs at least minimal documentation.");
        }

        // Check for basic structure
        var hasHeadings = trimmed.Contains("#");
        var hasContent = trimmed.Split('\n').Length > 2;

        if (!hasHeadings && !hasContent)
        {
            return ValidationResult.Invalid("Markdown lacks structure (headings) and sufficient content.");
        }

        return ValidationResult.Valid();
    }

    private static UmlDiagramType InferDiagramType(ArtifactKind artifactKind)
    {
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

    private static ValidationResult ValidateDiagramTypeContent(UmlDiagramType diagramType, string content)
    {
        return diagramType switch
        {
            UmlDiagramType.UseCase => ValidateUseCaseContent(content),
            UmlDiagramType.Class => ValidateClassContent(content),
            UmlDiagramType.Sequence => ValidateSequenceContent(content),
            UmlDiagramType.Activity => ValidateActivityContent(content),
            UmlDiagramType.Component => ValidateComponentContent(content),
            UmlDiagramType.Deployment => ValidateDeploymentContent(content),
            _ => ValidationResult.Valid()
        };
    }

    private static ValidationResult ValidateUseCaseContent(string content)
    {
        // Use cases should have actor and usecase definitions
        if (!content.Contains("actor") && !content.Contains("usecase") && !content.Contains(":"))
        {
            return ValidationResult.Invalid("Use Case diagram should define actors and/or use cases.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateClassContent(string content)
    {
        // Classes should have class definitions
        if (!content.Contains("class") && !content.Contains("interface") && !content.Contains("{"))
        {
            return ValidationResult.Invalid("Class diagram should define classes or interfaces.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateSequenceContent(string content)
    {
        // Sequence diagrams should have participants and messages
        if (!content.Contains("participant") && !content.Contains("actor") && !content.Contains("-->"))
        {
            return ValidationResult.Invalid("Sequence diagram should define participants and messages.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateActivityContent(string content)
    {
        // Activity diagrams should have activities or states
        if (!content.Contains("activity") && !content.Contains("state") && !content.Contains(":"))
        {
            return ValidationResult.Invalid("Activity diagram should define activities or states.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateComponentContent(string content)
    {
        // Component diagrams should have components
        if (!content.Contains("component") && !content.Contains("[") && !content.Contains("interface"))
        {
            return ValidationResult.Invalid("Component diagram should define components.");
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult ValidateDeploymentContent(string content)
    {
        // Deployment diagrams should have nodes or artifacts
        if (!content.Contains("node") && !content.Contains("artifact") && !content.Contains("device"))
        {
            return ValidationResult.Invalid("Deployment diagram should define nodes, artifacts, or devices.");
        }

        return ValidationResult.Valid();
    }

    public sealed record ValidationResult(bool IsValid, string? Message)
    {
        public static ValidationResult Valid() => new(true, null);
        public static ValidationResult Invalid(string message) => new(false, message);
    }
}
