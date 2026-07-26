#!/bin/bash

# Orchestrator Agent - Bash Implementation
# Coordinates workflow across all agents: Analysis → PM → Dev → Review

set -e

# Configuration
STATE_DIR=".github/workflows/state"
HANDOFF_DIR=".github/workflows/handoffs"
STATE_FILE="$STATE_DIR/orchestrator-state.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Initialize directories
mkdir -p "$STATE_DIR"
mkdir -p "$HANDOFF_DIR"

# Parse command line arguments
COMMAND="${1:-}"
SPRINT_NAME="${2:-Sprint 5}"
PHASE="${3:-analysis}"
ISSUE="${4:-}"

# Function: Create new workflow state
new_workflow_state() {
    local sprint=$1
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

    cat > "$STATE_FILE" << EOF
{
  "sprint": "$sprint",
  "started_at": "$timestamp",
  "status": "initialized",
  "current_phase": "analysis",
  "phases": {
    "analysis": {
      "status": "pending",
      "completed_at": null
    },
    "pm": {
      "status": "pending",
      "completed_at": null
    },
    "development": {
      "status": "pending",
      "completed_at": null
    },
    "review": {
      "status": "pending",
      "completed_at": null
    }
  },
  "issues": {
    "created": 0,
    "in_progress": 0,
    "completed": 0
  },
  "pull_requests": {
    "created": 0,
    "approved": 0,
    "merged": 0
  },
  "blockers": []
}
EOF

    echo -e "${GREEN}✅ Workflow state initialized for $sprint${NC}"
}

# Function: Get workflow state
get_workflow_state() {
    if [ ! -f "$STATE_FILE" ]; then
        echo -e "${RED}❌ Workflow state not found. Run 'orchestrator.sh start <sprint_name>'${NC}"
        exit 1
    fi
    cat "$STATE_FILE"
}

# Function: Start workflow
start_workflow() {
    local sprint=$1

    echo ""
    echo -e "${CYAN}🎯 Orchestrator Agent - Starting Workflow${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""

    new_workflow_state "$sprint"

    echo -e "${YELLOW}📋 Workflow Details:${NC}"
    echo "  Sprint: $sprint"
    echo "  Status: Ready for Analysis Phase"
    echo ""

    echo -e "${CYAN}🔄 Next Steps:${NC}"
    echo "  1. Open Copilot Chat in VS Code"
    echo "  2. Copy .github/instructions/analysis.instructions.md"
    echo "  3. Paste into chat and add your project brief"
    echo "  4. When complete, run: orchestrator.sh advance pm"
    echo ""
}

# Function: Show status
show_status() {
    local state=$(get_workflow_state)

    echo ""
    echo -e "${CYAN}📊 Workflow Status Dashboard${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""

    local sprint=$(echo "$state" | jq -r '.sprint')
    local status=$(echo "$state" | jq -r '.status')
    local current=$(echo "$state" | jq -r '.current_phase')

    echo "Sprint: $sprint"
    echo "Status: $status"
    echo "Current Phase: $current"
    echo ""

    echo -e "${YELLOW}Phase Progress:${NC}"

    local phases=("analysis" "pm" "development" "review")
    for phase in "${phases[@]}"; do
        local phase_status=$(echo "$state" | jq -r ".phases.$phase.status")
        local emoji="⏳"
        if [ "$phase_status" = "completed" ]; then
            emoji="✅"
        elif [ "$phase_status" = "in_progress" ]; then
            emoji="🔄"
        fi
        echo "  $emoji $phase : $phase_status"
    done

    echo ""
    echo -e "${YELLOW}Metrics:${NC}"
    local issues_created=$(echo "$state" | jq '.issues.created')
    local issues_progress=$(echo "$state" | jq '.issues.in_progress')
    local issues_completed=$(echo "$state" | jq '.issues.completed')
    echo "  Issues: $issues_created created, $issues_progress in progress, $issues_completed completed"

    local prs_created=$(echo "$state" | jq '.pull_requests.created')
    local prs_approved=$(echo "$state" | jq '.pull_requests.approved')
    local prs_merged=$(echo "$state" | jq '.pull_requests.merged')
    echo "  PRs: $prs_created created, $prs_approved approved, $prs_merged merged"

    local blockers=$(echo "$state" | jq '.blockers | length')
    if [ "$blockers" -gt 0 ]; then
        echo ""
        echo -e "${RED}⚠️ Blockers:${NC}"
        echo "$state" | jq -r '.blockers[] | "  - \(.message)"'
    fi

    echo ""
}

# Function: Advance phase
advance_phase() {
    local next_phase=$1
    local state=$(get_workflow_state)
    local current_phase=$(echo "$state" | jq -r '.current_phase')
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

    echo ""
    echo -e "${CYAN}➡️ Advancing Workflow${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""

    # Update state using jq
    echo "$state" | \
        jq --arg ts "$timestamp" --arg phase "$next_phase" \
        ".phases[\"$current_phase\"].status = \"completed\" |
         .phases[\"$current_phase\"].completed_at = \$ts |
         .phases[\$phase].status = \"in_progress\" |
         .current_phase = \$phase" > "${STATE_FILE}.tmp"

    mv "${STATE_FILE}.tmp" "$STATE_FILE"

    echo -e "${GREEN}✅ $current_phase phase completed at $timestamp${NC}"
    echo -e "${YELLOW}🔄 $next_phase phase now in progress${NC}"
    echo ""

    # Show next steps based on phase
    case "$next_phase" in
        pm)
            echo -e "${CYAN}📋 PM Agent Instructions:${NC}"
            echo "  1. Copy .github/instructions/pm.instructions.md"
            echo "  2. Paste into Copilot Chat"
            echo "  3. Provide analysis files from $current_phase phase"
            echo "  4. Create GitHub Issues from user stories"
            ;;
        development)
            echo -e "${CYAN}💻 Development Agent Instructions:${NC}"
            echo "  1. Copy .github/instructions/development.instructions.md"
            echo "  2. Paste into Copilot Chat"
            echo "  3. Provide GitHub issue list"
            echo "  4. Implement issues with tests (70%+ coverage)"
            ;;
        review)
            echo -e "${CYAN}✅ Review Agent Instructions:${NC}"
            echo "  1. Copy .github/instructions/review.instructions.md"
            echo "  2. Paste into Copilot Chat"
            echo "  3. Provide Pull Request links"
            echo "  4. Verify acceptance criteria and approve"
            ;;
    esac

    echo ""
}

# Function: Show dashboard
show_dashboard() {
    local state=$(get_workflow_state)
    local sprint=$(echo "$state" | jq -r '.sprint')
    local current_phase=$(echo "$state" | jq -r '.current_phase')
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

    # Calculate progress
    local completed=0
    for phase in analysis pm development review; do
        if [ "$(echo "$state" | jq -r ".phases.$phase.status")" = "completed" ]; then
            ((completed++))
        fi
    done
    local progress=$((completed * 25))

    # Generate dashboard
    local dashboard=".github/workflows/PROGRESS_DASHBOARD.md"
    cat > "$dashboard" << EOF
# $sprint Workflow Dashboard

**Generated:** $timestamp
**Status:** Analysis → PM → Development → Review → Done

## Current Phase
- **In Progress:** $current_phase
- **Issues in Progress:** $(echo "$state" | jq '.issues.in_progress')
- **PRs awaiting Review:** $(($(echo "$state" | jq '.pull_requests.created') - $(echo "$state" | jq '.pull_requests.approved')))

## Progress Summary
EOF

    # Add phase statuses
    for phase in analysis pm development review; do
        local status=$(echo "$state" | jq -r ".phases.$phase.status")
        local icon="⏳"
        if [ "$status" = "completed" ]; then
            icon="✅"
        elif [ "$status" = "in_progress" ]; then
            icon="🔄"
        fi
        echo "- $icon $phase : $status" >> "$dashboard"
    done

    cat >> "$dashboard" << EOF

**Overall Progress:** $progress% complete ($completed/4 phases done)

## Metrics
- Issues Created: $(echo "$state" | jq '.issues.created')
- Issues In Progress: $(echo "$state" | jq '.issues.in_progress')
- Issues Completed: $(echo "$state" | jq '.issues.completed')
- PRs Created: $(echo "$state" | jq '.pull_requests.created')
- PRs Approved: $(echo "$state" | jq '.pull_requests.approved')
- PRs Merged: $(echo "$state" | jq '.pull_requests.merged')

## Timeline
- Started: $(echo "$state" | jq -r '.started_at')
- Last Updated: $timestamp

## Next Steps
1. Continue with $current_phase phase
2. When complete, run: orchestrator.sh advance <next_phase>
3. Check dashboard: orchestrator.sh dashboard
EOF

    # Add blockers if any
    local blockers=$(echo "$state" | jq '.blockers | length')
    if [ "$blockers" -gt 0 ]; then
        echo "" >> "$dashboard"
        echo "## ⚠️ Blockers" >> "$dashboard"
        echo "$state" | jq -r '.blockers[] | "\n- **\(.phase)**: \(.message) (at \(.timestamp))"' >> "$dashboard"
    fi

    cat "$dashboard"
    echo -e "${GREEN}📊 Dashboard saved to $dashboard${NC}"
}

# Function: Escalate blocker
escalate_blocker() {
    local message=$1
    local state=$(get_workflow_state)
    local current_phase=$(echo "$state" | jq -r '.current_phase')
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

    echo ""
    echo -e "${RED}⚠️ Escalation Alert${NC}"
    echo -e "${RED}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""

    echo "Phase: $current_phase"
    echo "Issue: $message"
    echo "Time: $(date)"
    echo ""

    echo -e "${YELLOW}Action Required:${NC}"
    echo "  1. Review the issue description above"
    echo "  2. Take corrective action (clarify requirements, fix code, etc.)"
    echo "  3. Update the relevant agent with clarification"
    echo "  4. Resume workflow when ready"
    echo ""

    # Add to blockers
    echo "$state" | \
        jq --arg msg "$message" --arg phase "$current_phase" --arg ts "$timestamp" \
        '.blockers += [{timestamp: $ts, phase: $phase, message: $msg}]' > "${STATE_FILE}.tmp"

    mv "${STATE_FILE}.tmp" "$STATE_FILE"
}

# Main command dispatch
case "$COMMAND" in
    start)
        start_workflow "$SPRINT_NAME"
        ;;
    status)
        show_status
        ;;
    advance)
        advance_phase "$PHASE"
        ;;
    dashboard)
        show_dashboard
        ;;
    escalate)
        escalate_blocker "$ISSUE"
        ;;
    *)
        echo "Usage: orchestrator.sh <command> [options]"
        echo ""
        echo "Commands:"
        echo "  start <sprint_name>          Start a new workflow"
        echo "  status                       Show current status"
        echo "  advance <phase>              Move to next phase (pm, development, review)"
        echo "  dashboard                    Generate progress dashboard"
        echo "  escalate <message>           Flag a blocker"
        echo ""
        echo "Example:"
        echo "  ./orchestrator.sh start \"Sprint 5\""
        echo "  ./orchestrator.sh status"
        echo "  ./orchestrator.sh advance pm"
        exit 1
        ;;
esac
