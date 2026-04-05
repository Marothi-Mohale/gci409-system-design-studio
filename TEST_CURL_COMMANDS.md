# Healthcare System Test - Individual Curl Commands
# Copy and paste these commands to test each step individually

# ============================================================================
# SETUP
# ============================================================================

# Set variables (modify as needed)
API_URL=http://localhost:5099
PROJECT_ID=<your-project-id>
GENERATION_ID=<your-generation-id>

# If you have a bearer token, set it:
TOKEN=<your-bearer-token>

# ============================================================================
# 1. CREATE PROJECT
# ============================================================================

curl -X POST "$API_URL/api/projects" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Healthcare Patient Management System",
    "description": "Secure HIPAA-compliant healthcare platform for patient records and provider collaboration"
  }' | jq

# Example Response:
# {
#   "id": "550e8400-e29b-41d4-a716-446655440000",
#   "name": "Healthcare Patient Management System",
#   "createdAtUtc": "2026-04-05T23:00:00Z"
# }
# Save the 'id' value as PROJECT_ID

# ============================================================================
# 2. LIST PROJECTS
# ============================================================================

curl -X GET "$API_URL/api/projects" \
  -H "Authorization: Bearer $TOKEN" | jq

# ============================================================================
# 3. ADD REQUIREMENTS & CONSTRAINTS
# ============================================================================

curl -X POST "$API_URL/api/projects/$PROJECT_ID/requirements" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "summary": "HIPAA-compliant patient management platform supporting 50,000+ active users with appointment scheduling, prescription tracking, and secure messaging",
    "requirements": [
      {
        "title": "Patient Account Management",
        "description": "Patients can create and manage accounts with demographic information, medical history, allergies. Support SSO via healthcare provider credentials."
      },
      {
        "title": "Appointment Scheduling",
        "description": "Real-time booking with provider availability, automated reminders 24h before, rescheduling, cancellation, and no-show tracking."
      },
      {
        "title": "Electronic Health Records",
        "description": "Secure storage of medical records: diagnoses, treatments, lab results, imaging reports, clinical notes accessible to authorized providers and patient."
      },
      {
        "title": "Prescription Management",
        "description": "Providers issue prescriptions visible to pharmacies. Patient notifications. Integration with pharmacy partners for fulfillment tracking."
      },
      {
        "title": "Secure Messaging",
        "description": "End-to-end encrypted messaging between patients and providers for consultations, follow-ups, questions. Message history retained 7 years."
      },
      {
        "title": "Lab Results Viewing",
        "description": "Patients view certified lab results from providers/labs. Includes normal ranges, provider annotations, follow-up recommendations."
      },
      {
        "title": "Billing & Insurance",
        "description": "Track service costs, insurance claims, patient responsibility, payment plans. Automated payment processing via secure gateway."
      },
      {
        "title": "Audit Compliance Logging",
        "description": "Complete audit trail: all user access to patient data, modifications, exports, compliance events. Admins and compliance officers only."
      }
    ],
    "constraints": [
      {
        "title": "HIPAA Compliance",
        "description": "Encryption in transit (TLS 1.2+), at rest (AES-256), access controls, audit logs, breach notification, data retention policies."
      },
      {
        "title": "Data Residency",
        "description": "All patient data in USA data centers (CONUS) compliant with state regulations. No international replication."
      },
      {
        "title": "Performance SLA",
        "description": "99.9% uptime SLA. API under 500ms (p95). Pages under 2s (p95). Support 50,000+ concurrent users with no degradation."
      },
      {
        "title": "Scalability",
        "description": "Auto-scale 5x traffic during peak periods. Database: 100,000+ writes/second throughput."
      },
      {
        "title": "Security & Authentication",
        "description": "MFA required for providers. OAuth2.0/OIDC support. RBAC. Password policy: 12+ chars, complexity required."
      },
      {
        "title": "Disaster Recovery",
        "description": "RTO 4h, RPO <1h. Daily backups (30d retention). Geo-redundancy for failover. Quarterly DR drills mandatory."
      },
      {
        "title": "Third-Party Integrations",
        "description": "Integration with EHR systems (Epic, Cerner, Athena) via HL7/FHIR. Payment gateway. Insurance eligibility verification."
      }
    ]
  }' | jq

# ============================================================================
# 4. QUEUE DIAGRAM GENERATION
# ============================================================================

curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "artifactKinds": [1, 2, 3, 5, 6, 7, 9, 10, 12, 13],
    "preferredFormat": 2
  }' | jq

# Artifact Kinds:
# 1  = UseCaseDiagram
# 2  = ClassDiagram
# 3  = SequenceDiagram
# 4  = ActivityDiagram
# 5  = ComponentDiagram
# 6  = DeploymentDiagram
# 7  = ContextDiagram
# 8  = DataFlowDiagram
# 9  = Erd
# 10 = ArchitectureSummary
# 11 = ModuleDecomposition
# 12 = ApiDesignSuggestion
# 13 = DatabaseDesignSuggestion

# Preferred Formats:
# 1 = Markdown
# 2 = Mermaid
# 3 = PlantUml
# 4 = Pdf
# 5 = Png

# ============================================================================
# 5. CHECK GENERATION STATUS
# ============================================================================

curl -X GET "$API_URL/api/projects/$PROJECT_ID/generation" \
  -H "Authorization: Bearer $TOKEN" | jq

# Status codes:
# 1 = Queued
# 2 = Processing
# 3 = Completed
# 4 = Failed

# ============================================================================
# 6. GET GENERATED ARTIFACTS
# ============================================================================

curl -X GET "$API_URL/api/projects/$PROJECT_ID/artifacts" \
  -H "Authorization: Bearer $TOKEN" | jq

# ============================================================================
# 7. GET SPECIFIC ARTIFACT
# ============================================================================

# First, get artifact ID from above response, then:

curl -X GET "$API_URL/api/artifacts/<artifact-id>" \
  -H "Authorization: Bearer $TOKEN" | jq

# ============================================================================
# 8. EXPORT ARTIFACT
# ============================================================================

# Export to PDF
curl -X POST "$API_URL/api/artifacts/<artifact-id>/exports" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "format": 4
  }' \
  -o artifact-export.pdf

# Export to PNG
curl -X POST "$API_URL/api/artifacts/<artifact-id>/exports" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "format": 5
  }' \
  -o artifact-export.png

# ============================================================================
# TESTING INDIVIDUAL ARTIFACT TYPES
# ============================================================================

# Generate only Use Case Diagram
curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds": [1], "preferredFormat": 2}' | jq

# Generate only Class Diagram
curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds": [2], "preferredFormat": 2}' | jq

# Generate only Sequence Diagram
curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds": [3], "preferredFormat": 2}' | jq

# Generate only Architecture Summary (Markdown)
curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds": [10], "preferredFormat": 1}' | jq

# Generate only API Design Suggestion (Markdown)
curl -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds": [12], "preferredFormat": 1}' | jq

# ============================================================================
# WINDOWS POWERSHELL VERSIONS (if curl is not available)
# ============================================================================

# Create Project (PowerShell)
$body = @{
    name = "Healthcare Patient Management System"
    description = "Secure HIPAA-compliant healthcare platform"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5099/api/projects" `
  -Method Post `
  -Headers @{ Authorization = "Bearer $TOKEN" } `
  -ContentType "application/json" `
  -Body $body | ConvertTo-Json

# Queue Generation (PowerShell)
$body = @{
    artifactKinds = @(1, 2, 3, 5, 6, 7, 9, 10, 12, 13)
    preferredFormat = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5099/api/projects/$PROJECT_ID/generation/queue" `
  -Method Post `
  -Headers @{ Authorization = "Bearer $TOKEN" } `
  -ContentType "application/json" `
  -Body $body | ConvertTo-Json

# ============================================================================
# QUICK REFERENCE
# ============================================================================

# View in browser after generation:
# http://localhost:5173 → Projects → Healthcare System → Generated Artifacts

# Common issues:
# - 401: Unauthorized - check TOKEN
# - 404: Not Found - check PROJECT_ID, GENERATION_ID
# - 400: Bad Request - check JSON format
# - 500: Server Error - check API logs, OpenAI API key
