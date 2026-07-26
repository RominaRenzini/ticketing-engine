# Agent Framework - File Index

Quick navigation for all agent framework files.

## 📖 Start Here

1. **README.md** - Complete guide to the framework
2. **INDEX.md** (this file) - Quick navigation
3. **AGENT_TEMPLATE.md** - Standard structure for all agents
4. **INSTRUCTIONS_TEMPLATE.md** - Template for Copilot instructions

---

## 🧠 The 5 Agents

### 1️⃣ Analysis Agent
Transforms project ideas into detailed analysis (functional + technical).

- **Definition:** `analysis.agent.md`
  - Purpose, responsibilities, inputs/outputs
  - Process flow (step-by-step)
  - Success criteria and error handling
  
- **Instructions:** `analysis.instructions.md`
  - How to guide Copilot in VS Code
  - Output format and structure
  - Decision logic and patterns
  - Quality checklist

**When to use:** User starts a sprint or feature analysis
**Outputs:** 
- `wiki/sprint-X/functional-analysis-sprint-X.md`
- `wiki/sprint-X/technical-analysis-sprint-X.md`

---

### 2️⃣ PM Agent
Creates GitHub Issues from functional analysis, manages project board.

- **Definition:** `pm.agent.md`
  - Purpose, responsibilities, inputs/outputs
  - Process flow (analyze → create issues → update board)
  - Success criteria and error handling
  
- **Instructions:** `pm.instructions.md`
  - How to guide Copilot in creating issues
  - GitHub Issue template format
  - Board setup and column structure
  - Decision logic for dependencies

**When to use:** After Analysis Agent completes
**Outputs:** 
- GitHub Issues (one per user story)
- GitHub Project Board (Sprint X)
- Handoff document (priority order)

---

### 3️⃣ Development Agent
Implements issues, writes tests, creates Pull Requests.

- **Definition:** `development.agent.md`
  - Purpose, responsibilities, inputs/outputs
  - Process flow (claim issue → implement → test → PR)
  - Success criteria and error handling
  
- **Instructions:** `development.instructions.md`
  - How to guide Copilot in implementation
  - Git branch naming convention
  - Commit message format
  - PR description template
  - Test writing patterns (Arrange-Act-Assert)
  - Code review standards (Clean Architecture)

**When to use:** After PM Agent creates issues
**Outputs:** 
- Feature branches (`feature/issue-#XXX-...`)
- Code changes (respecting Clean Architecture)
- Unit tests (70%+ coverage)
- Pull Requests (linked to issues)

---

### 4️⃣ Review Agent
Reviews PRs, verifies acceptance criteria, approves or requests changes.

- **Definition:** `review.agent.md`
  - Purpose, responsibilities, inputs/outputs
  - Process flow (read PR → verify criteria → approve/request)
  - Success criteria and error handling
  
- **Instructions:** `review.instructions.md`
  - How to guide Copilot in code review
  - PR review comment format
  - Approval vs. "changes requested" decisions
  - Edge case handling (concurrency, performance)
  - Test quality evaluation patterns

**When to use:** After Dev Agent creates Pull Requests
**Outputs:** 
- GitHub PR Review (approve or request changes)
- Review comment (detailed feedback)
- Escalation (if architectural concern)

---

### 5️⃣ Orchestrator Agent
Coordinates all agents, triggers sequentially, tracks progress.

- **Definition:** `orchestrator.agent.md`
  - Purpose, responsibilities, inputs/outputs
  - Workflow coordination (Analysis → PM → Dev → Review)
  - Handoff management and escalation
  
- **Instructions:** `orchestrator.instructions.md`
  - How to guide Copilot in workflow coordination
  - Progress dashboard format
  - Handoff document templates
  - Escalation protocol
  - Common workflow patterns

**When to use:** User starts a new sprint (future automation)
**Outputs:** 
- Progress dashboard (what phase, what's done, ETA)
- Handoff documents (between agents)
- Escalation notices (if blocked)

---

## 🎯 Quick Reference by Task

### I want to analyze a new sprint
→ Read: `analysis.agent.md` + `analysis.instructions.md`
→ Use: Copy `analysis.instructions.md` to Copilot Chat

### I want to create GitHub Issues
→ Read: `pm.agent.md` + `pm.instructions.md`
→ Use: Copy `pm.instructions.md` to Copilot Chat

### I want to implement issues
→ Read: `development.agent.md` + `development.instructions.md`
→ Use: Copy `development.instructions.md` to Copilot Chat

### I want to review code
→ Read: `review.agent.md` + `review.instructions.md`
→ Use: Copy `review.instructions.md` to Copilot Chat

### I want to track progress
→ Read: `orchestrator.agent.md` + `orchestrator.instructions.md`
→ Use: Copy `orchestrator.instructions.md` to Copilot Chat

---

## 📋 File Organization for Your Repo

Recommended structure:

```
.github/
├── agents/                    # Agent definitions
│   ├── analysis.agent.md
│   ├── pm.agent.md
│   ├── development.agent.md
│   ├── review.agent.md
│   └── orchestrator.agent.md
│
├── instructions/              # Copilot instructions
│   ├── analysis.instructions.md
│   ├── pm.instructions.md
│   ├── development.instructions.md
│   ├── review.instructions.md
│   └── orchestrator.instructions.md
│
├── README.md                  # This framework's README
│
└── workflows/                 # (Future: GitHub Actions)
    └── agent-workflow.yml     # Automate agent triggering
```

---

## 🔄 Workflow Sequences

### Full Sprint Workflow
```
Day 1: Analysis Agent
  ├─ Input: Project brief
  ├─ Output: functional-analysis.md + technical-analysis.md
  └─ Handoff: to PM Agent

Day 2: PM Agent
  ├─ Input: Analysis files
  ├─ Output: GitHub Issues + Board
  └─ Handoff: to Dev Agent

Days 3-5: Development Agent
  ├─ Input: GitHub Issues
  ├─ Output: Code + PRs
  └─ Handoff: to Review Agent

Days 5-6: Review Agent
  ├─ Input: Pull Requests
  ├─ Output: Approvals/Feedback
  └─ Result: Merged to main ✅
```

### Quick Issue Fix Workflow
```
Dev Agent (skip Analysis + PM, use existing issue)
  ├─ Input: GitHub Issue #42
  ├─ Output: Code + PR
  └─ Handoff: to Review Agent

Review Agent
  ├─ Input: Pull Request #5
  ├─ Output: Approval
  └─ Result: Merged ✅
```

---

## 🎓 Learning Path

**New to the framework?**

1. Read: `README.md` (overview)
2. Read: `AGENT_TEMPLATE.md` (understand structure)
3. Read: `INSTRUCTIONS_TEMPLATE.md` (understand Copilot guidance)
4. Read: `analysis.agent.md` (first agent in workflow)
5. Try: Copy `analysis.instructions.md` to Copilot, use it for a feature

**Want to understand a specific agent?**

1. Find agent in this INDEX
2. Read: `[agent].agent.md` (what it does)
3. Read: `[agent].instructions.md` (how Copilot helps)
4. Look at: Examples/patterns in instructions
5. Try: Copy instructions to Copilot Chat

---

## 📊 Agent Responsibilities Matrix

| Agent | Reads | Creates | Passes To | Triggers When |
|-------|-------|---------|-----------|---------------|
| **Analysis** | Project brief | Analysis MD files | PM Agent | Manual (user) |
| **PM** | Analysis files | GitHub Issues + Board | Dev Agent | Manual (post-analysis) |
| **Dev** | GitHub Issues | Code + Tests + PRs | Review Agent | Manual (post-issues) |
| **Review** | Pull Requests | Review feedback | Main (merge) | Manual/Auto (PR created) |
| **Orchestrator** | All agents | Progress dashboard | - | Manual (user starts sprint) |

---

## 🚀 Getting Started (5 Minutes)

1. **Copy all files** to `.github/agents/` and `.github/instructions/`
2. **Read:** `README.md` (10 min overview)
3. **Pick an agent:** Start with Analysis
4. **Copy instructions:** Open `analysis.instructions.md`
5. **Open Copilot Chat:** In VS Code/Visual Studio
6. **Paste:** Instructions into chat
7. **Add context:** Your project brief
8. **Ask:** "Analyze this feature using these instructions"
9. **Get:** Functional + technical analysis in minutes

Done! Now hand off to PM Agent for the next step. 🎉

---

## 💡 Pro Tips

- **Keep instructions accessible:** Bookmark the `.instructions.md` files you use most
- **Customize examples:** Update code examples in instructions to match your stack
- **Test workflow:** Try with a small feature first to get comfortable
- **Escalate early:** If an agent is stuck, ask user for clarification
- **Track handoffs:** Keep a log of what each agent produced for reference

---

## 📞 Common Questions

**Q: Do I need to use all 5 agents?**
A: Start with Analysis → PM → Dev → Review. Orchestrator is for future automation.

**Q: Can I customize the agents?**
A: Yes! The `.instructions.md` files are designed to be customized. Update examples, add constraints, etc.

**Q: How do I integrate with GitHub Actions?**
A: That's Phase 2. For now, trigger agents manually in Copilot Chat.

**Q: What if an agent gets stuck?**
A: Escalate to user. Each agent definition has an "Error Handling" section.

**Q: Can multiple people use this simultaneously?**
A: Yes, but coordinate to avoid conflicts (different sprints or features per person).

---

## 📚 File Sizes Reference

| File | Size | Purpose |
|------|------|---------|
| AGENT_TEMPLATE.md | 1.4K | Blueprint for agents |
| INSTRUCTIONS_TEMPLATE.md | 2.3K | Blueprint for instructions |
| analysis.agent.md | 5.1K | Analysis Agent definition |
| analysis.instructions.md | 6.6K | Copilot guidance for analysis |
| pm.agent.md | 6.3K | PM Agent definition |
| pm.instructions.md | 6.7K | Copilot guidance for PM |
| development.agent.md | 6.6K | Dev Agent definition |
| development.instructions.md | 8.8K | Copilot guidance for dev |
| review.agent.md | 6.6K | Review Agent definition |
| review.instructions.md | 8.0K | Copilot guidance for review |
| orchestrator.agent.md | 6.0K | Orchestrator definition |
| orchestrator.instructions.md | 9.4K | Copilot guidance for orchestration |

**Total:** ~74K of structured guidance + documentation

---

## ✅ Checklist for Setup

- [ ] Downloaded all files
- [ ] Read README.md
- [ ] Copied files to `.github/agents/` and `.github/instructions/`
- [ ] Reviewed AGENT_TEMPLATE.md
- [ ] Read analysis.agent.md
- [ ] Opened Copilot Chat in VS Code
- [ ] Copied analysis.instructions.md to Copilot
- [ ] Tested with first analysis request
- [ ] Got functional + technical analysis
- [ ] Ready to move to PM Agent

---

**Next:** Pick an agent and start! 🚀
