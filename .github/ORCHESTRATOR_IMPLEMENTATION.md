# Orchestrator Agent - Implementation Guide

The Orchestrator Agent is a workflow coordinator that manages the complete development lifecycle: **Analysis → PM → Development → Review → Merged**.

## 📦 What's Included

### 1. **GitHub Actions Workflow** (`.github/workflows/orchestrator.yml`)
Automated workflow triggering and state management (currently manual between phases, auto-managed state)

### 2. **PowerShell Script** (`.github/scripts/orchestrator.ps1`)
Local orchestration tool for tracking progress and managing handoffs

### 3. **Progress Tracking** (`.github/workflows/state/`)
JSON state files tracking workflow status and metrics

### 4. **Handoff Management** (`.github/workflows/handoffs/`)
Structured handoff documents passed between agents

---

## 🚀 Quick Start

### Option A: GitHub Actions (Recommended for CI/CD)

1. **Trigger the workflow** from GitHub Actions tab:
   ```
   Workflow: Orchestrator Agent
   Input: 
     - Sprint name: "Sprint 5"
     - Project brief: "path/to/brief.md" or inline content
     - Phase: "all" (or specific phase)
   ```

2. **Monitor progress** via the workflow run logs

3. **Check dashboard**: View `.github/workflows/PROGRESS_DASHBOARD.md`

### Option B: PowerShell Script (Recommended for Local Development)

#### Start a new workflow:
```powershell
cd .github/scripts
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"
```

**Output:**
```
🎯 Orchestrator Agent - Starting Workflow
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 Workflow Details:
  Sprint: Sprint 5
  Status: Ready for Analysis Phase

🔄 Next Steps:
  1. Open Copilot Chat in VS Code
  2. Copy .github/instructions/analysis.instructions.md
  3. Paste into chat and add your project brief
  4. When complete, run: orchestrator.ps1 -Command advance -Phase pm
```

#### Check workflow status:
```powershell
.\orchestrator.ps1 -Command status
```

**Output:**
```
📊 Workflow Status Dashboard
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Sprint: Sprint 5
Status: initialized
Current Phase: analysis

Phase Progress:
  ⏳ analysis : pending
  ⏳ pm : pending
  ⏳ development : pending
  ⏳ review : pending

Metrics:
  Issues: 0 created, 0 in progress, 0 completed
  PRs: 0 created, 0 approved, 0 merged
```

#### Advance to next phase:
```powershell
.\orchestrator.ps1 -Command advance -Phase pm
```

**Output:**
```
➡️ Advancing Workflow
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ analysis phase completed at 2026-07-26T15:30:00Z
🔄 pm phase now in progress

📋 PM Agent Instructions:
  1. Copy .github/instructions/pm.instructions.md
  2. Paste into Copilot Chat
  3. Provide analysis files from analysis phase
  4. Create GitHub Issues from user stories
```

#### View dashboard:
```powershell
.\orchestrator.ps1 -Command dashboard
```

**Output:**
```
# Sprint 5 Workflow Dashboard

**Status:** Analysis → PM → Development → Review → Done

## Current Phase
- **In Progress:** pm
- **Issues in Progress:** 0
- **PRs awaiting Review:** 0

## Progress Summary
- ✅ analysis : completed
- 🔄 pm : in_progress
- ⏳ development : pending
- ⏳ review : pending

**Overall Progress:** 25% complete (1/4 phases done)
```

#### Escalate a blocker:
```powershell
.\orchestrator.ps1 -Command escalate -Issue "Issue #16 criterion is ambiguous"
```

---

## 📋 Complete Workflow Example

### Day 1: Analysis Phase

1. Start workflow:
```powershell
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"
```

2. Follow Analysis instructions (copy from `.github/instructions/analysis.instructions.md`)

3. Analysis Agent creates:
   - `wiki/sprint-5/functional-analysis-sprint-5.md`
   - `wiki/sprint-5/technical-analysis-sprint-5.md`

4. Advance to PM phase:
```powershell
.\orchestrator.ps1 -Command advance -Phase pm
```

### Day 2: PM Phase

1. Follow PM instructions (copy from `.github/instructions/pm.instructions.md`)

2. PM Agent creates:
   - GitHub Issues #X, #X+1, #X+2, etc.
   - Updates GitHub Project board
   - Creates handoff document

3. Advance to Development phase:
```powershell
.\orchestrator.ps1 -Command advance -Phase development
```

### Days 3-5: Development Phase

1. Follow Development instructions (copy from `.github/instructions/development.instructions.md`)

2. Dev Agent:
   - Claims issues in priority order
   - Creates feature branches
   - Implements with 70%+ test coverage
   - Creates Pull Requests

3. Advance to Review phase:
```powershell
.\orchestrator.ps1 -Command advance -Phase review
```

### Days 5-6: Review Phase

1. Follow Review instructions (copy from `.github/instructions/review.instructions.md`)

2. Review Agent:
   - Reviews each PR
   - Verifies acceptance criteria
   - Approves or requests changes

3. PRs are merged to main

---

## 📊 Workflow State & Progress

### State File Structure (`.github/workflows/state/orchestrator-state.json`)

```json
{
  "sprint": "Sprint 5",
  "started_at": "2026-07-26T15:00:00Z",
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
```

### Handoff Documents (`.github/workflows/handoffs/`)

Each agent receives a handoff document from the previous one:

```markdown
# Handoff: Analysis → PM Agent

**From:** Analysis Agent  
**To:** PM Agent  
**Date:** 2026-07-26

## Artifacts Delivered
- Functional Analysis: wiki/sprint-5/functional-analysis-sprint-5.md
- Technical Analysis: wiki/sprint-5/technical-analysis-sprint-5.md

## User Stories to Create Issues From
1. US 5.1 - [Title]
2. US 5.2 - [Title]
...

## Priority/Dependencies
- US 5.1 has no dependencies (do first)
- US 5.2 depends on 5.1
...

---
**Next Agent:** PM Agent  
**Action:** Create GitHub Issues for each user story  
**Ready?** Yes, proceed automatically.
```

---

## 🎯 Orchestrator Commands Reference

| Command | Usage | Description |
|---------|-------|-------------|
| **start** | `orchestrator.ps1 -Command start -SprintName "Sprint 5"` | Initialize new workflow |
| **status** | `orchestrator.ps1 -Command status` | Show current workflow status |
| **advance** | `orchestrator.ps1 -Command advance -Phase pm` | Move to next phase |
| **escalate** | `orchestrator.ps1 -Command escalate -Issue "description"` | Flag a blocker |
| **dashboard** | `orchestrator.ps1 -Command dashboard` | Generate progress dashboard |

---

## 🔄 Phase Sequence

```
User: "Start Sprint 5"
    ↓
Orchestrator.start()
    ↓
[Manual] Run Analysis Agent (Copilot Chat)
    ↓
Orchestrator.advance(pm)
    ↓
[Manual] Run PM Agent (Copilot Chat)
    ↓
Orchestrator.advance(development)
    ↓
[Manual] Run Development Agent (Copilot Chat)
    ↓
Orchestrator.advance(review)
    ↓
[Manual] Run Review Agent (Copilot Chat)
    ↓
PRs merged to main ✅
```

---

## 📈 Tracking Progress

### Via Dashboard File
View `.github/workflows/PROGRESS_DASHBOARD.md` for:
- Current phase
- Progress percentage
- Issue and PR counts
- Timeline and ETA
- Any blockers

### Via PowerShell
```powershell
# Check overall status
.\orchestrator.ps1 -Command status

# Generate full dashboard
.\orchestrator.ps1 -Command dashboard
```

### Via GitHub Actions
Watch workflow run in GitHub Actions tab for real-time updates

---

## ⚠️ Error Handling & Escalation

### If an agent gets stuck:

```powershell
.\orchestrator.ps1 -Command escalate -Issue "Issue #16 criterion is ambiguous - clarification needed"
```

This will:
1. Flag the blocker in workflow state
2. Display escalation alert
3. Suggest resolution steps
4. Update dashboard with blocker status

### If a phase fails:

1. **Investigate** what went wrong (check agent output)
2. **Fix** the underlying issue (e.g., clarify requirements, fix code)
3. **Resume** by running the phase agent again
4. **Advance** to next phase when ready

---

## 🔧 Customization

### Modify phase names:
Edit `orchestrator.ps1` and change the `phases` hashtable

### Change state location:
Update `$StateDir` and `$HandoffDir` variables in script

### Add custom metrics:
Extend `$state.issues` and `$state.pull_requests` in `New-WorkflowState()`

### Integrate with CI/CD:
Modify `.github/workflows/orchestrator.yml` to add automated triggers

---

## 📝 Best Practices

1. **Run `status` frequently** - Keep yourself informed of current state
2. **Generate dashboard daily** - Share with team for visibility
3. **Escalate early** - Don't let agents get stuck for long
4. **Document handoffs** - Keep handoff files up-to-date
5. **Track blockers** - Use escalate command to flag issues
6. **Commit state** - Push state changes so team can see progress

---

## 🚀 Future Enhancements

- [ ] Automatic agent triggering via GitHub Actions
- [ ] Email notifications on phase completion
- [ ] Slack integration for status updates
- [ ] Web dashboard for progress visualization
- [ ] Parallel workflow support (multiple sprints)
- [ ] Agent retry logic and timeout handling
- [ ] Automated handoff validation

---

## 📚 Related Documentation

- **Agent Framework:** `.github/README.md`
- **Analysis Agent:** `.github/agents/analysis.agent.md` + `.github/instructions/analysis.instructions.md`
- **PM Agent:** `.github/agents/pm.agent.md` + `.github/instructions/pm.instructions.md`
- **Development Agent:** `.github/agents/development.agent.md` + `.github/instructions/development.instructions.md`
- **Review Agent:** `.github/agents/review.agent.md` + `.github/instructions/review.instructions.md`

---

## 💡 Quick Troubleshooting

| Issue | Solution |
|-------|----------|
| **State file not found** | Run `orchestrator.ps1 -Command start` first |
| **Dashboard shows wrong phase** | Run `orchestrator.ps1 -Command dashboard` to regenerate |
| **Need to go back a phase** | Edit `.github/workflows/state/orchestrator-state.json` manually |
| **Lost track of progress** | Run `orchestrator.ps1 -Command status` to see current state |
| **Agent stuck on phase** | Use `escalate` command to flag blocker and get guidance |

---

**Ready to start?** Run: `.\orchestrator.ps1 -Command start -SprintName "Sprint 5"` 🚀
