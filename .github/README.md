# Ticketing Engine - Agent Framework

**Complete workflow automation for the ticketing-engine development cycle.**

This framework provides 5 autonomous agents that orchestrate the complete development workflow: Analysis → PM (Issues) → Development → Review → Merged.

## 📦 What's Included

### Templates (Blueprints)
- **AGENT_TEMPLATE.md** - Standard definition structure for all agents
- **INSTRUCTIONS_TEMPLATE.md** - Copilot instruction template

### 5 Agents (Agent Definition + Instructions)

1. **Analysis Agent**
   - `analysis.agent.md` - Agent definition and workflow
   - `analysis.instructions.md` - Copilot instructions
   - **Role:** Transforms project ideas into functional + technical analysis
   - **Trigger:** Manual (user initiates sprint analysis)

2. **PM Agent**
   - `pm.agent.md` - Agent definition and workflow
   - `pm.instructions.md` - Copilot instructions
   - **Role:** Creates GitHub Issues from functional analysis, manages board
   - **Trigger:** Manual (after Analysis completes)

3. **Development Agent**
   - `development.agent.md` - Agent definition and workflow
   - `development.instructions.md` - Copilot instructions
   - **Role:** Implements issues, writes tests, creates Pull Requests
   - **Trigger:** Manual (after PM creates issues)

4. **Review Agent**
   - `review.agent.md` - Agent definition and workflow
   - `review.instructions.md` - Copilot instructions
   - **Role:** Reviews PRs, verifies acceptance criteria, approves or requests changes
   - **Trigger:** Manual/Automatic (when PR created)

5. **Orchestrator Agent**
   - `orchestrator.agent.md` - Agent definition and workflow
   - `orchestrator.instructions.md` - Copilot instructions
   - **Role:** Coordinates all agents, triggers sequentially, tracks progress
   - **Trigger:** Manual (user starts sprint)

## 🚀 Workflow Overview

```
User: "Start Sprint 3 Analysis"
    ↓
Analysis Agent
  ├─ Creates: functional-analysis.md
  ├─ Creates: technical-analysis.md
  └─ Hands off to PM Agent
    ↓
PM Agent
  ├─ Creates: GitHub Issues (from user stories)
  ├─ Updates: Project Board
  └─ Hands off to Dev Agent
    ↓
Development Agent
  ├─ Creates: Feature branches
  ├─ Implements: Issues (following acceptance criteria)
  ├─ Writes: Unit tests (70%+ coverage)
  ├─ Creates: Pull Requests
  └─ Hands off to Review Agent
    ↓
Review Agent
  ├─ Verifies: Acceptance criteria met
  ├─ Checks: Code quality + architecture
  ├─ Evaluates: Test coverage
  └─ Approves: PR (or requests changes)
    ↓
Merge to main ✅
```

## 📋 How to Use

### 1. Setup
Copy all files to your `.github/` directory:

```
.github/
├── agents/
│   ├── analysis.agent.md
│   ├── pm.agent.md
│   ├── development.agent.md
│   ├── review.agent.md
│   └── orchestrator.agent.md
│
└── instructions/
    ├── analysis.instructions.md
    ├── pm.instructions.md
    ├── development.instructions.md
    ├── review.instructions.md
    └── orchestrator.instructions.md
```

### 2. Use with Copilot in Visual Studio

**For each agent, copy the corresponding `.instructions.md` into Copilot Chat:**

#### Analysis Agent
1. Open Copilot Chat in VS Code/Visual Studio
2. Copy contents of `analysis.instructions.md`
3. Paste into Copilot Chat
4. Add your project brief (idea + tech stack)
5. Ask: "Analyze this feature using the Analysis Agent instructions"
6. Copilot will create functional-analysis.md and technical-analysis.md

#### PM Agent
1. Copy contents of `pm.instructions.md` into Copilot Chat
2. Provide links to the analysis files created above
3. Ask: "Create GitHub Issues from this analysis using PM Agent instructions"
4. Copilot will generate issue templates ready to post to GitHub

#### Development Agent
1. Copy contents of `development.instructions.md` into Copilot Chat
2. Provide GitHub Issue numbers and priority order
3. Ask: "Implement these issues using Development Agent instructions"
4. Copilot will generate code, tests, and PR descriptions

#### Review Agent
1. Copy contents of `review.instructions.md` into Copilot Chat
2. Paste Pull Request URL and description
3. Ask: "Review this PR using Review Agent instructions"
4. Copilot will analyze code and provide detailed review feedback

#### Orchestrator Agent (for future automation)
1. Copy contents of `orchestrator.instructions.md` into Copilot Chat
2. Ask: "Track progress for Sprint X using Orchestrator instructions"
3. Copilot can maintain a progress dashboard

### 3. Manual Agent Triggering (Current)

Right now, you'll trigger agents manually:

```
User → Analysis Agent → (copy handoff) → PM Agent → (copy handoff) → Dev Agent → ...
```

Each agent receives explicit handoff information from the previous one.

### 4. Future: Automated Agent Triggering

A GitHub Actions workflow can automatically trigger agents:
- Analysis completes → PM Agent triggered
- PM creates issues → Dev Agent triggered
- Dev creates PR → Review Agent triggered

(To be implemented in Phase 2)

## 🎯 Key Concepts

### Agent Definition vs. Instructions
- **Agent Definition** (`.agent.md`): What the agent does, its responsibilities, inputs/outputs
- **Instructions** (`.instructions.md`): How to guide Copilot when acting as that agent

### Handoff Protocol
Each agent creates a **handoff document** for the next agent containing:
- What was completed
- What artifacts were created
- What the next agent should do
- Any blockers or escalations

### Acceptance Criteria
Every GitHub Issue has acceptance criteria (checkboxes). Dev Agent implements to meet these criteria exactly. Review Agent verifies each criterion is met.

## 📝 Example Sprint Flow

### Sprint 3: Reservation Lifecycle Enforcement

```
Day 1 - Analysis
- User provides project brief (reservation expiration feature)
- Analysis Agent creates:
  - wiki/sprint-3/functional-analysis-sprint-3.md
  - wiki/sprint-3/technical-analysis-sprint-3.md
- Handoff to PM Agent

Day 2 - Issues
- PM Agent creates 5 GitHub Issues:
  - US 3.1: Finite reservation window
  - US 3.2: Automatic release on expiry
  - US 3.3: Worker-driven lifecycle
  - US 3.4: Idempotent release
  - US 3.5: Observable lifecycle events
- Board updated with "Ready for Dev" column
- Handoff to Dev Agent

Days 3-5 - Implementation
- Dev Agent claims issues in priority order
- Implements with 70%+ test coverage
- Creates PRs for each issue
- Handoff to Review Agent

Days 5-6 - Review
- Review Agent verifies each PR
- Approves all (or requests changes)
- PRs merged to main

Result: 5 issues completed, all merged ✅
```

## 🛠️ Customization

### Adapt to Your Stack
Each `.instructions.md` file includes placeholders for:
- Your architecture patterns (Clean Architecture, DDD, etc.)
- Your technology stack (.NET, MongoDB, Kafka, etc.)
- Your naming conventions (branch names, commit messages, etc.)
- Your testing standards (coverage targets, test patterns, etc.)

### Common Customizations
1. **Technology Stack:** Update examples in instructions to match your stack
2. **Architecture Patterns:** Specify your Clean Architecture or DDD rules
3. **Code Style:** Add your linting/formatting requirements
4. **Testing Standards:** Specify coverage targets and test patterns

## 📊 Progress Tracking

The Orchestrator Agent maintains a **progress dashboard** showing:
- Current phase (Analysis / PM / Dev / Review / Done)
- Issues: created / in-progress / completed
- PRs: created / approved / merged
- Timeline and ETA
- Blockers or escalations

Example dashboard:
```
Sprint 3 Workflow Dashboard

Status: Analysis ✅ → PM ✅ → Dev 🔄 → Review ⏳ → Done ⏳

Current Phase: Development
- Issues in Progress: #16, #17
- PRs awaiting Review: 1

Progress: 40% complete (2 phases done, 3 to go)

Next Steps:
- Dev Agent continues with issue #16
- Review Agent will be triggered when PR is created
```

## 🚨 Escalation & Blockers

If an agent gets stuck or encounters ambiguity:

1. Agent flags the blocker
2. Orchestrator escalates to user
3. User provides clarification
4. Agent resumes work
5. Progress dashboard updated

Example escalation:
```
⚠️ Dev Agent Blocked on Issue #16

Issue: Acceptance criterion is ambiguous
"Seat remains unavailable until reservation expires or explicitly completed"
Does "explicitly completed" mean checkout success, or any action?

User Action Needed: PM clarifies in issue comment
Agent will resume once clarification provided.
```
