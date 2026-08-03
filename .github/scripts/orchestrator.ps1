# Orchestrator Agent - PowerShell Implementation
# Coordinates workflow across all agents: Analysis -> PM -> Dev -> Review

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("start", "status", "advance", "next", "escalate", "dashboard")]
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

    $state | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $StateFile
    Write-Host "[OK] Workflow state initialized for $Sprint" -ForegroundColor Green
}

function Get-WorkflowState {
    if (-not (Test-Path $StateFile)) {
        Write-Host "[ERROR] Workflow state not found. Run 'orchestrator.ps1 -Command start -SprintName <name>'" -ForegroundColor Red
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

    $state | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $StateFile
}

function Start-Workflow {
    param([string]$Sprint)

    Write-Host ""
    Write-Host "Orchestrator Agent - Starting Workflow" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Cyan
    Write-Host ""

    New-WorkflowState -Sprint $Sprint

    Write-Host "Workflow Details:" -ForegroundColor Yellow
    Write-Host "  Sprint: $Sprint"
    Write-Host "  Status: Ready for Analysis Phase"
    Write-Host ""

    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Open Copilot Chat in VS Code"
    Write-Host "  2. Ask Orchestrator Agent: 'Avvia $Sprint con questo brief: ...'"
    Write-Host "  3. Approve or request changes at each phase gate"
    Write-Host "  4. Optional CLI shortcut: orchestrator.ps1 -Command next"
    Write-Host ""
}

function Get-NextPhase {
    param([string]$CurrentPhase)

    switch ($CurrentPhase) {
        "analysis" { return "pm" }
        "pm" { return "development" }
        "development" { return "review" }
        "review" { return "done" }
        default { return "analysis" }
    }
}

function Show-Status {
    $state = Get-WorkflowState

    Write-Host ""
    Write-Host "Workflow Status Dashboard" -ForegroundColor Cyan
    Write-Host "-------------------------" -ForegroundColor Cyan
    Write-Host ""

    Write-Host "Sprint: $($state.sprint)" -ForegroundColor Yellow
    Write-Host "Status: $($state.status)"
    Write-Host "Current Phase: $($state.current_phase)"
    Write-Host ""

    Write-Host "Phase Progress:" -ForegroundColor Yellow
    foreach ($phase in $state.phases.PSObject.Properties.Name) {
        $phaseStatus = $state.phases.$phase.status
        $marker = switch ($phaseStatus) {
            "completed" { "[x]" }
            "in_progress" { "[~]" }
            default { "[ ]" }
        }
        Write-Host "  $marker $phase : $phaseStatus"
    }

    Write-Host ""
    Write-Host "Metrics:" -ForegroundColor Yellow
    Write-Host "  Issues: $($state.issues.created) created, $($state.issues.in_progress) in progress, $($state.issues.completed) completed"
    Write-Host "  PRs: $($state.pull_requests.created) created, $($state.pull_requests.approved) approved, $($state.pull_requests.merged) merged"

    if ($state.blockers.Count -gt 0) {
        Write-Host ""
        Write-Host "Blockers:" -ForegroundColor Red
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
    Write-Host "Advancing Workflow" -ForegroundColor Cyan
    Write-Host "------------------" -ForegroundColor Cyan
    Write-Host ""

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

    Write-Host "[OK] $currentPhase phase completed at $now" -ForegroundColor Green
    Write-Host "[RUNNING] $NextPhase phase now in progress" -ForegroundColor Yellow
    Write-Host ""

    switch ($NextPhase) {
        "pm" {
            Write-Host "PM Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/pm.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide analysis files from $currentPhase phase"
            Write-Host "  4. Create GitHub Issues from user stories"
        }
        "development" {
            Write-Host "Development Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/development.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide GitHub issue list"
            Write-Host "  4. Implement issues with tests (70% plus coverage)"
        }
        "review" {
            Write-Host "Review Agent Instructions:" -ForegroundColor Cyan
            Write-Host "  1. Copy .github/instructions/review.instructions.md"
            Write-Host "  2. Paste into Copilot Chat"
            Write-Host "  3. Provide Pull Request links"
            Write-Host "  4. Verify acceptance criteria and approve"
        }
    }

    Write-Host ""
}

function Advance-WorkflowWithConfirmation {
    $state = Get-WorkflowState
    $currentPhase = $state.current_phase
    $nextPhase = Get-NextPhase -CurrentPhase $currentPhase

    if ($nextPhase -eq "done") {
        Write-Host "" 
        Write-Host "[OK] Workflow completed. No further phases to advance." -ForegroundColor Green
        Write-Host ""
        return
    }

    Write-Host ""
    Write-Host "Phase gate" -ForegroundColor Cyan
    Write-Host "Current phase: $currentPhase"
    Write-Host "Next phase: $nextPhase"

    $confirmation = Read-Host "Proceed to next phase? (y/n)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "[PAUSED] Workflow not advanced." -ForegroundColor Yellow
        Write-Host ""
        return
    }

    Advance-Phase -NextPhase $nextPhase
}

function Escalate-Blocker {
    param([string]$Message)

    $state = Get-WorkflowState

    Write-Host ""
    Write-Host "Escalation Alert" -ForegroundColor Red
    Write-Host "----------------" -ForegroundColor Red
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

    $state.blockers += @{
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
        phase = $state.current_phase
        message = $Message
    }

    $state | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 $StateFile
}

function Show-Dashboard {
    $state = Get-WorkflowState

    $completed = @($state.phases.PSObject.Properties | Where-Object { $_.Value.status -eq "completed" }).Count
    $total = $state.phases.PSObject.Properties.Count
    $progress = [math]::Round(($completed / $total) * 100)

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# $($state.sprint) Workflow Dashboard")
    $lines.Add("")
    $lines.Add("**Status:** Analysis -> PM -> Development -> Review -> Done")
    $lines.Add("")
    $lines.Add("## Current Phase")
    $lines.Add("- **In Progress:** $($state.current_phase)")
    $lines.Add("- **Issues in Progress:** $($state.issues.in_progress)")
    $lines.Add("- **PRs awaiting Review:** $($state.pull_requests.created - $state.pull_requests.approved)")
    $lines.Add("")
    $lines.Add("## Progress Summary")

    foreach ($phase in $state.phases.PSObject.Properties.Name) {
        $phaseStatus = $state.phases.$phase.status
        $icon = if ($phaseStatus -eq "completed") {
            "[x]"
        }
        elseif ($phaseStatus -eq "in_progress") {
            "[~]"
        }
        else {
            "[ ]"
        }

        $lines.Add("- $icon $phase : $phaseStatus")
    }

    $lines.Add("")
    $lines.Add("**Overall Progress:** $progress% complete ($completed/$total phases done)")
    $lines.Add("")
    $lines.Add("## Metrics")
    $lines.Add("- Issues Created: $($state.issues.created)")
    $lines.Add("- Issues In Progress: $($state.issues.in_progress)")
    $lines.Add("- Issues Completed: $($state.issues.completed)")
    $lines.Add("- PRs Created: $($state.pull_requests.created)")
    $lines.Add("- PRs Approved: $($state.pull_requests.approved)")
    $lines.Add("- PRs Merged: $($state.pull_requests.merged)")
    $lines.Add("")
    $lines.Add("## Timeline")
    $lines.Add("- Started: $($state.started_at)")
    $lines.Add("")
    $lines.Add("## Next Steps")
    $lines.Add("1. Continue with $($state.current_phase) phase")
    $lines.Add("2. Use chat-first orchestration with Orchestrator Agent")
    $lines.Add("3. Optional shortcut: orchestrator.ps1 -Command next")
    $lines.Add("4. Check dashboard: orchestrator.ps1 -Command dashboard")

    if ($state.blockers.Count -gt 0) {
        $lines.Add("")
        $lines.Add("## Blockers")
        foreach ($blocker in $state.blockers) {
            $lines.Add("- **$($blocker.phase)**: $($blocker.message) (at $($blocker.timestamp))")
        }
    }

    $dashboardContent = $lines -join [Environment]::NewLine

    Write-Host $dashboardContent

    $dashboardContent | Set-Content -Encoding UTF8 ".github/workflows/PROGRESS_DASHBOARD.md"
    Write-Host ""
    Write-Host "[OK] Dashboard saved to .github/workflows/PROGRESS_DASHBOARD.md" -ForegroundColor Green
}

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
    "next" {
        Advance-WorkflowWithConfirmation
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
