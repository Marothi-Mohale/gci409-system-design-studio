# Quick Start - Run Real-World Test Now

## 🚀 Fastest Way to Test (30 seconds)

### Option 1: PowerShell (Windows) - EASIEST

```powershell
cd c:\Users\ASUS\Downloads\gci409
.\test-healthcare-system.ps1
```

**What it does:**
1. Creates Healthcare Patient Management System project
2. Adds 10 requirements + 8 constraints
3. Generates 10 system design artifacts (diagrams + docs)
4. Shows real-time progress
5. Opens UI with results

**Duration:** 30-60 seconds

---

### Option 2: Bash (Linux/Mac)

```bash
cd ~/gci409
chmod +x test-healthcare-system.sh
./test-healthcare-system.sh
```

---

### Option 3: Manual Steps (No Script)

#### Step 1: Authenticate
```bash
# Get your bearer token (save this)
curl -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}' | jq .token
```

#### Step 2: Create Project
```bash
TOKEN="<paste-token-here>"

curl -X POST http://localhost:5099/api/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Healthcare Patient Management System","description":"HIPAA-compliant healthcare platform"}' | jq .id

# Save PROJECT_ID from response
```

#### Step 3: Add Requirements
```bash
curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/requirements \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d @- << 'EOF'
{
  "summary": "HIPAA-compliant patient management platform supporting 50,000+ users",
  "requirements": [
    {"title": "Patient Account Management", "description": "Patient account with demographics, medical history, allergies"},
    {"title": "Appointment Scheduling", "description": "Real-time booking with reminders and rescheduling"},
    {"title": "Electronic Health Records", "description": "Secure storage of diagnoses, treatments, lab results"},
    {"title": "Prescription Management", "description": "Provider-issued prescriptions with pharmacy integration"},
    {"title": "Secure Messaging", "description": "Encrypted patient-provider messaging with 7-year retention"},
    {"title": "Lab Results", "description": "View certified lab results with normal ranges and annotations"},
    {"title": "Billing & Insurance", "description": "Invoice tracking, insurance claims, payment processing"},
    {"title": "Audit Logging", "description": "Complete HIPAA audit trail of all data access"}
  ],
  "constraints": [
    {"title": "HIPAA Compliance", "description": "TLS 1.2+ encryption, AES-256 at rest, MFA, audit logs"},
    {"title": "Performance SLA", "description": "99.9% uptime, <500ms API responses, <2s page loads"},
    {"title": "Scalability", "description": "Auto-scale 5x traffic, 100,000+ writes/sec database throughput"},
    {"title": "Data Residency", "description": "All data in USA data centers (CONUS) only"}
  ]
}
EOF
```

#### Step 4: Generate Diagrams
```bash
curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/generation/queue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds":[1,2,3,5,6,7,9,10,12,13],"preferredFormat":2}' | jq
```

#### Step 5: Check Status (repeat until done)
```bash
curl -X GET http://localhost:5099/api/projects/$PROJECT_ID/generation \
  -H "Authorization: Bearer $TOKEN" | jq '.[0].status'
# Status: 1=Queued, 2=Processing, 3=Completed, 4=Failed
```

#### Step 6: View in UI
Open http://localhost:5173 → Projects → Healthcare System → Generated Artifacts

---

### Option 4: Web UI (No Terminal)

1. Open http://localhost:5173
2. Click **New Project**
3. Name: "Healthcare Patient Management System"
4. Click **Add Requirements**
5. Add the requirements (copy from REAL_WORLD_TEST_EXAMPLE.md)
6. Click **Generate Diagrams**
7. Select all artifact types
8. Watch real-time progress
9. View results!

---

## 📊 Generated Artifacts

You'll get **10 system design artifacts**:

| # | Type | Format | What it Shows |
|---|------|--------|------|
| 1 | Use Case Diagram | Mermaid | Patient, provider, admin workflows |
| 2 | Class Diagram | Mermaid | User, Appointment, Prescription entities |
| 3 | Sequence Diagram | Mermaid | Patient appointment booking flow |
| 4 | Component Diagram | Mermaid | API Gateway, Auth, Patient, Messaging services |
| 5 | Deployment Diagram | Mermaid | Load balancer, API servers, database, cache |
| 6 | Context Diagram | Mermaid | Healthcare platform with external systems |
| 7 | Entity-Relationship | Mermaid | Database tables and relationships |
| 8 | Architecture Summary | Markdown | Patterns, layers, design decisions, compliance |
| 9 | API Design | Markdown | Endpoints, authentication, error handling |
| 10 | Database Design | Markdown | PostgreSQL schema, indexes, optimization |

---

## ✅ Verification Checklist

Before you start:

- [ ] API Health: `curl http://localhost:5099/health` → "Healthy"
- [ ] Frontend: http://localhost:5173 loads
- [ ] Database: Port 5432 listening
- [ ] OpenAI API key set: `echo $env:OPENAI_API_KEY` (Windows)
- [ ] Have user account or registration ready

---

## 🎯 Expected Results

```
✅ Healthcare Patient Management System ✅
  ├─ 10 requirements added
  ├─ 8 constraints specified
  ├─ 10 artifacts generated
  │  ├─ Use Case Diagram (Actors, use cases, system boundary)
  │  ├─ Class Diagram (Domain entities, relationships)
  │  ├─ Sequence Diagram (Appointment booking flow)
  │  ├─ Component Diagram (Microservices, interfaces)
  │  ├─ Deployment Diagram (Cloud infrastructure)
  │  ├─ Context Diagram (External integrations)
  │  ├─ Entity-Relationship (Database schema)
  │  ├─ Architecture Summary (Patterns, SLA, compliance)
  │  ├─ API Design (Endpoints, auth, error handling)
  │  └─ Database Design (Scalability, optimization)
  └─ Duration: 30-60 seconds
```

---

## 🔍 Monitoring Progress

### Real-time in UI
- Open http://localhost:5173
- Watch status change: Queued → Processing → Completed

### Via API
```bash
curl -X GET http://localhost:5099/api/projects/$PROJECT_ID/generation \
  -H "Authorization: Bearer $TOKEN" | jq '.[] | {status, completedAtUtc}'
```

### Console Output (if running script)
```
[Attempt 1/120] Status: ⏳ Queued
[Attempt 10/120] Status: 🔄 Processing
[Attempt 45/120] Status: ✅ Completed
```

---

## 📁 All Testing Documentation

| File | Purpose |
|------|---------|
| **TESTING_GUIDE.md** | Complete step-by-step guide (this file) |
| **REAL_WORLD_TEST_EXAMPLE.md** | Detailed healthcare scenario with all payloads |
| **TEST_CURL_COMMANDS.md** | Individual curl commands for each operation |
| **test-healthcare-system.ps1** | Automated PowerShell script |
| **test-healthcare-system.sh** | Automated bash script |
| **DIAGRAM_GENERATION.md** | Architecture and integration guide |
| **PROMPT_ENGINEERING_REFERENCE.md** | How each prompt was engineered |

---

## 🐛 Troubleshooting

### Error: 401 Unauthorized
```bash
# Get new token
curl -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"your@email.com","password":"YourPassword123!"}' | jq .token
```

### Error: OpenAI API failure
```bash
# Check API key
echo $env:OPENAI_API_KEY  # Should output your key

# Check API logs for details
# Restart API with: dotnet run --project src/backend/Gci409.Api/Gci409.Api.csproj
```

### Generation never completes
```bash
# Try single artifact
curl -X POST http://localhost:5099/api/projects/$PROJECT_ID/generation/queue \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"artifactKinds":[1],"preferredFormat":2}' | jq
```

---

## 🎉 After Test Success

1. **Download diagrams:**
   - Export as PDF or PNG
   - Share with team

2. **Review artifacts:**
   - Check accuracy to requirements
   - Verify HIPAA compliance in docs
   - Note performance/scalability patterns

3. **Try different scenarios:**
   - E-Commerce (payment, inventory, scalability)
   - SaaS (multi-tenancy, real-time, data isolation)
   - IoT (high-throughput, edge computing)
   - Finance (compliance, audit trails, latency)

4. **Regenerate with changes:**
   - Update requirements
   - Generate new version
   - Compare versions in UI

---

## ⚡ TL;DR

```powershell
# Windows PowerShell - Run this:
cd c:\Users\ASUS\Downloads\gci409
.\test-healthcare-system.ps1

# Then open: http://localhost:5173
# Go to: Projects → Healthcare System → Generated Artifacts
```

**That's it! 🚀**
