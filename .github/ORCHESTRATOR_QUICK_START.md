# Orchestrator Agent - Quick Start Guide

Get your workflow running in 5 minutes!

## 🎯 What This Does

The Orchestrator Agent coordinates your entire development workflow:

```
Feature Request
    ↓
Analysis Agent → Creates functional + technical analysis
    ↓
PM Agent → Creates GitHub Issues from analysis
    ↓
Development Agent → Implements issues with tests
    ↓
Review Agent → Reviews PRs and approves
    ↓
Merged to main ✅
```

---

## 🚀 5-Minute Setup

### Step 1: Start a Workflow

**On Windows (PowerShell):**
```powershell
cd .github/scripts
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"
```

**On Mac/Linux (Bash):**
```bash
cd .github/scripts
chmod +x orchestrator.sh
./orchestrator.sh start "Sprint 5"
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

### Step 2: Run Analysis Agent

1. **Open VS Code Copilot Chat** (Ctrl+Shift+I)
2. **Copy** entire content of `.github/instructions/analysis.instructions.md`
3. **Paste** into Copilot Chat
4. **Add your project brief** (problem statement, tech stack, constraints)
5. **Ask:** "Analyze this feature using these instructions"

**Wait:** Analysis Agent creates:
- `wiki/sprint-5/functional-analysis-sprint-5.md`
- `wiki/sprint-5/technical-analysis-sprint-5.md`

### Step 3: Advance to PM Phase

```powershell
.\orchestrator.ps1 -Command advance -Phase pm
```

### Step 4: Run PM Agent

1. **Copy** `.github/instructions/pm.instructions.md`
2. **Paste** into Copilot Chat
3. **Add context** about analysis files
4. **Ask:** "Create GitHub Issues from this analysis using these instructions"

**Wait:** PM Agent creates:
- 5+ GitHub Issues with acceptance criteria
- GitHub Project board

### Step 5: Advance to Development Phase

```powershell
.\orchestrator.ps1 -Command advance -Phase development
```

### Step 6: Run Development Agent

1. **Copy** `.github/instructions/development.instructions.md`
2. **Paste** into Copilot Chat
3. **Provide** GitHub issue numbers
4. **Ask:** "Implement these issues using these instructions"

**Wait:** Dev Agent creates:
- Feature branches
- Code implementation
- Unit tests (70%+ coverage)
- Pull Requests

### Step 7: Advance to Review Phase

```powershell
.\orchestrator.ps1 -Command advance -Phase review
```

### Step 8: Run Review Agent

1. **Copy** `.github/instructions/review.instructions.md`
2. **Paste** into Copilot Chat
3. **Provide** PR links
4. **Ask:** "Review these PRs using these instructions"

**Review Agent:**
- Verifies all acceptance criteria are met
- Approves PRs (or requests changes)

### Step 9: Merge to Main ✅

PRs are merged, workflow complete!

---

## 📊 Track Your Progress

### View Current Status
```powershell
.\orchestrator.ps1 -Command status
```

### View Full Dashboard
```powershell
.\orchestrator.ps1 -Command dashboard
```

Generates: `.github/workflows/PROGRESS_DASHBOARD.md`

---

## ⚠️ If Something Goes Wrong

### Agent Gets Stuck?

```powershell
.\orchestrator.ps1 -Command escalate -Issue "Issue #16 criterion is ambiguous - needs clarification"
```

This will:
1. Flag the blocker
2. Show escalation alert
3. Suggest resolution steps

### Go Back a Phase?

Edit `.github/workflows/state/orchestrator-state.json` manually and change `current_phase`

### Need Help?

Check: `.github/ORCHESTRATOR_IMPLEMENTATION.md` for detailed documentation

---

## 📁 Where Everything Lives

```
.github/
├── workflows/
│   ├── orchestrator.yml                 ← GitHub Actions workflow
│   ├── state/
│   │   └── orchestrator-state.json      ← Workflow state & progress
│   ├── handoffs/
│   │   └── orchestrator-handoff.md      ← Handoff documents
│   └── PROGRESS_DASHBOARD.md            ← Generated progress dashboard
│
├── scripts/
│   ├── orchestrator.ps1                 ← PowerShell orchestrator
│   └── orchestrator.sh                  ← Bash orchestrator
│
├── instructions/
│   ├── analysis.instructions.md         ← Copy to Copilot for analysis
│   ├── pm.instructions.md               ← Copy to Copilot for PM
│   ├── development.instructions.md      ← Copy to Copilot for dev
│   ├── review.instructions.md           ← Copy to Copilot for review
│   └── orchestrator.instructions.md     ← Orchestration guidance
│
├── agents/
│   ├── analysis.agent.md                ← Agent definitions
│   ├── pm.agent.md
│   ├── development.agent.md
│   ├── review.agent.md
│   └── orchestrator.agent.md
│
├── ORCHESTRATOR_IMPLEMENTATION.md       ← Full documentation
└── ORCHESTRATOR_QUICK_START.md         ← This file
```

---

## 🎯 Commands Cheat Sheet

| Command | Usage | Purpose |
|---------|-------|---------|
| **start** | `orchestrator.ps1 -Command start -SprintName "Sprint 5"` | Initialize workflow |
| **status** | `orchestrator.ps1 -Command status` | Check progress |
| **advance** | `orchestrator.ps1 -Command advance -Phase pm` | Move to next phase |
| **dashboard** | `orchestrator.ps1 -Command dashboard` | Generate dashboard |
| **escalate** | `orchestrator.ps1 -Command escalate -Issue "msg"` | Flag blocker |

---

## ✅ Checklist

- [ ] Run `orchestrator.ps1 -Command start -SprintName "Sprint 5"`
- [ ] Read Analysis Agent instructions
- [ ] Open Copilot Chat in VS Code
- [ ] Copy analysis.instructions.md to chat
- [ ] Provide project brief
- [ ] Wait for analysis to complete (1-2 hours)
- [ ] Run `orchestrator.ps1 -Command advance -Phase pm`
- [ ] Run PM Agent via Copilot Chat
- [ ] Create GitHub Issues (1-2 hours)
- [ ] Run `orchestrator.ps1 -Command advance -Phase development`
- [ ] Run Development Agent via Copilot Chat
- [ ] Code + tests complete (3-5 days)
- [ ] Run `orchestrator.ps1 -Command advance -Phase review`
- [ ] Run Review Agent via Copilot Chat
- [ ] PRs approved and merged (1 day)
- [ ] ✅ Sprint complete!

---

## 💡 Pro Tips

1. **Copy instructions frequently** - Each phase has different instructions
2. **Check status often** - Use `status` command between phases
3. **View dashboard daily** - Keeps team aligned on progress
4. **Escalate early** - Don't wait if an agent gets stuck
5. **Commit state changes** - Push state files so team can see progress
6. **Use phase names** - Follow the naming: analysis → pm → development → review

---

## 🤔 Common Questions

**Q: Do I need to use all 5 agents?**  
A: Yes, the sequence is: Analysis → PM → Dev → Review. This ensures quality.

**Q: Can I run multiple sprints in parallel?**  
A: Recommend doing one sprint at a time. Create separate state files if needed.

**Q: What if Analysis takes longer than expected?**  
A: That's OK! Don't advance until analysis files are created. Use `status` to monitor.

**Q: Can I skip a phase?**  
A: Not recommended. Each phase provides critical output for the next.

**Q: What if Review Agent requests changes?**  
A: Dev Agent fixes the issues and pushes new commits. Trigger Review Agent again.

---

## 🚀 Next Steps

1. **Run:** `orchestrator.ps1 -Command start -SprintName "Sprint 5"`
2. **Read:** `.github/instructions/analysis.instructions.md`
3. **Open:** Copilot Chat in VS Code
4. **Copy & Paste:** Instructions into chat
5. **Add:** Your project brief
6. **Ask:** "Analyze this feature"
7. **Wait:** For analysis to complete
8. **Return:** Here and run `advance` command

**Good luck! 🎉**

For detailed documentation, see: `.github/ORCHESTRATOR_IMPLEMENTATION.md`
