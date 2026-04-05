# Diagram Generation Quick Start Guide

## OpenAI Configuration

Before generating diagrams, ensure OpenAI is configured:

### 1. Set API Key

**Option A - Environment Variable:**
```bash
$env:OPENAI_API_KEY = "sk-your-api-key-here"
```

**Option B - Appsettings.Development.json:**
```json
{
  "OpenAi": {
    "Enabled": true,
    "ApiKey": "sk-your-api-key-here",
    "ApiBaseUrl": "https://api.openai.com/v1",
    "GenerationModel": "gpt-4-turbo",
    "Temperature": 0.7
  }
}
```

### 2. Verify Configuration

The application will automatically detect and use the OpenAI configuration. Check logs for confirmation messages.

## Using the Diagram Generation System

### Via HTTP API

**Endpoint:** `POST /api/projects/{projectId}/generation/queue`

**Request Body:**
```json
{
  "artifactKinds": [1, 2, 3],
  "preferredFormat": 1
}
```

Where:
- `artifactKinds`: Array of ArtifactKind enum values
  - 1: UseCaseDiagram
  - 2: ClassDiagram
  - 3: SequenceDiagram
  - 4: ActivityDiagram
  - 5: ComponentDiagram
  - 6: DeploymentDiagram
  - 7: ContextDiagram
  - 8: DataFlowDiagram
  - 9: Erd
  - 10: ArchitectureSummary
  - 11: ModuleDecomposition
  - 12: ApiDesignSuggestion
  - 13: DatabaseDesignSuggestion

- `preferredFormat`: OutputFormat enum value
  - 1: Markdown
  - 2: Mermaid
  - 3: PlantUml
  - 4: Pdf
  - 5: Png

**Example Request:**
```bash
curl -X POST http://localhost:5099/api/projects/your-project-id/generation/queue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-token" \
  -d '{
    "artifactKinds": [1, 2, 3, 7, 10],
    "preferredFormat": 2
  }'
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "requirementSetVersionId": "...",
  "status": 1,
  "targets": [
    { "artifactKind": 1, "preferredFormat": 2 },
    { "artifactKind": 2, "preferredFormat": 2 },
    { "artifactKind": 3, "preferredFormat": 2 },
    { "artifactKind": 7, "preferredFormat": 2 },
    { "artifactKind": 10, "preferredFormat": 2 }
  ],
  "createdAtUtc": "2026-04-05T22:30:00Z",
  "completedAtUtc": null,
  "failureReason": null
}
```

### Check Generation Status

**Endpoint:** `GET /api/projects/{projectId}/generation`

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": 3,
    "completedAtUtc": "2026-04-05T22:31:15Z",
    "targets": [...]
  }
]
```

Status codes:
- 1: Queued
- 2: Processing
- 3: Completed
- 4: Failed

### Retrieve Generated Artifacts

Once generation is complete, artifacts are available in the project dashboard:
- Browse `Generated Artifacts`
- View versions (each generation creates a new version)
- Compare versions side-by-side
- Export to PDF/PNG/Markdown

## Example Scenarios

### Scenario 1: E-Commerce Platform Design

**Requirements:**
- Multi-tenant support for sellers
- Real-time inventory updates
- Payment processing integration
- Order tracking and analytics

**Constraints:**
- PSD2 compliance
- 99.9% availability SLA
- Under 500ms response time
- PostgreSQL 14+

**Generate:**
```json
{
  "artifactKinds": [1, 2, 3, 5, 6, 7, 9, 10],
  "preferredFormat": 2
}
```

**Expected Output:**
- UseCaseDiagram: Show seller, buyer, admin interactions
- ClassDiagram: Product, Order, Transaction entities
- SequenceDiagram: Checkout flow
- ComponentDiagram: API, Payment Service, Inventory Service
- DeploymentDiagram: Multi-AZ deployment
- ContextDiagram: System + payment gateways + analytics
- Erd: Database schema
- ArchitectureSummary: Patterns, SLA considerations

### Scenario 2: Healthcare Patient Management

**Requirements:**
- Patient records management
- Appointment scheduling
- Prescription tracking
- Doctor-patient messaging

**Constraints:**
- HIPAA compliance
- Audit trail for all operations
- Support 10,000+ concurrent users
- Data residency in-country

**Generate:**
```json
{
  "artifactKinds": [1, 10, 12, 13],
  "preferredFormat": 1
}
```

**Expected Output:**
- UseCaseDiagram: Patients, doctors, admins workflows
- ArchitectureSummary: Layers, security measures for HIPAA
- ApiDesignSuggestion: Patient API endpoints with privacy controls
- DatabaseDesignSuggestion: Encryption, audit tables

### Scenario 3: Real-Time Data Streaming Platform

**Requirements:**
- Ingest 1M+ events per second
- Sub-second latency analysis
- Complex event processing
- Historical data retention (5 years)

**Constraints:**
- Cost optimization for cloud
- Disaster recovery (RTO 4 hours)
- Multi-region deployment

**Generate:**
```json
{
  "artifactKinds": [4, 5, 8, 6, 10, 11],
  "preferredFormat": 2
}
```

**Expected Output:**
- ActivityDiagram: Event processing pipeline
- ComponentDiagram: Kafka, Spark, DuckDB, API components
- DataFlowDiagram: Data ingestion → processing → storage → consumption
- DeploymentDiagram: Multi-region setup
- ArchitectureSummary: Scalability patterns
- ModuleDecomposition: Streaming core, storage, query engine

## Interpreting Generated Content

### Mermaid Diagram Issues

**Missing connections:**
- Check requirements for relationships
- Verify constraint description completeness
- Regenerate with more detail in requirements

**Too many elements:**
- Complex system automatically broken into multiple specialized diagrams
- Use Context Diagram for high-level overview
- Use Component/Activity for detailed workflows

**Generic naming:**
- Update requirements with domain-specific terminology
- Include business context in summary
- Regenerate for domain-aligned artifact

### Markdown Documentation Gaps

**Missing sections:**
- Add constraint details (compliance, performance, scalability)
- Include external system dependencies
- Specify non-functional requirements

**Incomplete content:**
- Expand requirement descriptions
- Add context around business rules
- Include example workflows

## Troubleshooting

### Generation Fails with "No usable design artifacts"

**Cause:** All artifacts failed validation or parsing

**Solution:**
1. Check OpenAI API key is valid
2. Verify requirements are sufficiently detailed
3. Try with fewer artifact kinds (1-2 at a time)
4. Check logs for specific parsing errors

### Generated Diagrams Won't Render

**PlantUml Issues:**
- Missing @startuml/@enduml tags
- Invalid UML syntax (check PlantUML documentation)
- Use Mermaid as fallback format

**Mermaid Issues:**
- Regenerate requesting Mermaid format explicitly
- Check for character encoding issues
- Use web-based Mermaid editor to debug syntax

### API Timeouts

**Cause:** Generation taking too long (large artifact sets)

**Solution:**
- Generate fewer artifacts per request (max 3-5)
- Use Markdown format for documentation (faster than diagrams)
- Request specific diagram types that matter most

## Performance Tips

1. **Start with diagram types** - Faster than documentation
2. **Generate one at a time** - Better error isolation
3. **Use Mermaid over PlantUml** - Faster generation
4. **Cache successful generations** - Don't regenerate unless requirements change
5. **Batch requirements** - More detailed input → better output → fewer iterations

## Cost Optimization

| Artifact Type | Est. Tokens | Relative Cost |
|---|---|---|
| UseCaseDiagram | 800-1200 | Low |
| ClassDiagram | 1000-1500 | Medium |
| SequenceDiagram | 900-1200 | Low |
| ContextDiagram | 600-900 | Low |
| Erd | 800-1200 | Medium |
| ArchitectureSummary | 1500-2500 | High |
| ApiDesignSuggestion | 2000-3500 | High |
| DatabaseDesignSuggestion | 2000-3500 | High |

**Cost-Effective Strategy:**
1. Generate diagrams first (cheaper)
2. Use generated diagrams to inform documentation requests
3. Request documentation with diagram summaries as context

## Next Actions

After generating artifacts:

1. **Review** - Check each artifact for accuracy
2. **Approve/Reject** - Provide feedback (enables future improvements)
3. **Export** - Generate PDF/PNG for documentation
4. **Share** - Distribute to stakeholders via reports
5. **Refine** - Update requirements and regenerate if needed
