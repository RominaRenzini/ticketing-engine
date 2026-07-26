# Orchestrator Integration Guide

Complete guide to using the Orchestrator Agent with the existing agent framework.

---

## 🎯 Architecture Overview

### The Complete Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                     ORCHESTRATOR AGENT                           │
│  Coordinates workflow, tracks progress, manages handoffs         │
└─────────────────────────────────────────────────────────────────┘
        ↓              ↓              ↓              ↓
   ┌─────────┐   ┌─────────┐   ┌──────────────┐   ┌─────────┐
   │ANALYSIS │   │   PM    │   │ DEVELOPMENT  │   │ REVIEW  │
   │ AGENT   │→→→│ AGENT   │→→→│   AGENT      │→→→│ AGENT   │
   └─────────┘   └─────────┘   └──────────────┘   └─────────┘
        ↓              ↓              ↓              ↓
   Functional &   GitHub         Code +         Approvals
   Technical     Issues          Tests            + Merge
   Analysis
```

### Data Flow

```
User Input (Project Brief)
    ↓
Analysis Agent Creates
  ├── functional-analysis.md
  └── technical-analysis.md
    ↓ (Handoff)
PM Agent Creates
  ├── GitHub Issues (5+)
  └── Project Board
    ↓ (Handoff)
Development Agent Creates
  ├── Feature Branches
  ├── Code Implementation
  ├── Unit Tests (70%+)
  └── Pull Requests
    ↓ (Handoff)
Review Agent Creates
  ├── Review Comments
  ├── Approvals
  └── Merge to Main ✅
```

---

## 📋 Complete File Structure

```
.github/
│
├── 📄 README.md                          ← Framework overview
├── 📄 INDEX.md                           ← Navigation guide
├── 📄 AGENT_TEMPLATE.md                  ← Agent template
├── 📄 INSTRUCTIONS_TEMPLATE.md           ← Instructions template
│
├── 📄 ORCHESTRATOR_IMPLEMENTATION.md     ← Detailed orchestrator docs
├── 📄 ORCHESTRATOR_INTEGRATION.md        ← This file
├── 📄 ORCHESTRATOR_QUICK_START.md        ← 5-minute quickstart
│
├── 📁 agents/
│   ├── analysis.agent.md                 ← What Analysis Agent does
│   ├── pm.agent.md                       ← What PM Agent does
│   ├── development.agent.md              ← What Dev Agent does
│   ├── review.agent.md                   ← What Review Agent does
│   └── orchestrator.agent.md             ← What Orchestrator does
│
├── 📁 instructions/
│   ├── analysis.instructions.md          ← Copy to Copilot (Analysis)
│   ├── pm.instructions.md                ← Copy to Copilot (PM)
│   ├── development.instructions.md       ← Copy to Copilot (Dev)
│   ├── review.instructions.md            ← Copy to Copilot (Review)
│   ├── orchestrator.instructions.md      ← Orchestrator guidance
│   ├── agent-workflow.instructions.md    ← Workflow instructions
│   └── issue-implementation.instructions.md
│
├── 📁 scripts/
│   ├── orchestrator.ps1                  ← PowerShell orchestrator
│   └── orchestrator.sh                   ← Bash orchestrator
│
├── 📁 workflows/
│   ├── orchestrator.yml                  ← GitHub Actions workflow
│   ├── state/
│   │   └── orchestrator-state.json       ← Workflow state (generated)
│   ├── handoffs/
│   │   └── orchestrator-handoff.md       ← Handoff docs (generated)
│   └── PROGRESS_DASHBOARD.md             ← Progress dashboard (generated)
│
└── 📁 skills/                            ← Agent skills (optional)
    ├── analysis/
    │   └── SKILL.md
    ├── development/
    │   └── SKILL.md
    └── documentation/
        └── SKILL.md
```

---

## 🔄 How Agents Work Together

### Phase 1: Analysis

**Triggered by:** Orchestrator (or manually)  
**Instructions:** `.github/instructions/analysis.instructions.md`  
**Agent:** Analysis Agent  

**What it does:**
- Reads project brief (problem, tech stack, constraints)
- Analyzes business requirements
- Creates user stories with acceptance criteria
- Proposes technical architecture
- Documents risks and tradeoffs

**Outputs:**
- `wiki/sprint-X/functional-analysis-sprint-X.md`
- `wiki/sprint-X/technical-analysis-sprint-X.md`

**Handoff:** Orchestrator receives analysis files → Triggers PM Agent

---

### Phase 2: PM

**Triggered by:** Orchestrator (or manually after Analysis)  
**Instructions:** `.github/instructions/pm.instructions.md`  
**Agent:** PM Agent  

**Input from Analysis:**
- Functional analysis (user stories)
- Technical analysis (architecture, constraints)

**What it does:**
- Reads user stories from functional analysis
- Creates GitHub Issues (one per story)
- Adds acceptance criteria as checkboxes
- Links dependencies
- Updates GitHub Project board

**Outputs:**
- 5+ GitHub Issues (properly labeled)
- GitHub Project board with columns
- Priority ordering

**Handoff:** Orchestrator receives issues → Triggers Dev Agent

---

### Phase 3: Development

**Triggered by:** Orchestrator (or manually after PM)  
**Instructions:** `.github/instructions/development.instructions.md`  
**Agent:** Development Agent  

**Input from PM:**
- GitHub Issues (1-5 prioritized)
- Technical context from Analysis phase

**What it does:**
- Claims issues in priority order
- Creates feature branches (`feature/issue-#XXX-...`)
- Implements features following Clean Architecture
- Writes unit tests (70%+ coverage)
- Creates Pull Requests

**Outputs:**
- Feature branches
- Code changes (respecting architecture)
- Test suite (comprehensive coverage)
- Pull Requests (linked to issues)

**Handoff:** Orchestrator receives PRs → Triggers Review Agent

---

### Phase 4: Review

**Triggered by:** Orchestrator (or manually after Dev)  
**Instructions:** `.github/instructions/review.instructions.md`  
**Agent:** Review Agent  

**Input from Dev:**
- Pull Request links
- Acceptance criteria (from issues)
- Code changes with tests

**What it does:**
- Reads each PR thoroughly
- Checks code quality and architecture
- Verifies test coverage
- Checks acceptance criteria
- Approves or requests changes

**Outputs:**
- PR reviews (detailed comments)
- Approvals ✅
- Change requests (if needed)
- Merged PRs to main

**Handoff:** Orchestrator marks workflow complete

---

## 🚀 Running the Complete Workflow

### Quick Start (5 minutes)

```powershell
# Step 1: Start workflow
cd .github/scripts
.\orchestrator.ps1 -Command start -SprintName "Sprint 5"

# Step 2: Check status
.\orchestrator.ps1 -Command status

# Step 3-8: Run agents manually (see below)

# Step 9: Check dashboard
.\orchestrator.ps1 -Command dashboard
```

### Detailed Workflow (Multi-day)

#### Day 1: Analysis Phase (1-2 hours)

1. **Start orchestrator:**
   ```powershell
   .\orchestrator.ps1 -Command start -SprintName "Sprint 5"
   ```

2. **Run Analysis Agent:**
   - Open Copilot Chat in VS Code
   - Copy `.github/instructions/analysis.instructions.md`
   - Paste into chat
   - Add your project brief
   - Ask: "Analyze this feature using these instructions"

3. **Verify outputs:**
   - ✅ `wiki/sprint-5/functional-analysis-sprint-5.md` exists?
   - ✅ `wiki/sprint-5/technical-analysis-sprint-5.md` exists?

4. **Advance to PM phase:**
   ```powershell
   .\orchestrator.ps1 -Command advance -Phase pm
   ```

#### Day 2: PM Phase (1-2 hours)

1. **Run PM Agent:**
   - Copy `.github/instructions/pm.instructions.md`
   - Paste into Copilot Chat
   - Provide links to analysis files
   - Ask: "Create GitHub Issues from this analysis using these instructions"

2. **Verify outputs:**
   - ✅ 5+ GitHub Issues created?
   - ✅ Each issue has acceptance criteria?
   - ✅ Issues linked with dependencies?
   - ✅ GitHub Project board updated?

3. **Advance to Development phase:**
   ```powershell
   .\orchestrator.ps1 -Command advance -Phase development
   ```

#### Days 3-5: Development Phase (3-5 days)

1. **Run Development Agent:**
   - Copy `.github/instructions/development.instructions.md`
   - Paste into Copilot Chat
   - Provide GitHub issue numbers
   - Ask: "Implement these issues using these instructions"

2. **Verify outputs (continuously):**
   - ✅ Feature branches created for each issue?
   - ✅ Code respects Clean Architecture?
   - ✅ Tests are 70%+ coverage?
   - ✅ Pull Requests created?

3. **Check progress:**
   ```powershell
   .\orchestrator.ps1 -Command status
   ```

4. **Advance to Review phase when PRs are ready:**
   ```powershell
   .\orchestrator.ps1 -Command advance -Phase review
   ```

#### Days 5-6: Review Phase (1 day)

1. **Run Review Agent:**
   - Copy `.github/instructions/review.instructions.md`
   - Paste into Copilot Chat
   - Provide PR links
   - Ask: "Review these PRs using these instructions"

2. **Verify outputs:**
   - ✅ All acceptance criteria met?
   - ✅ Code quality is good?
   - ✅ Test coverage sufficient?
   - ✅ PRs approved?

3. **Merge PRs:**
   - Review Agent merges approved PRs to main
   - All issues closed

4. **Check final dashboard:**
   ```powershell
   .\orchestrator.ps1 -Command dashboard
   ```

---

## 📊 Monitoring Progress

### Real-Time Status
```powershell
.\orchestrator.ps1 -Command status
```

Shows:
- Current phase
- Phase statuses (pending/in_progress/completed)
- Issue and PR metrics

### Dashboard
```powershell
.\orchestrator.ps1 -Command dashboard
```

Generates `.github/workflows/PROGRESS_DASHBOARD.md` with:
- Progress percentage
- Timeline and completion estimates
- Metrics (issues, PRs, etc.)
- Blockers (if any)

### State File
Check `.github/workflows/state/orchestrator-state.json` for:
- Exact state data
- Timestamps for each phase
- Blocker history

---

## 🔗 Handoff Protocol

### Analysis → PM Handoff

**What Orchestrator Passes:**
```
{
  "from": "Analysis Agent",
  "to": "PM Agent",
  "artifacts": [
    "wiki/sprint-5/functional-analysis-sprint-5.md",
    "wiki/sprint-5/technical-analysis-sprint-5.md"
  ],
  "user_stories": [
    "US 5.1: ...",
    "US 5.2: ...",
    "..."
  ],
  "dependencies": { /* from technical analysis */ },
  "constraints": { /* from technical analysis */ }
}
```

**PM Agent Action:**
- Create GitHub Issues from user stories
- Add acceptance criteria (from functional analysis)
- Link dependencies
- Update project board

---

### PM → Development Handoff

**What Orchestrator Passes:**
```
{
  "from": "PM Agent",
  "to": "Development Agent",
  "issues": [
    { "number": 15, "title": "...", "priority": 1 },
    { "number": 16, "title": "...", "priority": 2 },
    ...
  ],
  "technical_context": "wiki/sprint-5/technical-analysis-sprint-5.md",
  "dependencies": { /* issue dependencies */ }
}
```

**Dev Agent Action:**
- Claim issues in priority order
- Create feature branches
- Implement code + tests
- Create PRs

---

### Development → Review Handoff

**What Orchestrator Passes:**
```
{
  "from": "Development Agent",
  "to": "Review Agent",
  "pull_requests": [
    { "number": 5, "issue": 15, "url": "..." },
    { "number": 6, "issue": 16, "url": "..." },
    ...
  ],
  "acceptance_criteria": { /* from issues */ },
  "coverage": { "target": "70%", "actual": "75%" }
}
```

**Review Agent Action:**
- Review each PR
- Verify acceptance criteria
- Approve or request changes

---

## ⚠️ Handling Issues

### Phase Gets Stuck?

```powershell
.\orchestrator.ps1 -Command escalate -Issue "Issue #16 criterion is ambiguous"
```

This:
1. Flags the blocker in workflow state
2. Displays escalation alert with guidance
3. Updates dashboard with blocker status

**Resolution:**
1. Review the blocker message
2. Take corrective action (clarify, fix, etc.)
3. Notify the relevant agent
4. Resume when ready

### Need to Go Back?

Edit `.github/workflows/state/orchestrator-state.json`:
- Change `current_phase` to previous phase
- Mark current phase as "pending"

### PRs Need Changes?

When Review Agent requests changes:
1. Dev Agent receives notification
2. Dev Agent fixes issues
3. Pushes new commits
4. Trigger Review Agent again (same PR)

---

## 🎯 Best Practices

### For Orchestrator Operator

1. **Initialize clearly** - Use descriptive sprint names
2. **Monitor continuously** - Check status at least daily
3. **Escalate early** - Don't wait if an agent gets stuck
4. **Document handoffs** - Keep handoff files updated
5. **Commit state** - Push state changes so team sees progress
6. **Communicate** - Share dashboard with team daily

### For Agents

1. **Follow instructions precisely** - Don't skip steps
2. **Provide clear handoffs** - Next agent needs all info
3. **Verify outputs** - Check that required files exist
4. **Test thoroughly** - 70%+ coverage is minimum
5. **Document decisions** - Why was this choice made?

### For Team

1. **Use consistent sprint names** - Makes tracking easier
2. **Review dashboard daily** - Stay informed on progress
3. **Escalate blockers immediately** - Don't work around them
4. **Commit state files** - Share progress with team
5. **Celebrate milestones** - Acknowledge phase completions

---

## 🚀 Advanced: Customization

### Modify Phase Names

Edit orchestrator scripts:
```powershell
# Change from: analysis → pm → development → review
# To: planning → design → implementation → validation
```

### Add Custom Metrics

Extend `.github/workflows/state/orchestrator-state.json`:
```json
{
  "custom_metrics": {
    "code_style_issues": 0,
    "performance_concerns": 0,
    "security_risks": 0
  }
}
```

### Integrate with Slack

Add to orchestrator script:
```powershell
# Send dashboard to Slack when phase completes
Invoke-WebRequest -Uri $SLACK_WEBHOOK -Method Post -Body $DASHBOARD
```

### Automate Entirely (Future)

Replace manual phase advancement with GitHub Actions:
```yaml
# When all PRs merged, automatically mark review as complete
on:
  pull_request:
    types: [closed]
```

---

## 📚 Related Documentation

- **Quick Start:** `.github/ORCHESTRATOR_QUICK_START.md`
- **Full Implementation:** `.github/ORCHESTRATOR_IMPLEMENTATION.md`
- **Agent Framework:** `.github/README.md`
- **Analysis Agent:** `.github/agents/analysis.agent.md`
- **PM Agent:** `.github/agents/pm.agent.md`
- **Development Agent:** `.github/agents/development.agent.md`
- **Review Agent:** `.github/agents/review.agent.md`

---

## ✅ Verification Checklist

After implementing Orchestrator Agent:

- [ ] All script files created (orchestrator.ps1, orchestrator.sh)
- [ ] GitHub Actions workflow created (orchestrator.yml)
- [ ] Documentation complete (IMPLEMENTATION, INTEGRATION, QUICK_START)
- [ ] Tested with test sprint (e.g., "Sprint 5")
- [ ] Dashboard generates correctly
- [ ] State tracking works
- [ ] Handoff documents created
- [ ] Team trained on usage
- [ ] First real sprint started successfully

---

## 🎓 Learning Path

**New to the system?**

1. Read: `.github/README.md` (overview, 10 min)
2. Read: `.github/ORCHESTRATOR_QUICK_START.md` (basics, 5 min)
3. Run: `orchestrator.ps1 -Command start -SprintName "Test"` (1 min)
4. Try: Full workflow with real sprint (3-5 days)
5. Read: `.github/ORCHESTRATOR_IMPLEMENTATION.md` (advanced, 20 min)

**Want to understand a specific agent?**

1. Read: `agents/[agent].agent.md` (what it does)
2. Read: `instructions/[agent].instructions.md` (how Copilot helps)
3. Copy instructions to Copilot Chat
4. Try with real data

---

**Ready to start?** Run: `orchestrator.ps1 -Command start -SprintName "Sprint 5"` 🚀
