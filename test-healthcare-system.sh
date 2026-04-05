#!/bin/bash
# Quick curl test script for Healthcare Patient Management System

API_URL="http://localhost:5099"
PROJECT_NAME="Healthcare Patient Management System"

echo "======================================================"
echo "Healthcare System Diagram Generation Test"
echo "======================================================"
echo ""

# ============================================================================
# Helper function to pretty-print JSON
# ============================================================================

pretty_json() {
    echo "$1" | python3 -m json.tool 2>/dev/null || echo "$1"
}

# ============================================================================
# 1. CREATE PROJECT
# ============================================================================

echo "[1/4] Creating project..."

PROJECT_RESPONSE=$(curl -s -X POST "$API_URL/api/projects" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "'"$PROJECT_NAME"'",
    "description": "Secure HIPAA-compliant platform for managing patient records, appointments, prescriptions, and provider collaboration"
  }')

PROJECT_ID=$(echo "$PROJECT_RESPONSE" | jq -r '.id' 2>/dev/null)

if [ -z "$PROJECT_ID" ] || [ "$PROJECT_ID" == "null" ]; then
    echo "❌ Project creation failed"
    echo "Response: $PROJECT_RESPONSE"
    exit 1
fi

echo "✅ Project created: $PROJECT_ID"
echo ""

# ============================================================================
# 2. ADD REQUIREMENTS
# ============================================================================

echo "[2/4] Adding requirements and constraints..."

REQUIREMENTS_RESPONSE=$(curl -s -X POST "$API_URL/api/projects/$PROJECT_ID/requirements" \
  -H "Content-Type: application/json" \
  -d '{
    "summary": "HIPAA-compliant patient management platform supporting 50,000+ active users with appointment scheduling, prescription tracking, and secure messaging",
    "requirements": [
      {
        "title": "Patient Account Management",
        "description": "Patients can create and manage accounts with demographic information, medical history, allergies, and emergency contacts"
      },
      {
        "title": "Appointment Scheduling",
        "description": "Real-time booking with provider availability, automated reminders, rescheduling, and no-show tracking"
      },
      {
        "title": "Electronic Health Records",
        "description": "Secure storage of medical records, diagnoses, treatments, lab results, imaging reports, and clinical notes"
      },
      {
        "title": "Prescription Management",
        "description": "Providers issue prescriptions visible to pharmacies with fulfillment tracking and patient notifications"
      },
      {
        "title": "Secure Messaging",
        "description": "End-to-end encrypted messaging between patients and providers with 7-year retention"
      },
      {
        "title": "Audit Compliance",
        "description": "Complete audit trail logging all user access, modifications, exports for HIPAA compliance"
      }
    ],
    "constraints": [
      {
        "title": "HIPAA Compliance",
        "description": "Encryption in transit (TLS 1.2+), at rest (AES-256), access controls, audit logs, breach notification"
      },
      {
        "title": "Data Residency",
        "description": "All patient data in USA data centers (CONUS) with no international replication"
      },
      {
        "title": "Performance SLA",
        "description": "99.9% uptime, sub-500ms API responses, sub-2s page loads, support 50,000+ concurrent users"
      },
      {
        "title": "Scalability",
        "description": "Auto-scale 5x traffic during peak periods, 100,000+ writes/second database throughput"
      }
    ]
  }')

REQ_ID=$(echo "$REQUIREMENTS_RESPONSE" | jq -r '.id' 2>/dev/null)

if [ -z "$REQ_ID" ] || [ "$REQ_ID" == "null" ]; then
    echo "❌ Requirements creation failed"
    echo "Response: $REQUIREMENTS_RESPONSE"
    exit 1
fi

echo "✅ Requirements added: $REQ_ID"
echo ""

# ============================================================================
# 3. QUEUE GENERATION
# ============================================================================

echo "[3/4] Queuing artifact generation..."

GENERATION_RESPONSE=$(curl -s -X POST "$API_URL/api/projects/$PROJECT_ID/generation/queue" \
  -H "Content-Type: application/json" \
  -d '{
    "artifactKinds": [1, 2, 3, 5, 6, 7, 9, 10, 12, 13],
    "preferredFormat": 2
  }')

GENERATION_ID=$(echo "$GENERATION_RESPONSE" | jq -r '.id' 2>/dev/null)

if [ -z "$GENERATION_ID" ] || [ "$GENERATION_ID" == "null" ]; then
    echo "❌ Generation queue failed"
    echo "Response: $GENERATION_RESPONSE"
    exit 1
fi

echo "✅ Generation queued: $GENERATION_ID"
echo "   Artifacts: UseCaseDiagram, ClassDiagram, SequenceDiagram, ComponentDiagram,"
echo "             DeploymentDiagram, ContextDiagram, Erd, ArchitectureSummary,"
echo "             ApiDesignSuggestion, DatabaseDesignSuggestion"
echo ""

# ============================================================================
# 4. POLL FOR COMPLETION
# ============================================================================

echo "[4/4] Monitoring generation progress..."

STATUS=1
ATTEMPTS=0
MAX_ATTEMPTS=120

while [ $ATTEMPTS -lt $MAX_ATTEMPTS ]; do
    ATTEMPTS=$((ATTEMPTS + 1))
    
    STATUS_RESPONSE=$(curl -s -X GET "$API_URL/api/projects/$PROJECT_ID/generation" \
      -H "Content-Type: application/json")
    
    STATUS=$(echo "$STATUS_RESPONSE" | jq -r ".[0].status" 2>/dev/null)
    
    case $STATUS in
        1)
            STATUS_TEXT="⏳ Queued"
            ;;
        2)
            STATUS_TEXT="🔄 Processing"
            ;;
        3)
            STATUS_TEXT="✅ Completed"
            ;;
        4)
            STATUS_TEXT="❌ Failed"
            ;;
        *)
            STATUS_TEXT="❓ Unknown"
            ;;
    esac
    
    echo -ne "\r[Attempt $ATTEMPTS/$MAX_ATTEMPTS] Status: $STATUS_TEXT"
    
    if [ "$STATUS" == "3" ]; then
        echo ""
        echo ""
        echo "✅ GENERATION COMPLETED SUCCESSFULLY"
        break
    elif [ "$STATUS" == "4" ]; then
        REASON=$(echo "$STATUS_RESPONSE" | jq -r ".[0].failureReason" 2>/dev/null)
        echo ""
        echo "❌ Generation failed: $REASON"
        exit 1
    fi
    
    sleep 1
done

# ============================================================================
# RESULTS
# ============================================================================

echo ""
echo "======================================================"
echo "✅ TEST COMPLETED"
echo "======================================================"
echo ""
echo "View Results:"
echo "  1. Open: http://localhost:5173"
echo "  2. Navigate to: Projects → $PROJECT_NAME"
echo "  3. Click: Generated Artifacts"
echo "  4. View and export diagrams"
echo ""
echo "Project Details:"
echo "  - Project ID: $PROJECT_ID"
echo "  - Requirement ID: $REQ_ID"
echo "  - Generation ID: $GENERATION_ID"
echo ""
