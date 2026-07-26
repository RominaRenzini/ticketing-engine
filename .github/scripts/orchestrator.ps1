# Orchestrator Agent - PowerShell Implementation
# Coordinates workflow across all agents: Analysis → PM → Dev → Review

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("start", "status", "advance", "escalate", "dashboard")]
    [string]$Command,

    [Parameter(Mandatory = $false)]
    [string]$SprintName = "Sprint 5",

    [Parameter(Mandatory = $false)]
    [string]$Phase = "analysis",

    [Parameter(Mandatory = $false)]
    [string]$Issue = ""
)

$StateDir = ".github/workflows/state"
$HandoffDir = ".github/workflows/handoffs"
$StateFile = "$StateDir/orchestrator-state.json"

# Initialize directories
if (-not (Test-Path $StateDir)) {
    New-Item -ItemType Directory -Path $StateDir -Force | Out-Null
}
if (-not (Test-Path $HandoffDir)) {
    New-Item -ItemType Directory -Path $HandoffDir -Force | Out-Null
}

function New-WorkflowState {
    param([string]$Sprint)

    $state = @{
        sprint = $Sprint
        started_at = (Get-Date).ToUniversalTime().ToString("o")
        status = "initialized"
        current_phase = "analysis"
        phases = @{
            analysis = @{ status = "pending"; completed_at = $null }
            pm = @{ status = "pending"; completed_at = $null }
            development = @{ status = "pending"; completed_at = $null }
            review = @{ status = "pending"; completed_at = $null }
        }
        issues = @{
            created = 0
            in_progress = 0
            completed = 0
        }
        pull_requests = @{
            created = 0
            approved = 0
            merged = 0
        }
        blockers = @()
    }

    $state | ConvertTo-Json -Depth 10 | Set-Content $StateFile
    Write-Host "✅ Workflow state initialized for $Sprint" -ForegroundColor Green
}

function Get-WorkflowState {
    if (-not (Test-Path $StateFile)) {
        Write-Host "❌ Workflow state not found. Run 'orchestrator.ps1 -Command start -SprintName <name>'" -ForegroundColor Red
        exit 1
    }

    Get-Content $StateFile | ConvertFrom-Json
}

function Update-WorkflowState {
    param(
        [hashtable]$Updates
    )

    $state = Get-WorkflowState

    foreach ($key in $Updates.Keys) {
        if ($key -eq "phases") {
            foreach ($phase in $Updates[$key].Keys) {
                $state.phases.$phase = $Updates[$key][$phase]
            }
        }
        elseif ($key -eq "issues" -or $key -eq "pull_requests") {
            foreach ($metric in $Updates[$key].Keys) {
                $state.$key.$metric = $Updates[$key][$metric]
            }
        }
        else {
            $state.$key = $Updates[$key]
        }
    }

    $state | ConvertTo-Json -Depth 10 | Set-Content $StateFile
}

function Start-Workflow {
    param([string]$Sprint)

    Write-Host ""
    Write-Host "🎯 Orchestrator Agent - Starting Workflow" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""

    New-WorkflowState -Sprint $Sprint

    Write-Host "📋 Workflow Details:" -ForegroundColor Yellow
    Write-Host "  Sprint: $Sprint"
    Write-Host "  Status: Ready for Analysis Phase"
    Write-Host ""

    Write-Host "🔄 Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Open Copilot Chat in VS Code"
    Write-Host "  2. Copy .github/instructions/analysis.instructions.md"
    Write-Host "  3. Paste into chat and add your project brief"
    Write-Host "  4. When complete, run: orchestrator.ps1 -Command advance -Phase pm"
    Write-Host ""
}

function Show-Status {
    $state = Get-WorkflowState

    Write-Host ""
    Write-Host "📊 Workflow Status Dashboard" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""

    Write-Host "Sprint: $($state.sprint)" -ForegroundColor Yellow
    Write-Host "Status: $($state.status)"
    Write-Host "Current Phase: $($state.current_phase)"
    Write-Host ""

    Write-Host "Phase Progress:" -ForegroundColor Yellow
    foreach ($phase in $state.phases.PSObject.Properties.Name) {
        $status = $state.phases.$phase.status
        $emoji = switch ($status) {
            "completed" { "✅" }
            "in_progress" { "🔄" }
            default { "⏳" }
        }
        Write-Host "  $emoji $phase : $status"
    }

    Write-Host ""
    Write-Host "Metrics:" -ForegroundColor Yellow
    Write-Host "  Issues: $($state.issues.created) created, $($state.issues.in_progress) in progress, $($state.issues.completed) completed"
    Write-Host "  PRs: $($state.pull_requests.created) created, $($state.pull_requests.approved) approved, $($state.pull_requests.merged) merged"

    if ($state.blockers.Count -gt 0) {
        Write-Host ""
        Write-Host "⚠️ Blockers:" -ForegroundColor Red
        foreach ($blocker in $state.blockers) {
            Write-Host "  - $blocker"
        }
    }

    Write-Host ""
}

function Advance-Phase {
    param([string]$NextPhase)

    $state = Get-WorkflowState
    $currentPhase = $state.current_phase

    Write-Host ""
    Write-Host "➡️ Advancing Workflow" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""

    # Mark current phase as completed
    $now = (Get-Date).ToUniversalTime().ToString("o")
    Update-WorkflowState @{
        phases = @{
            $currentPhase = @{
                status = "completed"
                completed_at = $now
            }
            $NextPhase = @{
                status = "in_progress"
                completed_at = $null
            }
        }
        current_phase = $NextPhase
    }

    Write-Host "✅ $currentPhase phase completed at $now" -ForegroundColor Green
    Write-Host "🔄 $NextPhase phase now in progress" -ForegroundColor Yellow
    Write-Host ""

    # Show next steps based on phase
    switch ($NextPhase) {
        "pm" {
            Write-Host "📋 PM Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/pm.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide analysis files from $currentPhase phase"
            Write-Host "  4. Create GitHub Issues from user stories"
        }
        "development" {
            Write-Host "💻 Development Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/development.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide GitHub issue list"
            Write-Host "  4. Implement issues with tests (70%+ coverage)"
        }
        "review" {
            Write-Host "✅ Review Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/review.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide Pull Request links"
            Write-Host "  4. Verify acceptance criteria and approve"
        }
    }

    Write-Host ""
}

function Escalate-Blocker {
    param([string]$Message)

    $state = Get-WorkflowState

    Write-Host ""
    Write-Host "⚠️ Escalation Alert" -ForegroundColor Red
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Red
    Write-Host ""

    Write-Host "Phase: $($state.current_phase)"
    Write-Host "Issue: $Message"
    Write-Host "Time: $(Get-Date)"
    Write-Host ""

    Write-Host "Action Required:" -ForegroundColor Yellow
    Write-Host "  1. Review the issue description above"
    Write-Host "  2. Take corrective action (clarify requirements, fix code, etc.)"
    Write-Host "  3. Update the relevant agent with clarification"
    Write-Host "  4. Resume workflow when ready"
    Write-Host ""

    # Add to blockers
    $state.blockers += @{
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
        phase = $state.current_phase
        message = $Message
    }

    $state | ConvertTo-Json -Depth 10 | Set-Content $StateFile
}

function Show-Dashboard {
    $state = Get-WorkflowState

    # Calculate progress percentage
    $completed = @($state.phases.PSObject.Properties | Where-Object { $_.Value.status -eq "completed" }).Count
    $total = $state.phases.PSObject.Properties.Count
    $progress = [math]::Round(($completed / $total) * 100)

    $dashboardContent = @"
# $($state.sprint) Workflow Dashboard

**Status:** Analysis → PM → Development → Review → Done

## Current Phase
- **In Progress:** $($state.current_phase)
- **Issues in Progress:** $($state.issues.in_progress)
- **PRs awaiting Review:** $($state.pull_requests.created - $state.pull_requests.approved)

## Progress Summary
"@

    foreach ($phase in $state.phases.PSObject.Properties.Name) {
        $status = $state.phases.$phase.status
        if ($status -eq "completed") { $icon = "✅" }
        elseif ($status -eq "in_progress") { $icon = "🔄" }
        else { $icon = "⏳" }

        $dashboardContent += "`n- $icon $phase : $status"
    }

    $dashboardContent += @"


**Overall Progress:** $progress% complete ($completed/$total phases done)

## Metrics
- Issues Created: $($state.issues.created)
- Issues In Progress: $($state.issues.in_progress)
- Issues Completed: $($state.issues.completed)
- PRs Created: $($state.pull_requests.created)
- PRs Approved: $($state.pull_requests.approved)
- PRs Merged: $($state.pull_requests.merged)

## Timeline
- Started: $($state.started_at)

## Next Steps
1. Continue with $($state.current_phase) phase
2. When complete, run: orchestrator.ps1 -Command advance -Phase <next_phase>
3. Check dashboard: orchestrator.ps1 -Command dashboard
"@

    if ($state.blockers.Count -gt 0) {
        $dashboardContent += "`n`n## ⚠️ Blockers`n"
        foreach ($blocker in $state.blockers) {
            $dashboardContent += "`n- **$($blocker.phase)**: $($blocker.message) (at $($blocker.timestamp))`n"
        }
    }

    Write-Host $dashboardContent

    # Save dashboard
    $dashboardContent | Set-Content ".github/workflows/PROGRESS_DASHBOARD.md"
    Write-Host ""
    Write-Host "📊 Dashboard saved to .github/workflows/PROGRESS_DASHBOARD.md" -ForegroundColor Green
}

# Execute command
switch ($Command) {
    "start" {
        Start-Workflow -Sprint $SprintName
    }
    "status" {
        Show-Status
    }
    "advance" {
        Advance-Phase -NextPhase $Phase
    }
    "escalate" {
        Escalate-Blocker -Message $Issue
    }
    "dashboard" {
        Show-Dashboard
    }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        exit 1
    }
}
