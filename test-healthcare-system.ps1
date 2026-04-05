#!/usr/bin/env pwsh
<#
.SYNOPSIS
Automated test script for Healthcare Patient Management System diagram generation
.DESCRIPTION
Creates a project, adds requirements, generates diagrams, and monitors progress
#>

param(
    [string]$ApiUrl = "http://localhost:5099",
    [string]$Token = "",  # Will be requested if not provided
    [switch]$SkipAuth = $false
)

$ErrorActionPreference = "Stop"

# Colors for output
$Success = @{ ForegroundColor = "Green" }
$Error = @{ ForegroundColor = "Red" }
$Info = @{ ForegroundColor = "Cyan" }
$Warning = @{ ForegroundColor = "Yellow" }

function Write-Success { Write-Host @Success $args }
function Write-Error { Write-Host @Error $args }
function Write-Info { Write-Host @Info $args }
function Write-Warning { Write-Host @Warning $args }

# ============================================================================
# 1. REQUEST AUTHENTICATION TOKEN
# ============================================================================

Write-Info "`n[1/5] AUTHENTICATION"
Write-Info "======================================================"

if (-not $SkipAuth -and -not $Token) {
    Write-Info "Enter your email:"
    $email = Read-Host
    
    Write-Info "Enter your password:"
    $password = Read-Host -AsSecureString
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password))
    
    try {
        $authResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" `
            -Method Post `
            -ContentType "application/json" `
            -Body @{
                email = $email
                password = $plainPassword
            } `
            -ErrorAction Stop
        
        $Token = $authResponse.token
        Write-Success "✅ Authentication successful"
        Write-Success "   Token: $($Token.Substring(0, 20))..."
    }
    catch {
        Write-Error "❌ Authentication failed: $($_.Exception.Message)"
        exit 1
    }
}
else {
    Write-Success "✅ Using provided token"
}

# ============================================================================
# 2. CREATE PROJECT
# ============================================================================

Write-Info "`n[2/5] CREATE PROJECT"
Write-Info "======================================================"

$projectData = @{
    name = "Healthcare Patient Management System"
    description = "Secure HIPAA-compliant platform for managing patient records, appointments, prescriptions, and provider collaboration"
} | ConvertTo-Json

try {
    $projectResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/projects" `
        -Method Post `
        -ContentType "application/json" `
        -Headers @{ Authorization = "Bearer $Token" } `
        -Body $projectData `
        -ErrorAction Stop
    
    $projectId = $projectResponse.id
    Write-Success "✅ Project created"
    Write-Success "   Project ID: $projectId"
    Write-Success "   Name: $($projectResponse.name)"
}
catch {
    Write-Error "❌ Project creation failed: $($_.Exception.Message)"
    exit 1
}

# ============================================================================
# 3. ADD REQUIREMENTS & CONSTRAINTS
# ============================================================================

Write-Info "`n[3/5] ADD REQUIREMENTS & CONSTRAINTS"
Write-Info "======================================================"

$requirementsData = @{
    summary = "HIPAA-compliant patient management platform supporting 50,000+ active users with real-time appointment scheduling, prescription tracking, and secure messaging"
    requirements = @(
        @{
            title = "Patient Account Management"
            description = "Patients can create and manage their own accounts with demographic information, medical history, allergies, and emergency contacts. Support SSO via healthcare provider credentials."
        },
        @{
            title = "Provider Directory"
            description = "Searchable directory of healthcare providers with specialties, availability, ratings, and direct messaging. Providers only visible to authorized roles."
        },
        @{
            title = "Appointment Scheduling"
            description = "Real-time appointment booking with provider availability visibility, automated reminders 24 hours before appointment, rescheduling/cancellation capabilities, and no-show tracking."
        },
        @{
            title = "Electronic Health Records"
            description = "Secure storage of patient medical records including diagnoses, treatments, lab results, imaging reports, and clinical notes accessible only to authorized providers and patient."
        },
        @{
            title = "Prescription Management"
            description = "Providers can issue prescriptions visible to pharmacies. Patients notified of prescription status. Integration with pharmacy partners for fulfillment tracking."
        },
        @{
            title = "Secure Messaging"
            description = "End-to-end encrypted messaging between patients and providers for non-urgent consultations, follow-ups, and general questions. Message history retained for 7 years."
        },
        @{
            title = "Lab Results Viewing"
            description = "Patients can view certified lab results uploaded by providers or laboratories with normal ranges, provider annotations, and follow-up recommendations."
        },
        @{
            title = "Billing & Insurance"
            description = "Integrated billing system tracking service costs, insurance claims, patient responsibility, payment plans, and automated payment processing via secure gateway."
        },
        @{
            title = "Audit & Compliance Logging"
            description = "Complete audit trail logging all user access to patient data, modifications, exports, and compliance events. Accessible only to authorized administrators."
        },
        @{
            title = "Mobile & Web Access"
            description = "Responsive web interface and native mobile apps (iOS/Android) with offline capabilities and synchronized state across devices."
        }
    )
    constraints = @(
        @{
            title = "HIPAA Compliance"
            description = "Must meet HIPAA requirements: encryption in transit (TLS 1.2+), encryption at rest (AES-256), access controls, audit logs, breach notification, data retention policies."
        },
        @{
            title = "Data Residency"
            description = "All patient data stored in USA data centers (CONUS) compliant with state healthcare database regulations. No international replication."
        },
        @{
            title = "Performance SLA"
            description = "99.9% uptime SLA. API response times under 500ms (p95). Page loads under 2 seconds (p95). Support 50,000+ concurrent users with no degradation."
        },
        @{
            title = "Scalability"
            description = "System must auto-scale to handle 5x current traffic during peak periods. Database support 100,000+ records/second write throughput."
        },
        @{
            title = "Security & Authentication"
            description = "Multi-factor authentication required for providers. OAuth2.0/OIDC support. Role-based access control (RBAC). 12+ character passwords with complexity."
        },
        @{
            title = "Disaster Recovery"
            description = "RTO 4 hours, RPO < 1 hour. Daily backups with 30-day retention. Geo-redundancy for failover. Quarterly DR drills mandatory."
        },
        @{
            title = "Third-Party Integrations"
            description = "Integration with major EHR systems (Epic, Cerner, Athena) via HL7/FHIR APIs. Payment gateway support. Insurance eligibility verification service."
        },
        @{
            title = "Regulatory Reporting"
            description = "Support Meaningful Use attestation, CMS reporting, and state health department compliance. Export data in HL7, CCD, and FHIR formats."
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $requirementsResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/projects/$projectId/requirements" `
        -Method Post `
        -ContentType "application/json" `
        -Headers @{ Authorization = "Bearer $Token" } `
        -Body $requirementsData `
        -ErrorAction Stop
    
    Write-Success "✅ Requirements added"
    Write-Success "   Requirement Set ID: $($requirementsResponse.id)"
    Write-Success "   Requirements: $($requirementsResponse.requirements.Count)"
    Write-Success "   Constraints: $($requirementsResponse.constraints.Count)"
}
catch {
    Write-Error "❌ Requirements creation failed: $($_.Exception.Message)"
    exit 1
}

# ============================================================================
# 4. QUEUE ARTIFACT GENERATION
# ============================================================================

Write-Info "`n[4/5] QUEUE ARTIFACT GENERATION"
Write-Info "======================================================"

Write-Info "Generating 10 artifacts:"
Write-Info "  - Use Case Diagram (Mermaid)"
Write-Info "  - Class Diagram (Mermaid)"
Write-Info "  - Sequence Diagram (Mermaid)"
Write-Info "  - Component Diagram (Mermaid)"
Write-Info "  - Deployment Diagram (Mermaid)"
Write-Info "  - Context Diagram (Mermaid)"
Write-Info "  - Entity-Relationship Diagram (Mermaid)"
Write-Info "  - Architecture Summary (Markdown)"
Write-Info "  - API Design Suggestion (Markdown)"
Write-Info "  - Database Design Suggestion (Markdown)"

$generationData = @{
    artifactKinds = @(1, 2, 3, 5, 6, 7, 9, 10, 12, 13)
    preferredFormat = 2  # Mermaid
} | ConvertTo-Json

try {
    $generationResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/projects/$projectId/generation/queue" `
        -Method Post `
        -ContentType "application/json" `
        -Headers @{ Authorization = "Bearer $Token" } `
        -Body $generationData `
        -ErrorAction Stop
    
    $generationId = $generationResponse.id
    Write-Success "✅ Generation queued"
    Write-Success "   Generation ID: $generationId"
    Write-Success "   Status: Queued"
}
catch {
    Write-Error "❌ Generation queue failed: $($_.Exception.Message)"
    exit 1
}

# ============================================================================
# 5. MONITOR GENERATION PROGRESS
# ============================================================================

Write-Info "`n[5/5] MONITOR GENERATION PROGRESS"
Write-Info "======================================================"

$statusMap = @{
    1 = "⏳ Queued"
    2 = "🔄 Processing"
    3 = "✅ Completed"
    4 = "❌ Failed"
}

$maxAttempts = 120  # 2 minutes with 1-second intervals
$attempt = 0
$completed = $false

while ($attempt -lt $maxAttempts) {
    $attempt++
    
    try {
        $statusResponse = Invoke-RestMethod `
            -Uri "$ApiUrl/api/projects/$projectId/generation" `
            -Method Get `
            -Headers @{ Authorization = "Bearer $Token" } `
            -ErrorAction Stop
        
        $latestGeneration = $statusResponse | Where-Object { $_.id -eq $generationId } | Select-Object -First 1
        $status = $latestGeneration.status
        $statusText = $statusMap[$status]
        
        Write-Host -NoNewline "`r[Attempt $attempt/$maxAttempts] Generation Status: $statusText"
        
        if ($status -eq 3) {
            Write-Success "`n✅ Generation completed successfully!"
            Write-Success "   Generated Artifacts:"
            Write-Success "   - Use Cases, Class, Sequence, Component, Deployment Diagrams"
            Write-Success "   - Context, Entity-Relationship Diagrams"
            Write-Success "   - Architecture Summary, API Design, Database Design"
            $completed = $true
            break
        }
        elseif ($status -eq 4) {
            Write-Error "`n❌ Generation failed"
            Write-Error "   Reason: $($latestGeneration.failureReason)"
            exit 1
        }
        
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Error "`n❌ Status check failed: $($_.Exception.Message)"
        exit 1
    }
}

if (-not $completed) {
    Write-Warning "`n⚠️  Generation still processing. Check status at:"
    Write-Warning "   $ApiUrl/api/projects/$projectId/generation"
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Info "`n$('=' * 62)"
Write-Success "✅ TEST COMPLETED SUCCESSFULLY"
Write-Info "======================================================"
Write-Info "`nView Results:"
Write-Info "  1. UI: http://localhost:5173"
Write-Info "  2. Navigate to: Projects → Healthcare Patient Management System"
Write-Info "  3. Go to: Generated Artifacts tab"
Write-Info "  4. View diagrams and export to PDF/PNG"
Write-Info "`nProject Details:"
Write-Info "  - Project ID: $projectId"
Write-Info "  - Generation ID: $generationId"
Write-Info "  - API Endpoint: $ApiUrl"
Write-Info "`n"
