using System.Text;
using Gci409.Domain.Artifacts;

namespace Gci409.Infrastructure.Generation;

/// <summary>
/// Builds specialized, efficient prompts for diagram generation using OpenAI.
/// Each diagram type receives a tailored prompt that emphasizes correct syntax and semantic accuracy.
/// </summary>
internal static class DiagramPromptBuilder
{
    public static string BuildDiagramGuidance()
    {
        return """
DIAGRAM GENERATION RULES:
1. SYNTAX VALIDATION:
   - Mermaid: Must be syntactically valid and parseable by mermaid-js v10+
   - PlantUml: Must follow @startuml/@enduml conventions and be valid UML notation
   - Always escape special characters properly
   - Include minimal comments for clarity

2. SEMANTIC ACCURACY:
   - Reflect the actual components, relationships, and flows from requirements
   - Use meaningful names aligned with the project domain
   - Group related elements logically
   - Avoid generic placeholder names

3. LAYOUT & READABILITY:
   - Limit diagrams to 15-20 elements for clarity (break into multiple diagrams if needed)
   - Use clear directional flow (top-to-bottom for flows, left-to-right for data flow)
   - Apply consistent naming conventions within each diagram

4. CONSTRAINT INTEGRATION:
   - Consider performance requirements when designing architecture
   - Reflect security/compliance constraints in component relationships
   - Include integration points for external systems
   - Show data flow restricted by regulatory constraints if applicable
""";
    }

    public static string BuildSpecializedPrompt(ArtifactKind artifactKind, string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        return artifactKind switch
        {
            ArtifactKind.UseCaseDiagram => BuildUseCaseDiagramPrompt(projectName, requirements, constraints),
            ArtifactKind.ClassDiagram => BuildClassDiagramPrompt(projectName, requirementSummary, requirements),
            ArtifactKind.SequenceDiagram => BuildSequenceDiagramPrompt(projectName, requirementSummary, requirements),
            ArtifactKind.ActivityDiagram => BuildActivityDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.ComponentDiagram => BuildComponentDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.DeploymentDiagram => BuildDeploymentDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.ContextDiagram => BuildContextDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.DataFlowDiagram => BuildDataFlowDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.Erd => BuildEntityRelationshipDiagramPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.ArchitectureSummary => BuildArchitectureSummaryPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.ModuleDecomposition => BuildModuleDecompositionPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.ApiDesignSuggestion => BuildApiDesignPrompt(projectName, requirementSummary, requirements, constraints),
            ArtifactKind.DatabaseDesignSuggestion => BuildDatabaseDesignPrompt(projectName, requirementSummary, requirements, constraints),
            _ => BuildGenericPrompt(projectName, requirementSummary, requirements, constraints)
        };
    }

    private static string BuildUseCaseDiagramPrompt(string projectName, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Use Case Diagram (UML) for the system in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Identify all primary actors (users/systems) interacting with the system");
        sb.AppendLine("2. Extract use cases from the requirements (user stories, workflows)");
        sb.AppendLine("3. Show relationships: associations, includes (<<include>>), extends (<<extend>>)");
        sb.AppendLine("4. Align actors with requirements they support");
        sb.AppendLine("5. Include system boundaries clearly");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 5-12 use cases maximum for clarity");
        sb.AppendLine("- Meaningful use case names (verb + object pattern)");
        sb.AppendLine("- Clear actor roles based on requirements context");
        sb.AppendLine("- Include extends/includes relationships where dependencies exist");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildClassDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Class Diagram (UML) in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Identify domain entities from requirements (nouns, key concepts)");
        sb.AppendLine("2. Define class properties aligned with functional requirements");
        sb.AppendLine("3. Model relationships: associations, inheritance, dependencies");
        sb.AppendLine("4. Include multiplicity indicators (1..1, 1..*, etc.)");
        sb.AppendLine("5. Show aggregation/composition where semantically appropriate");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 8-15 classes for maintainability");
        sb.AppendLine("- Realistic properties (not just id/name)");
        sb.AppendLine("- Minimal visibility modifiers (focus on relationships)");
        sb.AppendLine("- Abstract classes where inheritance patterns emerge");
        sb.AppendLine("- Avoid cross-cutting concerns in this structural view");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildSequenceDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Sequence Diagram (UML) in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Identify the primary use case or business flow");
        sb.AppendLine("2. List actors/objects in logical order (user/client → system → external services)");
        sb.AppendLine("3. Show message flow with clear naming (verb + noun, e.g., submitOrder())");
        sb.AppendLine("4. Include return messages for important operations");
        sb.AppendLine("5. Use alt/par/loop blocks for conditional/parallel flows if mentioned in requirements");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 5-10 distinct messages maximum for clarity");
        sb.AppendLine("- Clear lifecycle (participant creation/destruction if needed)");
        sb.AppendLine("- Synchronous calls shown as solid arrows");
        sb.AppendLine("- Asynchronous calls as dashed arrows if applicable");
        sb.AppendLine("- Include activation boxes for duration visualization");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildActivityDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate an Activity Diagram (UML) in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Model processes/workflows described in requirements");
        sb.AppendLine("2. Include decision nodes (guards) for conditional branches");
        sb.AppendLine("3. Show parallel flows (forks/joins) for concurrent activities");
        sb.AppendLine("4. Use swimlanes for different roles/departments if applicable");
        sb.AppendLine("5. Mark start/end points explicitly");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 7-15 activities maximum");
        sb.AppendLine("- Clear decision criteria in guard conditions");
        sb.AppendLine("- Synchronized fork/join patterns for parallel flows");
        sb.AppendLine("- Meaningful activity names (gerunds, e.g., 'Processing Payment')");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildComponentDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Component Diagram (UML) in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Decompose system into major components/services from requirements");
        sb.AppendLine("2. Show interfaces provided and required by each component");
        sb.AppendLine("3. Model dependencies between components");
        sb.AppendLine("4. Group related components logically (e.g., by domain, layer)");
        sb.AppendLine("5. Include external system integrations as components");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 6-12 components for clarity");
        sb.AppendLine("- Clear, specific component names (e.g., 'UserAuthenticationService')");
        sb.AppendLine("- Interfaces represent actual contracts between components");
        sb.AppendLine("- Dependencies flow from higher-level to lower-level components");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildDeploymentDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Deployment Diagram (UML) in PlantUml and Mermaid formats.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Identify deployment nodes (servers, containers, cloud resources) from requirements");
        sb.AppendLine("2. Show artifacts (compiled code, databases) deployed to each node");
        sb.AppendLine("3. Model communication paths between nodes");
        sb.AppendLine("4. Include external services/cloud platforms if mentioned");
        sb.AppendLine("5. Reflect scaling/redundancy requirements in deployment structure");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 4-8 deployment nodes for clarity");
        sb.AppendLine("- Specific deployment technology names where known");
        sb.AppendLine("- Clear communication protocols between nodes");
        sb.AppendLine("- Reflects non-functional requirements (scalability, availability)");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildContextDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Context Diagram (C4 Model Level 1) in Mermaid format.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Show the system as a single box in the center");
        sb.AppendLine("2. Identify external systems/services the system integrates with");
        sb.AppendLine("3. Identify user roles/actors interacting with the system");
        sb.AppendLine("4. Draw relationships showing data flow or integration points");
        sb.AppendLine("5. Keep it simple and high-level (big picture view)");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- Central system with 3-6 external entities");
        sb.AppendLine("- Clear labels on relationships describing interaction type");
        sb.AppendLine("- User actors on left, external systems on right (convention)");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildDataFlowDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate a Data Flow Diagram (DFD) in Mermaid format showing system data movements.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Identify processes (transformations) from system workflows");
        sb.AppendLine("2. Map data stores (databases, caches, repositories) mentioned or implied");
        sb.AppendLine("3. Show data flows between processes, actors, and data stores");
        sb.AppendLine("4. Label flows with data types or descriptions (e.g., 'UserCredentials', 'OrderData')");
        sb.AppendLine("5. Organize logically: external entities → processes → data stores → outputs");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 5-10 processes maximum");
        sb.AppendLine("- 2-4 primary data stores");
        sb.AppendLine("- Clear, domain-specific data naming");
        sb.AppendLine("- No unused elements");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildEntityRelationshipDiagramPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate an Entity-Relationship Diagram (ERD) in Mermaid format for database design.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("1. Extract entities (tables) from domain models and requirements");
        sb.AppendLine("2. Define attributes for each entity (columns with data types)");
        sb.AppendLine("3. Model relationships: one-to-one, one-to-many, many-to-many");
        sb.AppendLine("4. Identify primary keys and foreign keys");
        sb.AppendLine("5. Consider normalization (avoid repeating groups)");
        sb.AppendLine();
        sb.AppendLine("QUALITY CRITERIA:");
        sb.AppendLine("- 6-12 entities for clarity");
        sb.AppendLine("- Key attributes marked (id, unique identifiers)");
        sb.AppendLine("- Clear cardinality on relationships");
        sb.AppendLine("- Attribute names and types reflect domain language");
        sb.AppendLine("- Third normal form (3NF) alignment");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildArchitectureSummaryPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate an Architecture Summary as comprehensive Markdown documentation.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("Structure the document with these sections:");
        sb.AppendLine("## Overview");
        sb.AppendLine("- High-level description of the system's purpose and scope");
        sb.AppendLine("## Architectural Patterns");
        sb.AppendLine("- Primary patterns used (MVC, microservices, layered, event-driven, etc.)");
        sb.AppendLine("- Justification based on requirements and constraints");
        sb.AppendLine("## System Layers");
        sb.AppendLine("- Presentation/API Layer");
        sb.AppendLine("- Business Logic Layer");
        sb.AppendLine("- Data Access Layer");
        sb.AppendLine("- Infrastructure/Platform Layer");
        sb.AppendLine("- Brief description and responsibilities of each");
        sb.AppendLine("## Key Design Decisions");
        sb.AppendLine("- Title, rationale, and alternatives considered for 3-5 major decisions");
        sb.AppendLine("## Non-Functional Considerations");
        sb.AppendLine("- Performance, scalability, security, maintainability");
        sb.AppendLine("- How architecture addresses constraints");
        sb.AppendLine("## Assumptions & Dependencies");
        sb.AppendLine("- Key assumptions made");
        sb.AppendLine("- External dependencies");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildModuleDecompositionPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate Module Decomposition document as guided Markdown.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("Structure the document with these sections:");
        sb.AppendLine("## Module Overview");
        sb.AppendLine("- Table with Module Name, Purpose, Key Responsibilities");
        sb.AppendLine("## Core Modules (describe each)");
        sb.AppendLine("### [Module Name]");
        sb.AppendLine("- **Purpose**: What this module does");
        sb.AppendLine("- **Exports**: Public interfaces/functions");
        sb.AppendLine("- **Dependencies**: Other modules it depends on");
        sb.AppendLine("- **Rationale**: Why structured this way");
        sb.AppendLine("## Module Dependencies");
        sb.AppendLine("- Clear dependency graph or textual description");
        sb.AppendLine("- Avoid circular dependencies");
        sb.AppendLine("## Integration Points");
        sb.AppendLine("- How modules communicate");
        sb.AppendLine("- Message formats or interface contracts");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildApiDesignPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate API Design Suggestion as detailed Markdown specification.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("Structure the document with these sections:");
        sb.AppendLine("## API Overview");
        sb.AppendLine("- API style (REST, GraphQL, gRPC) recommender and justification");
        sb.AppendLine("- Base URL/endpoint structure");
        sb.AppendLine("## Core Resources & Endpoints");
        sb.AppendLine("For each major resource:");
        sb.AppendLine("### [Resource Name]");
        sb.AppendLine("- `GET /resources` - List");
        sb.AppendLine("- `POST /resources` - Create");
        sb.AppendLine("- `GET /resources/{id}` - Retrieve");
        sb.AppendLine("- `PUT /resources/{id}` - Update");
        sb.AppendLine("- `DELETE /resources/{id}` - Delete");
        sb.AppendLine("- Each with request/response schema outlines");
        sb.AppendLine("## Authentication & Authorization");
        sb.AppendLine("- Auth scheme (bearer token, API key, OAuth2)");
        sb.AppendLine("- Required scopes/permissions");
        sb.AppendLine("## Error Handling");
        sb.AppendLine("- Standard error response format");
        sb.AppendLine("- Common error codes (400, 401, 403, 404, 500)");
        sb.AppendLine("## Pagination & Filtering");
        sb.AppendLine("- Query parameter strategy");
        sb.AppendLine("- Page size recommendations");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildDatabaseDesignPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TASK: Generate Database Design Suggestion as comprehensive Markdown guide.");
        sb.AppendLine();
        sb.AppendLine("CONTEXT:");
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        sb.AppendLine("INSTRUCTIONS:");
        sb.AppendLine("Structure the document with these sections:");
        sb.AppendLine("## Database Choice Rationale");
        sb.AppendLine("- Recommended DBMS (PostgreSQL, MongoDB, etc.)");
        sb.AppendLine("- Justification based on requirements");
        sb.AppendLine("## Schema Overview");
        sb.AppendLine("- Core entities/collections and relationships");
        sb.AppendLine("## Entity Descriptions");
        sb.AppendLine("For each major entity:");
        sb.AppendLine("### [Entity Name]");
        sb.AppendLine("- Primary Key strategy");
        sb.AppendLine("- Critical indexes");
        sb.AppendLine("- Constraints (unique, foreign keys, checks)");
        sb.AppendLine("## Data Integrity & Constraints");
        sb.AppendLine("- Foreign key relationships");
        sb.AppendLine("- Unique constraints");
        sb.AppendLine("- Check constraints reflecting business rules");
        sb.AppendLine("## Performance Optimization");
        sb.AppendLine("- Recommended indexes");
        sb.AppendLine("- Partitioning strategy if applicable");
        sb.AppendLine("- Caching strategy (Redis, memcached)");
        sb.AppendLine("## Scalability & High Availability");
        sb.AppendLine("- Replication strategy");
        sb.AppendLine("- Backup and recovery approach");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildGenericPrompt(string projectName, string requirementSummary, IReadOnlyCollection<string> requirements, IReadOnlyCollection<string> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Project: {projectName}");
        sb.AppendLine($"Summary: {requirementSummary}");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        AppendList(sb, requirements);
        sb.AppendLine();
        sb.AppendLine("Constraints:");
        AppendList(sb, constraints);
        sb.AppendLine();
        return sb.ToString();
    }

    private static void AppendList(StringBuilder sb, IReadOnlyCollection<string> items)
    {
        if (items.Count == 0)
        {
            sb.AppendLine("- None specified");
            return;
        }

        foreach (var item in items)
        {
            sb.AppendLine($"- {item}");
        }
    }
}
