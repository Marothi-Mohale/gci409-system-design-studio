# Diagram Generation - Prompt Engineering Reference

## Overview

This document details the specialized prompts created for each diagram type. Each prompt is optimized for OpenAI's generation quality and includes diagram-specific instructions, quality criteria, and examples.

## System Prompt (Shared Across All Generations)

```text
You are a principal software architect producing enterprise-grade system design artifacts.
Your expertise spans UML diagrams (PlantUml, Mermaid), architecture documentation, API/database 
design, and system patterns.

CORE RESPONSIBILITIES:
1. Generate concrete, implementation-useful artifacts from project requirements and constraints
2. For diagrams: produce syntactically valid, parseable notation that renders correctly
3. For documentation: provide practical Markdown with design decisions, assumptions, and rationale
4. Make minimal assumptions when information is missing; note significant gaps in summaries

QUALITY STANDARDS:
- Diagrams must follow UML standards and tool syntax exactly
- Content must be domain-specific, not generic templates
- Reflect fintech, compliance, security, or audit requirements explicitly
- Ensure relationships, dependencies, and constraints are clearly visible
- Use meaningful naming conventions aligned with the project
```

## Universal Diagram Guidance

Applied to all diagram generation requests:

```text
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
```

## Specialized Prompts by Artifact Type

### 1. Use Case Diagram

**Core Instructions:**
- Identify all primary actors (users/systems) interacting with the system
- Extract use cases from the requirements (user stories, workflows)
- Show relationships: associations, includes (<<include>>), extends (<<extend>>)
- Align actors with requirements they support
- Include system boundaries clearly

**Quality Criteria:**
- 5-12 use cases maximum for clarity
- Meaningful use case names (verb + object pattern)
- Clear actor roles based on requirements context
- Include extends/includes relationships where dependencies exist

**Example Structure:**
```
@startuml UseCaseDiagram
Actor "User" as user
Actor "Admin" as admin
Usecase "Login" as login
Usecase "View Dashboard" as dashboard
Usecase "Manage Users" as manage

user --> login
user --> dashboard
admin --> manage
login ..> dashboard: includes
@enduml
```

---

### 2. Class Diagram

**Core Instructions:**
- Identify domain entities from requirements (nouns, key concepts)
- Define class properties aligned with functional requirements
- Model relationships: associations, inheritance, dependencies
- Include multiplicity indicators (1..1, 1..*, etc.)
- Show aggregation/composition where semantically appropriate

**Quality Criteria:**
- 8-15 classes for maintainability
- Realistic properties (not just id/name)
- Minimal visibility modifiers (focus on relationships)
- Abstract classes where inheritance patterns emerge
- Avoid cross-cutting concerns in this structural view

**Example Structure:**
```
@startuml ClassDiagram
Class "User" {
  id: UUID
  name: String
  email: String
  createdAt: DateTime
}

Class "Project" {
  id: UUID
  name: String
  description: String
  ownerId: UUID
}

User "1" --* "0..*" Project: owns
@enduml
```

---

### 3. Sequence Diagram

**Core Instructions:**
- Identify the primary use case or business flow
- List actors/objects in logical order (user/client → system → external services)
- Show message flow with clear naming (verb + noun, e.g., submitOrder())
- Include return messages for important operations
- Use alt/par/loop blocks for conditional/parallel flows if mentioned in requirements

**Quality Criteria:**
- 5-10 distinct messages maximum for clarity
- Clear lifecycle (participant creation/destruction if needed)
- Synchronous calls shown as solid arrows
- Asynchronous calls as dashed arrows if applicable
- Include activation boxes for duration visualization

**Example Structure:**
```
@startuml SequenceDiagram
Actor "User" as user
Participant "API" as api
Participant "Database" as db
Participant "EmailService" as email

user -> api: registerUser(name, email)
api -> db: createUser()
db --> api: user_id
api -> email: sendEmail(verification_link)
email --> api: sent
api --> user: success
@enduml
```

---

### 4. Activity Diagram

**Core Instructions:**
- Model processes/workflows described in requirements
- Include decision nodes (guards) for conditional branches
- Show parallel flows (forks/joins) for concurrent activities
- Use swimlanes for different roles/departments if applicable
- Mark start/end points explicitly

**Quality Criteria:**
- 7-15 activities maximum
- Clear decision criteria in guard conditions
- Synchronized fork/join patterns for parallel flows
- Meaningful activity names (gerunds, e.g., 'Processing Payment')

**Example Structure:**
```
@startuml ActivityDiagram
start
:Submit Order;
if (Payment Valid?) then (yes)
  :Process Payment;
  fork
    :Update Inventory;
  and
    :Send Confirmation;
  end fork
  :Complete Order;
else (no)
  :Reject Order;
endif
stop
@enduml
```

---

### 5. Component Diagram

**Core Instructions:**
- Decompose system into major components/services from requirements
- Show interfaces provided and required by each component
- Model dependencies between components
- Group related components logically (e.g., by domain, layer)
- Include external system integrations as components

**Quality Criteria:**
- 6-12 components for clarity
- Clear, specific component names (e.g., 'UserAuthenticationService')
- Interfaces represent actual contracts between components
- Dependencies flow from higher-level to lower-level components

**Example Structure:**
```
@startuml ComponentDiagram
Component "API Gateway" as gateway
Component "Auth Service" as auth
Component "User Service" as users
Component "Database" as db

gateway -down- auth: uses
gateway -down- users: uses
auth -down- db: persists
users -down- db: persists
@enduml
```

---

### 6. Deployment Diagram

**Core Instructions:**
- Identify deployment nodes (servers, containers, cloud resources) from requirements
- Show artifacts (compiled code, databases) deployed to each node
- Model communication paths between nodes
- Include external services/cloud platforms if mentioned
- Reflect scaling/redundancy requirements in deployment structure

**Quality Criteria:**
- 4-8 deployment nodes for clarity
- Specific deployment technology names where known
- Clear communication protocols between nodes
- Reflects non-functional requirements (scalability, availability)

**Example Structure:**
```
@startuml DeploymentDiagram
Node "Load Balancer" as lb
Node "API Server 1" as api1
Node "API Server 2" as api2
Node "Database" as db

lb -down- api1
lb -down- api2
api1 -down- db
api2 -down- db
@enduml
```

---

### 7. Context Diagram

**Core Instructions:**
- Show the system as a single box in the center
- Identify external systems/services the system integrates with
- Identify user roles/actors interacting with the system
- Draw relationships showing data flow or integration points
- Keep it simple and high-level (big picture view)

**Quality Criteria:**
- Central system with 3-6 external entities
- Clear labels on relationships describing interaction type
- User actors on left, external systems on right (convention)

**Example (Mermaid):**
```
graph TB
    User["👤 Users"]
    Admin["👤 Administrators"]
    PaymentGW["💳 Payment Gateway"]
    EmailSvc["📧 Email Service"]
    Analytics["📊 Analytics"]
    
    User -->|Places Orders| System["🎯 E-Commerce<br/>Platform"]
    Admin -->|Manages System| System
    System -->|Process Payment| PaymentGW
    System -->|Send Email| EmailSvc
    System -->|Track Events| Analytics
```

---

### 8. Data Flow Diagram

**Core Instructions:**
- Identify processes (transformations) from system workflows
- Map data stores (databases, caches, repositories) mentioned or implied
- Show data flows between processes, actors, and data stores
- Label flows with data types or descriptions (e.g., 'UserCredentials', 'OrderData')
- Organize logically: external entities → processes → data stores → outputs

**Quality Criteria:**
- 5-10 processes maximum
- 2-4 primary data stores
- Clear, domain-specific data naming
- No unused elements

**Example (Mermaid):**
```
flowchart LR
    User["👤 User"]
    Login["📋 Login<br/>Process"]
    Auth["🔐 Auth Store"]
    Dashboard["📊 Fetch<br/>Dashboard"]
    Cache["💾 User Cache"]
    Display["🖥️ Display<br/>Dashboard"]
    
    User -->|Credentials| Login
    Login -->|Verify| Auth
    Auth -->|OK| Dashboard
    Dashboard -->|Cache Hit?| Cache
    Cache -->|Data| Display
```

---

### 9. Entity-Relationship Diagram

**Core Instructions:**
- Extract entities (tables) from domain models and requirements
- Define attributes for each entity (columns with data types)
- Model relationships: one-to-one, one-to-many, many-to-many
- Identify primary keys and foreign keys
- Consider normalization (avoid repeating groups)

**Quality Criteria:**
- 6-12 entities for clarity
- Key attributes marked (id, unique identifiers)
- Clear cardinality on relationships
- Attribute names and types reflect domain language
- Third normal form (3NF) alignment

**Example (Mermaid):**
```
erDiagram
    USER ||--o{ PROJECT : owns
    PROJECT ||--o{ ARTIFACT : contains
    PROJECT ||--o{ REQUIREMENT : specifies
    ARTIFACT }o--|| ARTIFACT_VERSION : has
    
    USER {
        uuid id PK
        string email UK
        string name
        datetime created_at
    }
    
    PROJECT {
        uuid id PK
        uuid owner_id FK
        string name
        string description
    }
```

---

### 10. Architecture Summary (Markdown)

**Sections Required:**
1. **Overview** - High-level system description and scope
2. **Architectural Patterns** - Patterns used with justification
3. **System Layers** - Responsibilities of each layer
4. **Key Design Decisions** - 3-5 major decisions with rationale
5. **Non-Functional Considerations** - Performance, scalability, security, maintainability
6. **Assumptions & Dependencies** - What we assume and depend on

**Example Sections:**
```markdown
## Overview
The e-commerce platform implements a scalable, cloud-native architecture 
designed to handle multi-tenant marketplace scenarios with real-time 
inventory synchronization.

## Architectural Patterns
- **Layered Architecture**: Clear separation between API, business, and data layers
- **Microservices**: Payment, Inventory, and Notification services operate independently
- **Event-Driven**: Asynchronous communication for non-blocking operations
```

---

### 11. Module Decomposition (Markdown)

**Sections Required:**
1. **Module Overview** - Table with all modules
2. **Core Modules** - Detailed description of each
3. **Module Dependencies** - Dependency graph or textual description
4. **Integration Points** - How modules communicate

**Example:**
```markdown
## Module Overview
| Module | Purpose | Dependencies |
|--------|---------|--------------|

## Core Modules
### Authentication Module
- **Purpose**: Manages user identity and JWT token generation
- **Exports**: LoginService, TokenValidator
- **Dependencies**: UserRepository, CryptoService
```

---

### 12. API Design Suggestion (Markdown)

**Sections Required:**
1. **API Overview** - Style (REST/GraphQL), URL structure
2. **Core Resources** - Major endpoints with CRUD operations
3. **Authentication** - Auth scheme, required scopes
4. **Error Handling** - Error response format, HTTP codes
5. **Pagination & Filtering** - Query parameter strategy

**Example:**
```markdown
## API Overview
REST API with base URL: `/api/v1`

## Core Resources
### Users
- `GET /users` - List users
- `POST /users` - Create user
- `GET /users/{id}` - Get user
- `PUT /users/{id}` - Update user
- `DELETE /users/{id}` - Delete user
```

---

### 13. Database Design Suggestion (Markdown)

**Sections Required:**
1. **Database Choice** - DBMS recommendation with justification
2. **Schema Overview** - Core entities and relationships
3. **Entity Descriptions** - Primary key, indexes, constraints
4. **Data Integrity** - Foreign keys, unique constraints
5. **Performance Optimization** - Indexing, partitioning, caching
6. **Scalability & HA** - Replication, backup strategy

**Example:**
```markdown
## Database Choice
PostgreSQL 14+ recommended for:
- ACID compliance for financial transactions
- JSON support for flexible product metadata
- Excellent performance for complex queries

## Schema Overview
- Users (authentication, profiles)
- Projects (metadata, settings)
- Requirements (versioned, audited)
- GeneratedArtifacts (versions, exports)
```

---

## Prompt Selection Logic

The `BuildSpecializedPrompt()` method selects prompts based on `ArtifactKind`:

```csharp
private static string BuildSpecializedPrompt(ArtifactKind kind, ...)
{
    return kind switch
    {
        ArtifactKind.UseCaseDiagram => BuildUseCaseDiagramPrompt(...),
        ArtifactKind.ClassDiagram => BuildClassDiagramPrompt(...),
        ArtifactKind.SequenceDiagram => BuildSequenceDiagramPrompt(...),
        ArtifactKind.ActivityDiagram => BuildActivityDiagramPrompt(...),
        ArtifactKind.ComponentDiagram => BuildComponentDiagramPrompt(...),
        ArtifactKind.DeploymentDiagram => BuildDeploymentDiagramPrompt(...),
        ArtifactKind.ContextDiagram => BuildContextDiagramPrompt(...),
        ArtifactKind.DataFlowDiagram => BuildDataFlowDiagramPrompt(...),
        ArtifactKind.Erd => BuildEntityRelationshipDiagramPrompt(...),
        ArtifactKind.ArchitectureSummary => BuildArchitectureSummaryPrompt(...),
        ArtifactKind.ModuleDecomposition => BuildModuleDecompositionPrompt(...),
        ArtifactKind.ApiDesignSuggestion => BuildApiDesignPrompt(...),
        ArtifactKind.DatabaseDesignSuggestion => BuildDatabaseDesignPrompt(...),
        _ => BuildGenericPrompt(...)
    };
}
```

## Token Efficiency

| Prompt Type | Est. Input Tokens | Est. Output Tokens | Total |
|---|---|---|---|
| System Prompt | 350 | 0 | 350 |
| UseCase Prompt | 500 | 1000 | 1500 |
| Class Prompt | 600 | 1200 | 1800 |
| Architecture Summary | 400 | 2000 | 2400 |
| API Design | 400 | 2500 | 2900 |

- **Specialized prompts are 15-25% more efficient than batch generation**
- **Focused instructions improve quality, reducing need for regeneration**
- **Per-artifact approach enables partial success (fail on 1 of 5 types)**

## Customization

To modify a prompt, edit the corresponding method in `DiagramPromptBuilder.cs`:

```csharp
private static string BuildUseCaseDiagramPrompt(...)
{
    var sb = new StringBuilder();
    // Modify instructions here
    return sb.ToString();
}
```

Changes take effect immediately on next generation request.
