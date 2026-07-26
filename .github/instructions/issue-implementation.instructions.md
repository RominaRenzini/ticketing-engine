# How to Implement a GitHub Issue Using Copilot

**Location:** `.github/instructions/issue-implementation.instructions.md`

This guide shows the step-by-step workflow for implementing a GitHub Issue with Copilot assistance.

---

## 🚀 Complete Workflow

### Phase 1: Preparation (5 minutes)

#### 1.1 Read the Issue
- Open the GitHub Issue
- Understand acceptance criteria
- Identify any related analysis documents
- Note any constraints or dependencies

#### 1.2 Understand the Architecture Context
Ask yourself:
- "Does this belong in Domain, Application, or Infrastructure layer?"
- "What existing aggregate roots or value objects apply here?"
- "What edge cases matter?"

#### 1.3 Create Feature Branch
```bash
git checkout -b feature/issue-#XXX-short-description
# Example: feature/issue-42-finite-reservation-window
```

---

### Phase 2: Implementation (30-60 minutes)

#### 2.1 Generate Domain Logic (if applicable)

**In Copilot Chat:**

```
I'm implementing GitHub issue:

[Issue Title and Number]

Acceptance Criteria:
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

Technology Stack:
- .NET 8
- MongoDB
- Clean Architecture (Domain/Application/Infrastructure)
- Kafka for events

Following .github/copilot-instructions.md:

Generate domain-level code for:
1. The domain logic (which aggregate root or value object)
2. Any domain exceptions needed
3. The invariant being protected (business rule)

Keep the domain layer pure - no infrastructure dependencies.

Important edge cases to consider:
- Concurrent access
- Timing/expiration scenarios
- State transitions

Provide:
- Domain class/method
- Domain exception (if needed)
- Brief explanation of invariant
```

**What Copilot will generate:**
- Domain class (e.g., `ConcertEvent` extension or new class)
- Method signature respecting the aggregate
- Domain exception
- Explanation of the business rule

#### 2.2 Implement the Code

1. Create/update the relevant file in the appropriate layer
2. Follow the Copilot output
3. Respect Clean Architecture boundaries
4. Test locally that it compiles

#### 2.3 Generate Unit Tests

**In Copilot Chat:**

```
Generate comprehensive unit tests for this method:

Method: [Method name and class]

Code:
[Paste the domain method]

Requirements:
- Use Arrange-Act-Assert pattern
- Test happy path
- Test edge cases:
  * Concurrent lock attempts on same seat
  * Lock on already-locked seat
  * Lock with negative duration (if applicable)
  * Lock on non-existent seat
- Use xUnit framework
- 70%+ coverage

Test class: [MethodName]Tests
Location: src/TicketingEngine.Tests/

Provide:
- Full test class with multiple [Fact] methods
- Clear test names: [MethodName]_[Condition]_[Expected]
- Use real domain objects (no mocks of domain layer)
```

**What Copilot will generate:**
- Complete test fixture
- Multiple test methods covering happy path and edge cases
- Clear naming and Arrange-Act-Assert pattern

#### 2.4 Run Tests Locally

```bash
# Run the tests
dotnet test src/TicketingEngine.Tests/

# Verify coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

#### 2.5 If Application/Infrastructure Layer Needed

**For Application Layer (use cases):**

```
Generate application logic for:

Use Case: [Use case name]
Domain Method: [Domain method from Phase 2.1]

Requirements:
- Depend on IConcertEventRepository (interface, not implementation)
- Call domain method
- Handle domain exceptions
- Publish domain events (if any)
- No infrastructure dependencies

Provide:
- Application service class
- Constructor with dependencies
- Public method that orchestrates the use case
```

**For Infrastructure Layer (repositories):**

```
Generate MongoDB repository implementation:

Interface: [Repository interface]
Aggregate: [Aggregate root]
Database: MongoDB (connection via IMongoCollection)

Requirements:
- Implement IConcertEventRepository
- GetByIdAsync(EventId) method
- SaveAsync(ConcertEvent) method
- Handle MongoDB exceptions

Provide:
- Full repository class
- Constructor injection of IMongoCollection<ConcertEvent>
- Implementation of required methods
```

---

### Phase 3: Verification (10 minutes)

#### 3.1 Build Check
```bash
dotnet build
# Should succeed with no errors
```

#### 3.2 All Tests Pass
```bash
dotnet test
# All tests should pass
```

#### 3.3 Code Review Self-Checklist
- [ ] Domain layer has zero infrastructure dependencies
- [ ] Tests use Arrange-Act-Assert pattern
- [ ] Edge cases are tested
- [ ] Code follows project naming conventions
- [ ] No business logic in controllers
- [ ] Layers are loosely coupled

---

### Phase 4: Create Pull Request (15 minutes)

#### 4.1 Generate PR Description

**In Copilot Chat:**

```
Generate a GitHub PR description:

Issue: #[issue number]
Issue Title: [issue title]

Acceptance Criteria from GitHub Issue:
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

Implementation Summary:
- Domain logic: [what changed]
- Tests: [number of tests, coverage %]
- Layers affected: Domain / Application / Infrastructure

Generate PR description that includes:
1. What changed (brief)
2. How it implements each acceptance criterion
3. Testing info (coverage, edge cases)
4. Any assumptions
5. Related documentation (link to wiki analysis if exists)

Format:
- Clear and scannable
- Use checkboxes for acceptance criteria
- Link to the GitHub Issue
```

#### 4.2 Push and Create PR

```bash
# Stage changes
git add .

# Commit
git commit -m "feat: implement [issue title] (issue #XXX)

- Add [what changed]
- [Second bullet point]
- Add [N] unit tests with [X]% coverage

Closes #XXX"

# Push
git push origin feature/issue-#XXX-short-description

# Create PR on GitHub with the description from 4.1
```

---

## 📋 Copilot Prompts Cheat Sheet

### Domain Logic
```
I'm implementing a GitHub issue in a Clean Architecture project.

Issue: [Issue number and title]

Acceptance Criteria:
- [ ] Criterion 1
- [ ] Criterion 2

Generate domain-level code for: [brief description]
Following .github/copilot-instructions.md
Keep domain layer pure (no infrastructure deps)
```

### Tests
```
Generate comprehensive unit tests for:
Method: [Method name]

Requirements:
- Arrange-Act-Assert pattern
- Happy path + edge cases
- Concurrent access testing
- 70%+ coverage

Test class: [ClassName]Tests
Framework: xUnit
```

### PR Description
```
Generate GitHub PR description for:
Issue: #[number]
Implementation: [brief summary]

Include:
- What changed
- How each acceptance criterion is met
- Test coverage info
- Related analysis/documentation

Format: Clear, scannable, checkbox format
```

---

## ✅ Checklist Before Pushing

- [ ] Feature branch created and working locally
- [ ] Code generated by Copilot (or manually written following patterns)
- [ ] Tests written and passing
- [ ] Build succeeds locally
- [ ] Code follows Clean Architecture boundaries
- [ ] No infrastructure dependencies in Domain layer
- [ ] 70%+ test coverage on core logic
- [ ] Naming conventions followed
- [ ] PR description generated
- [ ] Ready to push and create PR

---

## 🎯 Success Metrics

**After implementing an issue with Copilot:**

✅ Domain logic is clean and expressive
✅ Tests are comprehensive (edge cases included)
✅ Code respects Clean Architecture
✅ PR description is clear
✅ Build passes
✅ Acceptance criteria all met
✅ Ready for code review

---

## 🚨 Common Pitfalls

### Pitfall 1: Copilot Generated Infrastructure-Dependent Code
**Problem:** Generated code has MongoDB calls in Domain layer
**Solution:** Regenerate with explicit instruction: "Keep domain layer pure. No MongoDB imports."

### Pitfall 2: Tests Don't Cover Edge Cases
**Problem:** Tests only cover happy path
**Solution:** Ask Copilot explicitly: "Add tests for: concurrent access, invalid state, expiration"

### Pitfall 3: Forgot to Test Concurrency
**Problem:** Concurrent access not tested
**Solution:** Include in test prompt: "Test concurrent lock attempts on same seat"

### Pitfall 4: PR Description Doesn't Link to Acceptance Criteria
**Problem:** Reviewer can't see what was implemented
**Solution:** Use checkbox format in PR description linking each criterion to implementation

---

## 📞 When Copilot Struggles

If Copilot output isn't quite right:

1. **Be more specific:** Add examples or constraints to the prompt
2. **Reference the guidelines:** "@.github/copilot-instructions.md will help you..."
3. **Ask why:** "Why did you put this in the Domain layer instead of Application?"
4. **Provide context:** Paste existing code patterns as examples

---

## 🔗 References

- Copilot Instructions: `.github/copilot-instructions.md`
- Project Architecture: `/wiki/technical-architecture.md`
- Sprint 3 Analysis: `/wiki/sprint-3/functional-analysis-sprint-3.md`
- Workflow Example: `/wiki/sprint-3/copilot-workflow-example.md`
- Existing Tests: `src/TicketingEngine.Tests/`

---

**Next Issue? Follow this workflow and iterate!** 🚀
