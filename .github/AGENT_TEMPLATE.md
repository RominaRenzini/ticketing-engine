---
name: [AGENT_NAME]
role: [Primary responsibility - 1 sentence]
trigger: [manual | automatic | on-event]
trigger_condition: [When/how is this agent invoked?]
dependencies: [Which agents must run before this one?]
---

# [AGENT_NAME]

## Purpose
[2-3 sentences describing what this agent does and why]

## Responsibility
- [Core responsibility 1]
- [Core responsibility 2]
- [Core responsibility 3]

## Inputs
**From Previous Agent(s):**
- [What data/artifacts does it receive?]
- [File paths, issue IDs, etc.]

**From User:**
- [What user provides directly?]

## Process Flow
[Step-by-step what this agent does]

1. [First step]
2. [Second step]
3. [Continue...]
4. [Final step with output]

## Outputs
**Artifacts Created:**
- [What files/issues/PRs does it create?]
- [Where are they stored?]

**State Changes:**
- [What board state/issue state changes?]
- [What does it update?]

## Handoff to Next Agent
- [Which agent runs next?]
- [What information must be passed?]
- [How is handoff triggered?]

## Decision Points
[If there are conditional paths, list them]

- If [condition] → [action]
- If [condition] → [action]

## Success Criteria
- [ ] [Criterion 1]
- [ ] [Criterion 2]
- [ ] [Criterion 3]

## Error Handling
[What happens if something fails?]
- If [error] → [recovery action]
- If [error] → [recovery action]

## Notes for Implementation
[Specific guidance, edge cases, or constraints]
