using System.Text;
using System.Text.Json;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;

namespace Gci409.Infrastructure.Generation;

public sealed class RuleBasedArtifactGenerationEngine : IArtifactGenerationEngine
{
    public Task<IReadOnlyCollection<ArtifactDraft>> GenerateAsync(ArtifactGenerationInput input, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ArtifactDraft> results = input.ArtifactKinds.Distinct().Select(kind => kind switch
        {
            ArtifactKind.UseCaseDiagram => BuildUseCase(input),
            ArtifactKind.ClassDiagram => BuildClassDiagram(input),
            ArtifactKind.SequenceDiagram => BuildSequenceDiagram(input),
            ArtifactKind.ActivityDiagram => BuildActivityDiagram(input),
            ArtifactKind.ComponentDiagram => BuildComponentDiagram(input),
            ArtifactKind.DeploymentDiagram => BuildDeploymentDiagram(input),
            ArtifactKind.ContextDiagram => BuildContextDiagram(),
            ArtifactKind.DataFlowDiagram => BuildDataFlow(),
            ArtifactKind.Erd => BuildErd(),
            ArtifactKind.ArchitectureSummary => BuildArchitectureSummary(input),
            ArtifactKind.ModuleDecomposition => BuildModuleDecomposition(),
            ArtifactKind.ApiDesignSuggestion => BuildApiSuggestion(),
            ArtifactKind.DatabaseDesignSuggestion => BuildDatabaseSuggestion(),
            _ => BuildArchitectureSummary(input)
        }).ToList();

        return Task.FromResult(results);
    }

    private static ArtifactDraft BuildUseCase(ArtifactGenerationInput input)
    {
        var actors = ExtractActors(input.RequirementDescriptions);
        var useCases = ExtractUseCases(input.RequirementDescriptions);
        var plantUml = new StringBuilder()
            .AppendLine("@startuml")
            .AppendLine("left to right direction");

        foreach (var actor in actors)
        {
            plantUml.AppendLine($"actor \"{actor}\" as {Sanitize(actor)}");
        }

        foreach (var useCase in useCases)
        {
            plantUml.AppendLine($"usecase \"{useCase}\" as {Sanitize(useCase)}");
        }

        foreach (var actor in actors)
        {
            foreach (var useCase in useCases.Take(2))
            {
                plantUml.AppendLine($"{Sanitize(actor)} --> {Sanitize(useCase)}");
            }
        }

        plantUml.AppendLine("@enduml");

        var mermaid = new StringBuilder().AppendLine("flowchart LR");
        foreach (var actor in actors)
        {
            mermaid.AppendLine($"    {Sanitize(actor)}[{actor}]");
        }

        foreach (var useCase in useCases)
        {
            mermaid.AppendLine($"    {Sanitize(useCase)}(({useCase}))");
        }

        foreach (var actor in actors)
        {
            foreach (var useCase in useCases.Take(2))
            {
                mermaid.AppendLine($"    {Sanitize(actor)} --> {Sanitize(useCase)}");
            }
        }

        return UmlDraft(ArtifactKind.UseCaseDiagram, "Use Case Diagram", "Primary actors and interactions.", plantUml.ToString(), mermaid.ToString(), UmlDiagramType.UseCase);
    }

    private static ArtifactDraft BuildClassDiagram(ArtifactGenerationInput input)
    {
        var entities = ExtractEntities(input.RequirementDescriptions);
        var plantUml = new StringBuilder().AppendLine("@startuml");
        var mermaid = new StringBuilder().AppendLine("classDiagram");

        foreach (var entity in entities)
        {
            var sanitized = Sanitize(entity);
            plantUml.AppendLine($"class {sanitized} {{");
            plantUml.AppendLine("  +Id");
            plantUml.AppendLine("  +Name");
            plantUml.AppendLine("}");

            mermaid.AppendLine($"    class {sanitized} {{");
            mermaid.AppendLine("      +Id");
            mermaid.AppendLine("      +Name");
            mermaid.AppendLine("    }");
        }

        for (var index = 0; index < entities.Count - 1; index++)
        {
            plantUml.AppendLine($"{Sanitize(entities[index])} --> {Sanitize(entities[index + 1])}");
            mermaid.AppendLine($"    {Sanitize(entities[index])} --> {Sanitize(entities[index + 1])}");
        }

        plantUml.AppendLine("@enduml");
        return UmlDraft(ArtifactKind.ClassDiagram, "Class Diagram", "Initial domain structure and relationships.", plantUml.ToString(), mermaid.ToString(), UmlDiagramType.Class);
    }

    private static ArtifactDraft BuildSequenceDiagram(ArtifactGenerationInput input)
    {
        var actor = Sanitize(ExtractActors(input.RequirementDescriptions).FirstOrDefault() ?? "User");
        var plantUml = $"""
@startuml
actor {actor}
participant API
database PostgreSQL

{actor} -> API: Submit request
API -> PostgreSQL: Persist changes
PostgreSQL --> API: Return stored result
API --> {actor}: Return response
@enduml
""";

        var mermaid = """
sequenceDiagram
    actor User
    participant API
    participant PostgreSQL
    User->>API: Submit request
    API->>PostgreSQL: Persist changes
    PostgreSQL-->>API: Return stored result
    API-->>User: Return response
""";

        return UmlDraft(ArtifactKind.SequenceDiagram, "Sequence Diagram", "Ordered interaction flow for a representative request.", plantUml, mermaid, UmlDiagramType.Sequence);
    }

    private static ArtifactDraft BuildActivityDiagram(ArtifactGenerationInput input)
    {
        const string plantUml = """
@startuml
start
:Capture requirements;
:Evaluate constraints;
:Recommend artifacts;
:Generate design outputs;
if (Review required?) then (yes)
  :Submit for review;
  :Approve or revise;
endif
stop
@enduml
""";

        const string mermaid = """
flowchart TD
    Start([Start]) --> Capture[Capture requirements]
    Capture --> Evaluate[Evaluate constraints]
    Evaluate --> Recommend[Recommend artifacts]
    Recommend --> Generate[Generate design outputs]
    Generate --> Review{Review required?}
    Review -->|Yes| Submit[Submit for review]
    Submit --> Approve[Approve or revise]
    Approve --> End([End])
    Review -->|No| End
""";

        return UmlDraft(ArtifactKind.ActivityDiagram, "Activity Diagram", "High-level process flow from requirements through review.", plantUml, mermaid, UmlDiagramType.Activity);
    }

    private static ArtifactDraft BuildComponentDiagram(ArtifactGenerationInput input)
    {
        const string plantUml = """
@startuml
package "Frontend" {
  [React SPA]
}
package "Backend" {
  [API]
  [Recommendation Engine]
  [Generation Engine]
  [Export Service]
}
database "PostgreSQL"

[React SPA] --> [API]
[API] --> [Recommendation Engine]
[API] --> [Generation Engine]
[Generation Engine] --> [Export Service]
[API] --> "PostgreSQL"
[Generation Engine] --> "PostgreSQL"
@enduml
""";

        const string mermaid = """
flowchart LR
    FE[React SPA] --> API[ASP.NET Core API]
    API --> REC[Recommendation Engine]
    API --> GEN[Generation Engine]
    GEN --> EXP[Export Service]
    API --> DB[(PostgreSQL)]
    GEN --> DB
""";

        return UmlDraft(ArtifactKind.ComponentDiagram, "Component Diagram", "Core runtime components and their interactions.", plantUml, mermaid, UmlDiagramType.Component);
    }

    private static ArtifactDraft BuildDeploymentDiagram(ArtifactGenerationInput input)
    {
        const string plantUml = """
@startuml
node "Browser" {
  artifact "React App"
}
node "API Container" {
  component "gci409 API"
}
node "Worker Container" {
  component "gci409 Worker"
}
database "PostgreSQL"

"React App" --> "gci409 API"
"gci409 API" --> "PostgreSQL"
"gci409 Worker" --> "PostgreSQL"
@enduml
""";

        const string mermaid = """
flowchart TB
    Browser[Browser / React App] --> API[API Container]
    API --> DB[(PostgreSQL)]
    Worker[Worker Container] --> DB
""";

        return UmlDraft(ArtifactKind.DeploymentDiagram, "Deployment Diagram", "Containerized deployment topology for the current solution.", plantUml, mermaid, UmlDiagramType.Deployment);
    }

    private static ArtifactDraft BuildContextDiagram()
    {
        const string mermaid = """
flowchart LR
    Analyst[Business Analyst]
    Architect[Solution Architect]
    GCI[gci409]
    Delivery[Delivery Team]
    Review[Architecture Review Board]

    Analyst --> GCI
    Architect --> GCI
    GCI --> Delivery
    GCI --> Review
""";

        return TextDraft(ArtifactKind.ContextDiagram, "Context Diagram", "External actors and neighboring teams around gci409.", OutputFormat.Mermaid, mermaid);
    }

    private static ArtifactDraft BuildDataFlow()
    {
        const string mermaid = """
flowchart LR
    Inputs[Requirements & Constraints] --> Analyzer[Recommendation Analyzer]
    Analyzer --> Generator[Artifact Generator]
    Generator --> Repository[Artifacts Repository]
    Repository --> Exports[Export Output]
""";

        return TextDraft(ArtifactKind.DataFlowDiagram, "Data Flow Diagram", "Movement of requirements through recommendation, generation, and export.", OutputFormat.Mermaid, mermaid);
    }

    private static ArtifactDraft BuildErd()
    {
        const string mermaid = """
erDiagram
    PROJECT ||--o{ REQUIREMENT_SET : contains
    REQUIREMENT_SET ||--o{ REQUIREMENT_SET_VERSION : versions
    REQUIREMENT_SET_VERSION ||--o{ REQUIREMENT_ITEM : includes
    REQUIREMENT_SET_VERSION ||--o{ CONSTRAINT_ITEM : includes
    PROJECT ||--o{ GENERATED_ARTIFACT : owns
    GENERATED_ARTIFACT ||--o{ ARTIFACT_VERSION : versions
    PROJECT ||--o{ GENERATION_REQUEST : schedules
""";

        return TextDraft(ArtifactKind.Erd, "ERD", "Core relational model for projects, requirements, and artifacts.", OutputFormat.Mermaid, mermaid);
    }

    private static ArtifactDraft BuildArchitectureSummary(ArtifactGenerationInput input)
    {
        var markdown = $"""
# Architecture Summary

## Project
{input.ProjectName}

## Design Direction
- Use a modular monolith to keep the domain consistent while the product evolves.
- Separate the API runtime from the worker runtime to isolate user traffic from generation workloads.
- Persist requirements, recommendations, generation requests, and artifact versions in PostgreSQL.
- Treat UML and design artifacts as versioned, reviewable outputs rather than transient text.

## Key Constraints
{string.Join(Environment.NewLine, input.ConstraintDescriptions.Select(x => $"- {x}"))}

## Primary Capabilities
- Requirement and constraint capture
- Artifact recommendation
- Asynchronous artifact generation
- Versioned artifact storage
- Export of Markdown, Mermaid, and PlantUML assets
""";

        return TextDraft(ArtifactKind.ArchitectureSummary, "Architecture Summary", "Narrative architecture baseline for the project.", OutputFormat.Markdown, markdown);
    }

    private static ArtifactDraft BuildModuleDecomposition()
    {
        const string markdown = """
# Module Decomposition

## Core Modules
- Identity and access
- Projects and collaboration
- Requirements and constraints
- Recommendations
- Generation orchestration
- Artifacts and versioning
- Exports and audit logging

## Engineering Notes
- Each module should own its data and application services.
- Cross-module orchestration should happen through application contracts.
- Artifact generation should remain extensible through pluggable builders.
""";

        return TextDraft(ArtifactKind.ModuleDecomposition, "Module Decomposition", "Initial modular decomposition of the backend.", OutputFormat.Markdown, markdown);
    }

    private static ArtifactDraft BuildApiSuggestion()
    {
        const string markdown = """
# API Design Suggestions

## Core Resource Groups
- `/api/auth`
- `/api/projects`
- `/api/projects/{projectId}/requirements`
- `/api/projects/{projectId}/recommendations`
- `/api/projects/{projectId}/generation-requests`
- `/api/projects/{projectId}/artifacts`

## API Style
- Use REST for project, requirement, recommendation, and artifact resources.
- Use asynchronous request tracking for long-running generation operations.
- Expose immutable artifact version resources for export and review.
""";

        return TextDraft(ArtifactKind.ApiDesignSuggestion, "API Design Suggestions", "Initial API surface recommendation.", OutputFormat.Markdown, markdown);
    }

    private static ArtifactDraft BuildDatabaseSuggestion()
    {
        const string markdown = """
# Database Design Suggestions

## Storage Strategy
- Use PostgreSQL as the system of record.
- Keep versioned tables for requirement sets, rules, templates, and artifacts.
- Store textual diagram sources in the database.
- Reserve external object storage for later binary export outputs.

## Key Tables
- projects
- requirement_sets
- requirement_set_versions
- recommendations
- generation_requests
- generated_artifacts
- artifact_versions
- artifact_exports
""";

        return TextDraft(ArtifactKind.DatabaseDesignSuggestion, "Database Design Suggestions", "Initial relational storage guidance for the project.", OutputFormat.Markdown, markdown);
    }

    private static ArtifactDraft UmlDraft(ArtifactKind kind, string title, string summary, string plantUml, string mermaid, UmlDiagramType diagramType)
    {
        var representations = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [OutputFormat.PlantUml.ToString()] = plantUml,
            [OutputFormat.Mermaid.ToString()] = mermaid
        });

        return new ArtifactDraft(kind, title, summary, OutputFormat.PlantUml, plantUml, representations, diagramType);
    }

    private static ArtifactDraft TextDraft(ArtifactKind kind, string title, string summary, OutputFormat primaryFormat, string content)
    {
        var representations = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [primaryFormat.ToString()] = content
        });

        return new ArtifactDraft(kind, title, summary, primaryFormat, content, representations, UmlDiagramType.None);
    }

    private static List<string> ExtractActors(IEnumerable<string> descriptions)
    {
        var candidates = descriptions
            .SelectMany(x => x.Split([' ', ',', '.', ';', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(x => x.Length > 3)
            .Where(x => char.IsUpper(x[0]))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        return candidates.Count != 0 ? candidates : ["User", "Admin"];
    }

    private static List<string> ExtractUseCases(IEnumerable<string> descriptions)
    {
        var candidates = descriptions
            .Select(x => x.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? x)
            .Select(x => x.Length > 48 ? x[..48] : x)
            .Distinct()
            .Take(4)
            .ToList();

        return candidates.Count != 0 ? candidates : ["Capture Requirements", "Generate Artifacts"];
    }

    private static List<string> ExtractEntities(IEnumerable<string> descriptions)
    {
        var tokens = descriptions
            .SelectMany(x => x.Split([' ', ',', '.', ';', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(x => x.Length > 4)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        return tokens.Count != 0 ? tokens : ["Project", "Requirement", "Artifact", "Export"];
    }

    private static string Sanitize(string value)
    {
        var chars = value.Where(char.IsLetterOrDigit).ToArray();
        return chars.Length == 0 ? "Node" : new string(chars);
    }
}
