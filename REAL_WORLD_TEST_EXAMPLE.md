# Real-World Test Example: Healthcare Patient Management System

## Project Overview

A modern healthcare platform for managing patient records, appointments, prescriptions, and doctor-patient communications with HIPAA compliance and multi-tenant support.

## Test Scenario

This document walks through creating a project, adding requirements, and generating system design artifacts using the OpenAI diagram generation system.

## Setup & Testing Steps

### Step 1: Create Project

```bash
# Create a new project via API
POST http://localhost:5099/api/projects
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Healthcare Patient Management System",
  "description": "Secure HIPAA-compliant healthcare platform for patient records and provider collaboration"
}
```

**Expected Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Healthcare Patient Management System",
  "createdAtUtc": "2026-04-05T23:00:00Z"
}
```

### Step 2: Add Requirements & Constraints

```bash
# Create a requirement set
POST http://localhost:5099/api/projects/{projectId}/requirements
Content-Type: application/json
Authorization: Bearer {token}

{
  "summary": "HIPAA-compliant patient management platform supporting 50,000+ active users with real-time appointment scheduling, prescription tracking, and secure messaging",
  "requirements": [
    {
      "title": "Patient Account Management",
      "description": "Patients can create and manage their own accounts with demographic information, medical history, allergies, and emergency contacts. Support SSO via healthcare provider credentials."
    },
    {
      "title": "Provider Directory",
      "description": "Searchable directory of healthcare providers (doctors, nurses, specialists) with specialties, availability, ratings, and direct messaging capabilities. Providers only visible to authorized roles."
    },
    {
      "title": "Appointment Scheduling",
      "description": "Real-time appointment booking with provider availability visibility, automated reminders 24 hours before appointment, rescheduling/cancellation capabilities, and no-show tracking."
    },
    {
      "title": "Electronic Health Records (EHR)",
      "description": "Secure storage of patient medical records including diagnoses, treatments, lab results, imaging reports, and clinical notes accessible only to authorized providers and the patient."
    },
    {
      "title": "Prescription Management",
      "description": "Providers can issue prescriptions visible to pharmacies. Patients notified of prescription status. Integration with pharmacy partners for fulfillment tracking."
    },
    {
      "title": "Secure Messaging",
      "description": "End-to-end encrypted messaging between patients and providers for non-urgent consultations, follow-ups, and general questions. Message history retained for 7 years."
    },
    {
      "title": "Lab Results Viewing",
      "description": "Patients can view certified lab results uploaded by providers or laboratories. Results include normal ranges, provider annotations, and follow-up recommendations."
    },
    {
      "title": "Billing & Insurance",
      "description": "Integrated billing system tracking service costs, insurance claims, patient responsibility, payment plans, and automated payment processing via secure payment gateway."
    },
    {
      "title": "Audit & Compliance Logging",
      "description": "Complete audit trail logging all user access to patient data, modifications, exports, and compliance events. Accessible only to authorized administrators and compliance officers."
    },
    {
      "title": "Mobile & Web Access",
      "description": "Responsive web interface and native mobile apps (iOS/Android) with offline capabilities. Synchronized state across devices."
    }
  ],
  "constraints": [
    {
      "title": "HIPAA Compliance",
      "description": "Must meet all HIPAA requirements including encryption in transit (TLS 1.2+), encryption at rest (AES-256), access controls, audit logs, breach notification procedures, and data retention policies."
    },
    {
      "title": "Data Residency",
      "description": "All patient data must be stored in USA data centers (CONUS) compliant with state healthcare database regulations. No data replication to international regions."
    },
    {
      "title": "Performance SLA",
      "description": "99.9% uptime SLA. API response times under 500ms (p95). Page loads under 2 seconds (p95). Support 50,000+ concurrent users with no degradation."
    },
    {
      "title": "Scalability",
      "description": "System must auto-scale to handle 5x current traffic during flu season surge. Database can handle 100,000+ records/second write throughput."
    },
    {
      "title": "Security & Authentication",
      "description": "Multi-factor authentication required for providers. OAuth2.0/OIDC support. Role-based access control (RBAC). Password policy enforcement (12+ chars, complexity)."
    },
    {
      "title": "Disaster Recovery",
      "description": "RTO 4 hours, RPO < 1 hour. Daily backups with 30-day retention. Geo-redundancy for failover. Quarterly DR drills mandatory."
    },
    {
      "title": "Third-Party Integrations",
      "description": "Integration with major EHR systems (Epic, Cerner, Athena) via HL7 v2/v3 or FHIR APIs. Payment gateway support. Insurance eligibility verification service."
    },
    {
      "title": "Regulatory Reporting",
      "description": "Support for Meaningful Use attestation, CMS reporting, and state health department compliance reporting. Export data in HL7, CCD, and FHIR formats."
    }
  ]
}
```

### Step 3: Generate System Design Artifacts

```bash
# Queue complete artifact generation
POST http://localhost:5099/api/projects/{projectId}/generation/queue
Content-Type: application/json
Authorization: Bearer {token}

{
  "artifactKinds": [1, 2, 3, 5, 6, 7, 9, 10, 12, 13],
  "preferredFormat": 2
}
```

**Artifact Kinds Being Generated:**
- 1: UseCaseDiagram
- 2: ClassDiagram
- 3: SequenceDiagram
- 5: ComponentDiagram
- 6: DeploymentDiagram
- 7: ContextDiagram
- 9: EntityRelationshipDiagram
- 10: ArchitectureSummary
- 12: ApiDesignSuggestion
- 13: DatabaseDesignSuggestion

**Expected Response:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440011",
  "requirementSetVersionId": "550e8400-e29b-41d4-a716-446655440001",
  "status": 1,
  "targets": [
    { "artifactKind": 1, "preferredFormat": 2 },
    { "artifactKind": 2, "preferredFormat": 2 },
    ...
  ],
  "createdAtUtc": "2026-04-05T23:05:00Z",
  "completedAtUtc": null
}
```

### Step 4: Check Generation Status

```bash
# Poll for completion
GET http://localhost:5099/api/projects/{projectId}/generation
Authorization: Bearer {token}
```

**Status Progression:**
- Status 1: Queued
- Status 2: Processing (OpenAI generating artifacts)
- Status 3: Completed (all artifacts ready)
- Status 4: Failed (with failure reason)

### Step 5: View Generated Artifacts

Access the UI at http://localhost:5173 and navigate to:
1. **Projects** → Healthcare Patient Management System
2. **Generated Artifacts** tab
3. View each artifact (Use Cases, Class Diagram, Sequence Flow, etc.)
4. Export to PDF/PNG/Markdown

## Expected Artifacts Output

### 1. Use Case Diagram
Shows actors (Patient, Provider, Admin, Pharmacy, Insurance) and their interactions:
- Patient: Register, Schedule Appointment, View Results, Message Provider
- Provider: Manage Schedule, View Patient Records, Issue Prescription, Audit Logs
- Admin: User Management, Compliance Reports, System Settings

### 2. Class Diagram
Key entities:
```
User (base)
├─ Patient
├─ Provider
└─ Administrator

PatientRecord
├─ Appointment
├─ Prescription
├─ LabResult
└─ MedicalHistory

Message (encrypted)
Billing (invoice tracking)
AuditLog (HIPAA compliance)
```

### 3. Sequence Diagram
Primary flow: Patient video appointment booking
1. Patient → Login
2. System → Verify MFA
3. Patient → Browse Providers
4. System → Check Availability
5. Patient → Book Appointment
6. System → Send Reminder (via message queue)
7. Provider → Accept Appointment

### 4. Component Diagram
```
API Gateway
├─ Authentication Service (OAuth2/OIDC)
├─ Patient Service (account, demographics)
├─ Provider Service (specialties, availability)
├─ Appointment Service (scheduling, reminders)
├─ EHR Service (records, integration)
├─ Messaging Service (encrypted chat)
├─ Billing Service (invoicing, payments)
├─ Audit Service (HIPAA logging)
└─ Integration Service (EHR/Pharmacy/Insurance)

Data Layer:
├─ PostgreSQL (patient data, encrypted)
├─ Redis (session cache, message queue)
├─ S3 (document storage, encrypted)
└─ ElasticSearch (audit search)
```

### 5. Deployment Diagram
```
Load Balancer (multi-region)
├─ US-East (primary)
│  ├─ API Servers (3 instances)
│  ├─ Worker Nodes (2 instances)
│  ├─ PostgreSQL (primary, replicated)
│  ├─ Redis Cluster
│  └─ Elasticsearch Nodes
├─ US-West (failover)
│  └─ Standby replicas
└─ DR Region (monthly sync)
```

### 6. Context Diagram
```
    [Patients] ─── [Healthcare Platform] ─── [Pharmacies]
    [Providers]        (Central System)      [Insurance Co]
    [Admins]                                 [EHR Systems]
```

### 7. Entity-Relationship Diagram
Key tables:
- users, patients, providers, administrators
- appointments, prescriptions, lab_results
- medical_history, allergies, medications
- messages (encrypted), billing_invoices, payments
- audit_logs (immutable)

### 8. Architecture Summary
Covers:
- **Pattern**: Layered microservices with event-driven async communication
- **Layers**: API → Business Logic → Data Access → Infrastructure
- **Security**: HIPAA-first design with encryption, audit trails
- **Scalability**: Auto-scaling, caching, database partitioning by patient_id
- **Compliance**: SOC2, HIPAA, state regulations

### 9. API Design
REST API endpoints:
```
/api/v1/patients/{id}                    GET, PUT, DELETE
/api/v1/providers/{id}/availability      GET
/api/v1/appointments                     GET, POST
/api/v1/appointments/{id}                PUT, DELETE
/api/v1/patients/{id}/records            GET
/api/v1/messages/{id}                    GET, POST
/api/v1/billing/invoices                 GET
/api/v1/audit/logs                       GET (admin only)
```

### 10. Database Design
PostgreSQL 14+ with:
- Encryption at rest (pgcrypto)
- Row-level security for HIPAA
- Partitioning by patient for performance
- Indexes on common queries
- Audit trigger on all sensitive tables

## Running the Test

### Via curl/Postman:

```bash
# 1. Create project
curl -X POST http://localhost:5099/api/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"name":"Healthcare Patient Management System","description":"..."}'

# 2. Add requirements (save projectId from response)
curl -X POST http://localhost:5099/api/projects/{projectId}/requirements \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d @requirements.json

# 3. Generate artifacts
curl -X POST http://localhost:5099/api/projects/{projectId}/generation/queue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"artifactKinds":[1,2,3,5,6,7,9,10,12,13],"preferredFormat":2}'

# 4. Poll for status
curl -X GET http://localhost:5099/api/projects/{projectId}/generation \
  -H "Authorization: Bearer {token}"

# 5. View generated artifacts in UI
# Open http://localhost:5173 → Projects → Healthcare System → Generated Artifacts
```

### Via UI:

1. Open http://localhost:5173
2. Login/Register
3. Create new project: "Healthcare Patient Management System"
4. Add requirement set with the requirements above
5. Click "Generate Diagrams"
6. Select artifact types: All UML + Documentation
7. Click "Generate"
8. View results in real-time as they appear

## Expected Results

✅ **All 10 artifacts generated successfully**
- 7 diagrams (PlantUml + Mermaid)
- 3 documentation artifacts (Markdown)
- Each reflects healthcare domain specifics
- HIPAA compliance considerations visible
- Scalability patterns documented

✅ **Quality Indicators:**
- Use cases mention HIPAA, encryption, MFA
- Architecture summary addresses compliance, 99.9% SLA
- Database design includes encryption, partitioning
- API design includes security headers
- Diagrams show integration points with external systems

✅ **Performance:**
- Generation completes within 30-60 seconds
- No errors or validation failures
- All formats render correctly (Mermaid/PlantUml)
- Markdown is professionally formatted

## Troubleshooting

If generation fails:

1. **Check OpenAI API key** - Verify OPENAI_API_KEY is set
2. **Check logs** - Review API logs for parsing errors
3. **Check requirements** - Ensure requirements and constraints are clearly described
4. **Try single artifact** - Generate one type at a time to isolate issues
5. **Check token balance** - Ensure OpenAI account has available credits

## Custom Examples to Try

### Example 1: E-Commerce Platform
Focus on: Payment processing, inventory, scalability

### Example 2: SaaS Collaboration Tool
Focus on: Multi-tenancy, real-time sync, data isolation

### Example 3: IoT Sensor Network
Focus on: High-throughput, time-series data, edge computing

### Example 4: Financial Trading System
Focus on: Compliance, audit trails, latency requirements
