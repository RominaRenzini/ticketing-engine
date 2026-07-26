# Getting Started with the Orchestrator Agent

Welcome! This guide helps you get up and running with the ticketing-engine's Agent Framework and Orchestrator implementation.

---

## 🎯 What You'll Learn

By the end of this guide, you'll:
1. Understand how the Orchestrator Agent works
2. Run your first workflow (Analysis → PM → Dev → Review)
3. Track progress and manage handoffs
4. Handle issues and escalations

**Time to complete:** 5 minutes to start, 5 days for full workflow

---

## 🗂️ Finding Your Answer

### "I want to start a workflow NOW"
→ Go to: **[ORCHESTRATOR_QUICK_START.md](ORCHESTRATOR_QUICK_START.md)** (5 min read)

### "I want detailed documentation"
→ Go to: **[ORCHESTRATOR_IMPLEMENTATION.md](ORCHESTRATOR_IMPLEMENTATION.md)** (20 min read)

### "I want to understand how agents work together"
→ Go to: **[ORCHESTRATOR_INTEGRATION.md](ORCHESTRATOR_INTEGRATION.md)** (15 min read)

### "I want to see the framework overview"
→ Go to: **[README.md](README.md)** (10 min read)

### "I need an example project brief"
→ Go to: **[EXAMPLE_PROJECT_BRIEF.md](EXAMPLE_PROJECT_BRIEF.md)** (template)

### "I want to navigate all files"
→ Go to: **[INDEX.md](INDEX.md)** (file index)

---

## 🚀 5-Minute Quick Start

### Step 1: Open Terminal

**Windows:**
```powershell
cd C:\Users\<your-user>\source\repos\ticketing-engine\.github\scripts
```

**Mac/Linux:**
```bash
cd ~/source/repos/ticketing-engine/.github/scripts
```

### Step 2: Start Workflow

**Windows:**
```powershell
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"
```

**Mac/Linux:**
```bash
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

### Step 3: Open Copilot Chat

1. **Open VS Code**
2. **Press:** Ctrl+Shift+I (or Cmd+Shift+I on Mac)
3. **Copy:** Full content of `.github/instructions/analysis.instructions.md`
4. **Paste:** Into Copilot Chat
5. **Add:** Your project brief (problem, tech stack, constraints)
6. **Ask:** "Analyze this feature using these instructions"

### Step 4: Monitor Progress

```powershell
.\orchestrator.ps1 -Command status
```

Shows:
- Current phase
- Phase statuses
- Issues and PR counts

### Step 5: Advance When Ready

Once Analysis completes:
```powershell
.\orchestrator.ps1 -Command advance -Phase pm
```

Then run PM Agent (same process as Analysis).

---

## 📚 Complete Documentation Map

```
.github/
│
├─ 📖 GETTING_STARTED.md (← you are here)
│  └─ Entry point for all users
│
├─ 🚀 ORCHESTRATOR_QUICK_START.md
│  └─ 5-minute quickstart guide
│
├─ 📋 ORCHESTRATOR_IMPLEMENTATION.md
│  └─ Detailed implementation docs
│
├─ 🔗 ORCHESTRATOR_INTEGRATION.md
│  └─ How agents work together
│
├─ 📄 README.md
│  └─ Framework overview
│
├─ 📑 INDEX.md
│  └─ File navigation guide
│
├─ 📝 EXAMPLE_PROJECT_BRIEF.md
│  └─ Template for project briefs
│
├─ scripts/
│  ├─ orchestrator.ps1 (Windows)
│  └─ orchestrator.sh (Mac/Linux)
│
├─ workflows/
│  ├─ orchestrator.yml (GitHub Actions)
│  ├─ state/
│  │  └─ orchestrator-state.json (tracking)
│  ├─ handoffs/
│  │  └─ orchestrator-handoff.md (between agents)
│  └─ PROGRESS_DASHBOARD.md (generated)
│
├─ agents/
│  ├─ orchestrator.agent.md
│  ├─ analysis.agent.md
│  ├─ pm.agent.md
│  ├─ development.agent.md
│  └─ review.agent.md
│
└─ instructions/
   ├─ orchestrator.instructions.md
   ├─ analysis.instructions.md
   ├─ pm.instructions.md
   ├─ development.instructions.md
   └─ review.instructions.md
```

---

## 🎓 Learning Path

### Level 1: First-Time Users (1 hour)

1. **Read:** `ORCHESTRATOR_QUICK_START.md` (5 min)
2. **Read:** `EXAMPLE_PROJECT_BRIEF.md` (5 min)
3. **Run:** `orchestrator.ps1 -Command start` (1 min)
4. **Understand:** How to copy instructions to Copilot (5 min)
5. **Do:** Run Analysis Agent manually (1 hour)

**Result:** Completed first agent workflow ✅

---

### Level 2: Full Workflow (5 days)

1. **Continue** from Level 1
2. **Run** PM Agent (2 hours)
3. **Run** Development Agent (2-3 days)
4. **Run** Review Agent (1 day)
5. **Merge** PRs to main (1 day)

**Result:** Completed full workflow from feature request to merged code ✅

---

### Level 3: Advanced (1 week)

1. **Read:** `ORCHESTRATOR_IMPLEMENTATION.md` (20 min)
2. **Read:** `ORCHESTRATOR_INTEGRATION.md` (15 min)
3. **Understand:** State tracking and handoffs
4. **Customize:** Scripts for your needs
5. **Run:** Multiple sprints
6. **Integrate:** With CI/CD pipeline

**Result:** Mastery of orchestrator system ✅

---

## 💡 Key Concepts

### Phases (4 Sequential Stages)

1. **Analysis** (1-2 hours)
   - Input: Project brief
   - Output: Functional & technical analysis
   - Who: Analysis Agent

2. **PM** (1-2 hours)
   - Input: Analysis files
   - Output: GitHub Issues + board
   - Who: PM Agent

3. **Development** (3-5 days)
   - Input: GitHub Issues
   - Output: Code + PRs
   - Who: Development Agent

4. **Review** (1-2 days)
   - Input: Pull Requests
   - Output: Approvals & merged PRs
   - Who: Review Agent

### Handoffs (Information Passing)

Each agent receives a handoff from the previous agent containing:
- What was completed
- What artifacts were created
- What the next agent should do
- Any constraints or dependencies

### Progress Tracking

Orchestrator maintains:
- **State file** - Current phase and metrics
- **Handoff docs** - Between-agent communication
- **Dashboard** - Progress overview
- **Blockers** - Issues that need escalation

---

## ✅ Your First Workflow Checklist

- [ ] Read `ORCHESTRATOR_QUICK_START.md`
- [ ] Prepare your project brief (or use `EXAMPLE_PROJECT_BRIEF.md`)
- [ ] Run `orchestrator.ps1 -Command start -SprintName "Sprint 5"`
- [ ] Copy `analysis.instructions.md` to Copilot Chat
- [ ] Run Analysis Agent
- [ ] Verify analysis files created
- [ ] Run `orchestrator.ps1 -Command advance -Phase pm`
- [ ] Copy `pm.instructions.md` to Copilot Chat
- [ ] Run PM Agent
- [ ] Verify GitHub Issues created
- [ ] Run `orchestrator.ps1 -Command advance -Phase development`
- [ ] Copy `development.instructions.md` to Copilot Chat
- [ ] Run Development Agent
- [ ] Verify feature branches and PRs created
- [ ] Run `orchestrator.ps1 -Command advance -Phase review`
- [ ] Copy `review.instructions.md` to Copilot Chat
- [ ] Run Review Agent
- [ ] Verify PRs approved and merged
- [ ] Check final dashboard

**Total time:** 1 week for full workflow

---

## 🆘 Troubleshooting

### "Script not found" Error

**Windows:**
```powershell
# Run from .github/scripts directory
cd .github/scripts
.\orchestrator.ps1 -Command start
```

**Mac/Linux:**
```bash
# Make script executable first
chmod +x .github/scripts/orchestrator.sh
./orchestrator.sh start "Sprint 5"
```

### "State file not found" Error

Run `start` command first:
```powershell
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"
```

### "Agent got stuck" Issue

Use escalate command:
```powershell
.\orchestrator.ps1 -Command escalate -Issue "Agent blocked on ambiguous requirement"
```

Then resolve the issue and resume.

### "Need to go back a phase"

Edit `.github/workflows/state/orchestrator-state.json`:
- Change `current_phase` to previous phase
- Mark current phase as `"pending"`

### "Lost track of progress"

Check status:
```powershell
.\orchestrator.ps1 -Command status
```

View dashboard:
```powershell
.\orchestrator.ps1 -Command dashboard
```

---

## 🔗 Quick Links

| Document | Purpose | Read Time |
|----------|---------|-----------|
| [ORCHESTRATOR_QUICK_START.md](ORCHESTRATOR_QUICK_START.md) | Get started fast | 5 min |
| [ORCHESTRATOR_IMPLEMENTATION.md](ORCHESTRATOR_IMPLEMENTATION.md) | Detailed reference | 20 min |
| [ORCHESTRATOR_INTEGRATION.md](ORCHESTRATOR_INTEGRATION.md) | How agents work together | 15 min |
| [README.md](README.md) | Framework overview | 10 min |
| [INDEX.md](INDEX.md) | File navigation | 10 min |
| [EXAMPLE_PROJECT_BRIEF.md](EXAMPLE_PROJECT_BRIEF.md) | Project brief template | 5 min |

---

## 🎯 Next Steps

**Ready to start?**

1. **Copy** your project brief (or use `EXAMPLE_PROJECT_BRIEF.md` as template)
2. **Run** `orchestrator.ps1 -Command start -SprintName "Your Sprint"`
3. **Open** Copilot Chat in VS Code
4. **Copy** `.github/instructions/analysis.instructions.md`
5. **Paste** into chat and add your project brief
6. **Ask:** "Analyze this feature"

**Good luck! 🚀**

---

**Questions?** Check the relevant documentation above or escalate via:
```powershell
.\orchestrator.ps1 -Command escalate -Issue "your question here"
```
