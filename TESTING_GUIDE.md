# Complete Testing Guide for Diagram Generation System

## Overview

This guide walks you through testing the OpenAI diagram generation system using a real-world healthcare platform scenario.

## System Requirements

✅ **Services Running (verify before starting):**
- API: http://localhost:5099 (✅ should respond with `Healthy`)
- Frontend: http://localhost:5173 (✅ should load React UI)
- Database: localhost:5432 (✅ PostgreSQL running)
- Worker: Running (✅ background jobs enabled)

**Verify:**
```bash
# Check all services
curl http://localhost:5099/health
# Should return: "Healthy"
```

---

## Option 1: Automated Testing (PowerShell - Recommended)

### Windows PowerShell Script

**Easiest option for Windows users:**

```powershell
# Run the automated test script
cd c:\Users\ASUS\Downloads\gci409
.\test-healthcare-system.ps1
```

**What it does:**
1. Prompts for authentication (email + password)
2. Creates Healthcare Patient Management System project
3. Adds 10 requirements and 8 constraints
4. Queues generation for 10 artifact types
5. Polls for completion (monitors progress)
6. Displays results

**Expected Duration:** 30-60 seconds

**Output:**
```
[1/5] AUTHENTICATION
====================================================
✅ Authentication successful
   Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

[2/5] CREATE PROJECT
====================================================
✅ Project created
   Project ID: 550e8400-e29b-41d4-a716-446655440000

[3/5] ADD REQUIREMENTS & CONSTRAINTS
====================================================
✅ Requirements added
   Requirements: 10
   Constraints: 8

[4/5] QUEUE ARTIFACT GENERATION
====================================================
✅ Generation queued
   Generating 10 artifacts:
   - Use Case Diagram
   - Class Diagram
   - ...

[5/5] MONITOR GENERATION PROGRESS
====================================================
[Attempt 45/120] Generation Status: ✅ Completed

✅ VIEW RESULTS
View Results:
  1. UI: http://localhost:5173
  2. Projects → Healthcare Patient Management System
  3. Generated Artifacts tab
```

---

## Option 2: Shell Script (Linux/Mac)

```bash
# Make executable
chmod +x test-healthcare-system.sh

# Run the test
./test-healthcare-system.sh
```

**Same functionality as PowerShell version**

---

## Option 3: Manual Curl Commands

### 1. Get Authentication Token

First, authenticate and get a bearer token:

```bash
# Register/Login to get token
curl -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "YourPassword123!"
  }' | jq .token

# Save the token
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 2. Create Project

```bash
curl -X POST http://localhost:5099/api/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Healthcare Patient Management System",
    "description": "Secure HIPAA-compliant platform"
  }' | jq

# Save PROJECT_ID from response
PROJECT_ID="550e8400-e29b-41d4-a716-446655440000"
```

### 3. Add Requirements

```bash
curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/requirements \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "summary": "HIPAA-compliant patient management...",
    "requirements": [
      {"title": "Patient Account Management", ...},
      {"title": "Appointment Scheduling", ...},
      ...
    ],
    "constraints": [...]
  }' | jq
```

See **TEST_CURL_COMMANDS.md** for full JSON payloads.

### 4. Generate Diagrams

```bash
# Queue generation for all 10 artifact types
curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/generation/queue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "artifactKinds": [1, 2, 3, 5, 6, 7, 9, 10, 12, 13],
    "preferredFormat": 2
  }' | jq

# Save GENERATION_ID from response
GENERATION_ID="660e8400-e29b-41d4-a716-446655440011"
```

### 5. Monitor Progress

```bash
# Check status (run repeatedly until completed)
curl -X GET http://localhost:5099/api/projects/$PROJECT_ID/generation \
  -H "Authorization: Bearer $TOKEN" | jq

# Status values:
# 1 = Queued
# 2 = Processing
# 3 = Completed
# 4 = Failed
```

### 6. View Results in UI

Open http://localhost:5173 and navigate to:
1. **Projects** tab
2. Click **Healthcare Patient Management System**
3. Go to **Generated Artifacts** tab
4. View each generated artifact

---

## Option 4: Web UI (No Code Required)

### Steps:

1. **Open Frontend:**
   - Navigate to http://localhost:5173

2. **Login/Register:**
   - Create account or login with test credentials

3. **Create Project:**
   - Click "New Project"
   - Name: "Healthcare Patient Management System"
   - Description: (optional)
   - Click "Create"

4. **Add Requirements:**
   - Click "Add Requirements"
   - Enter summary: "HIPAA-compliant patient management..."
   - Add requirements (title + description)
   - Add constraints
   - Click "Save Requirements"

5. **Generate Diagrams:**
   - Click "Generate Diagrams"
   - Select artifact types:
     - ✅ Use Case Diagram
     - ✅ Class Diagram
     - ✅ Sequence Diagram
     - ✅ Component Diagram
     - ✅ Deployment Diagram
     - ✅ Context Diagram
     - ✅ Entity-Relationship Diagram
     - ✅ Architecture Summary
     - ✅ API Design Suggestion
     - ✅ Database Design Suggestion
   - Click "Generate"

6. **View Results:**
   - Monitor progress (status updates in real-time)
   - Once completed, view each artifact
   - Click artifact to expand and view full diagram/documentation
   - Export to PDF/PNG/Markdown

---

## Expected Results

### ✅ Successfully Generated Artifacts

#### 1. Use Case Diagram (Mermaid)
```
Shows actors:          Shows interactions:
- Patient              - Register Account
- Provider             - Schedule Appointment
- Admin                - View Health Records
- Pharmacy             - Send Prescription
- Insurance            - Process Payment
```

#### 2. Class Diagram (Mermaid)
```
Key entities:
- User (base class)
  ├─ Patient
  ├─ Provider
  └─ Administrator

- Appointment
- Prescription
- MedicalRecord
- Message (encrypted)
- AuditLog (HIPAA)
```

#### 3. Sequence Diagram (Mermaid)
```
Typical flow: Patient appointments
1. Patient → Login
2. System → Verify MFA
3. Patient → Search Providers
4. System → Check Availability
5. Patient → Book Appointment
6. System → Send Reminder Email
```

#### 4. Component Diagram (Mermaid)
Shows major services:
- API Gateway
- Authentication Service
- Patient Service
- Provider Service
- Appointment Service
- Messaging Service
- Billing Service
- Integration Service (EHR/Pharmacy/Insurance)

#### 5. Deployment Diagram (Mermaid)
Shows infrastructure:
- Load Balancer
- API Servers (multiple instances)
- Database Primary + Replicas
- Cache Layer
- Storage (S3 for documents)

#### 6. Context Diagram (Mermaid)
```
External Entities → Healthcare Platform ← External Systems
Patients ─────────────────┐
Providers ────────────────┤─── Healthcare Platform ───┬───── Pharmacies
Admins ───────────────────┘     (Central System)       ├───── Insurance
                                                       ├───── EHR Systems
                                                       └───── Labs
```

#### 7. Entity-Relationship Diagram (Mermaid)
Shows database tables:
- users, patients, providers, administrators
- appointments, prescriptions, lab_results
- medical_history, allergies, medications
- encrypted_messages, transactions, audit_logs

#### 8. Architecture Summary (Markdown)
Documents:
- System overview and scope
- Architectural patterns used
- System layers and responsibilities
- Key design decisions (3-5 major)
- Non-functional requirements (performance, security, scalability)
- Compliance considerations

#### 9. API Design Suggestion (Markdown)
Specifies:
- REST API endpoints
- Request/response examples
- Authentication & authorization
- Error handling
- Pagination & filtering

#### 10. Database Design Suggestion (Markdown)
Recommends:
- Database choice (PostgreSQL with justification)
- Schema overview
- Entity descriptions
- Indexes and constraints
- Performance optimization
- Scalability & HA strategy

---

## Troubleshooting

### Issue: Authentication Failed

**Symptom:** `401 Unauthorized`

**Solution:**
```bash
# Check if you have an account
# If not, register first through the UI or API
curl -X POST http://localhost:5099/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "YourPassword123!",
    "name": "Test User"
  }'

# Then login
curl -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "YourPassword123!"
  }' | jq .token
```

### Issue: Generation Failed

**Symptom:** `Status: Failed`, error message about OpenAI

**Solution:**
1. **Check OpenAI API Key:**
   ```bash
   # Verify environment variable is set
   echo $env:OPENAI_API_KEY  # Windows PowerShell
   echo $OPENAI_API_KEY      # Linux/Mac
   ```

2. **Check API Logs:**
   ```bash
   # Look for error messages in API console output
   # Should show OpenAI API call details
   ```

3. **Check Token Balance:**
   - Visit https://platform.openai.com/account/billing
   - Ensure credits available

### Issue: Generation Timeout

**Symptom:** Status stays at "Processing" for >2 minutes

**Solution:**
1. **Check API is responsive:**
   ```bash
   curl http://localhost:5099/health
   ```

2. **Try single artifact:**
   ```bash
   # Instead of all 10, try just one
   curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/generation/queue \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d '{"artifactKinds": [1], "preferredFormat": 2}'
   ```

### Issue: 404 Not Found

**Symptom:** Project or generation ID not found

**Solution:**
- Verify PROJECT_ID is correct
- Check you're using same $TOKEN
- List projects to find correct ID: `GET /api/projects`

---

## Testing Different Scenarios

### Scenario 1: E-Commerce Platform

**Focus Areas:** Payment processing, inventory, scalability

**Artifacts:** All 13 types

### Scenario 2: SaaS Collaboration Tool

**Focus Areas:** Multi-tenancy, real-time sync, data isolation

**Key Requirements:**
- Real-time collaborative editing
- Team member management
- File sharing and versioning
- Integration with popular tools (Slack, etc.)

### Scenario 3: IoT Sensor Network

**Focus Areas:** High-throughput, time-series data, edge computing

**Key Constraints:**
- Sub-second latency
- Device management (1M+ devices)
- Data aggregation and storage

### Scenario 4: Financial Trading System

**Focus Areas:** Compliance, audit trails, latency requirements

**Key Constraints:**
- SEC compliance
- <100ms order execution
- Complete audit trails

---

## Performance Metrics

### Expected Generation Times (per artifact)

| Artifact Type | Tokens | Time |
|---|---|---|
| UseCase Diagram | 1200 | 8-12s |
| Class Diagram | 1500 | 10-15s |
| Sequence Diagram | 1200 | 8-12s |
| Component Diagram | 1400 | 10-14s |
| Deployment Diagram | 1300 | 9-13s |
| Context Diagram | 900 | 6-10s |
| ERD | 1200 | 8-12s |
| Architecture Summary | 2500 | 15-20s |
| API Design | 3000 | 18-25s |
| Database Design | 3000 | 18-25s |

**Total for all 10:** 30-60 seconds (sequential generation)

### Cost Estimate

Assuming gpt-4-turbo pricing (~$0.01 per 1K input tokens):
- **Per full generation set:** ~$0.15 - $0.25
- **Per single artifact:** ~$0.015 - $0.025

---

## Next Steps After Testing

1. **Export and Share:**
   - Download diagrams as PDF/PNG
   - Share with team/stakeholders
   - Include in design documentation

2. **Refine:**
   - Update requirements if needed
   - Regenerate artifacts
   - Compare versions

3. **Implement:**
   - Use architecture as implementation guide
   - Follow API design for endpoint implementation
   - Use database design for schema creation

4. **Iterate:**
   - As requirements change, regenerate
   - Track artifact versions
   - Maintain design documentation

---

## Support & Documentation

- **Quick Start:** DIAGRAM_GENERATION_QUICK_START.md
- **Full Architecture:** DIAGRAM_GENERATION.md
- **Prompt Reference:** PROMPT_ENGINEERING_REFERENCE.md
- **Real-World Example:** REAL_WORLD_TEST_EXAMPLE.md
- **Curl Commands:** TEST_CURL_COMMANDS.md

---

## Status Check

Before running any test, verify:

```bash
# API running
curl http://localhost:5099/health
# Should return: "Healthy"

# Database connected
netstat -ano | findstr "5432"
# Should show LISTENING

# Frontend ready
curl http://localhost:5173
# Should return HTML
```

All green? ✅ You're ready to test!
