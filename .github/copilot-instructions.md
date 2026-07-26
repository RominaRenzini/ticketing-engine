# Copilot Instructions for Ticketing Engine Development

## 🎯 Project Context

You are assisting with **ticketing-engine**, a production-grade, high-concurrency reservation system.

### Key Characteristics
- **Concurrency:** Handles thousands of simultaneous users (flash sales)
- **Correctness:** Inventory must never be oversold
- **Architecture:** Clean Architecture with Domain/Application/Infrastructure separation
- **Technology:** .NET 8, MongoDB, Kafka, Docker
- **Quality:** 70%+ test coverage on core logic

---

## 🏗️ Architecture Constraints

### Clean Architecture is Sacred
The project strictly follows Clean Architecture layers:

```
Domain Layer
  ↓ (no dependencies on infrastructure)
Application Layer
  ↓ (orchestration and use cases)
Infrastructure Layer
  ↓ (MongoDB, Kafka, external services)
```

**Critical Rule:** Domain layer must NEVER depend on infrastructure.

### Layer Responsibilities

**Domain Layer (`Domain/`)**
- Express business invariants (what makes a valid seat lock?)
- Define aggregate roots and value objects
- Throw domain exceptions for violations
- Zero infrastructure dependencies

**Application Layer (`Application/`)**
- Orchestrate domain logic
- Define use cases / commands / queries
- Handle transactions and distributed calls
- Depend on domain only

**Infrastructure Layer (`Infrastructure/`)**
- Implement repositories (MongoDB)
- Publish events (Kafka)
- External API calls
- Configuration and setup

---

## 🧪 Testing Standards

### Coverage Target: 70%+ on Core Logic

#### Unit Tests (Domain Logic)
- Test domain invariants
- Test edge cases (concurrent locks, expiration)
- Use Arrange-Act-Assert pattern
- No infrastructure dependencies

#### Edge Cases to Always Test
- ⚠️ **Concurrency:** Two users locking same seat simultaneously
- ⚠️ **Timing:** Expiration while checkout is in progress
- ⚠️ **Failures:** Database disconnects, Kafka unavailable
- ⚠️ **Idempotency:** Operation is safe to retry

---

## 🎨 Code Style & Patterns

### Naming Conventions
- **Classes:** PascalCase (`ConcertEvent`, `SeatLockException`)
- **Methods:** PascalCase, verb-first (`LockSeat`, `ReleaseExpiredHold`)
- **Variables:** camelCase (`lockedUntil`, `seatId`)
- **Tests:** `[MethodName]_[Condition]_[Expected]`

### Patterns to Follow
- Domain exceptions in `Domain/Exceptions/`
- Repository pattern (interface in Application, impl in Infrastructure)
- Domain events for state changes
- Idempotent operations where possible

---

## ⚠️ Common Mistakes to Avoid

1. **Business logic in controllers** - Keep it in Domain layer
2. **Infrastructure dependencies in Domain** - Always use interfaces/abstractions
3. **Not testing edge cases** - Concurrency, timing, and failure scenarios matter
4. **Tight coupling between layers** - Use dependency injection and abstractions

---

## 📝 Working with Copilot

### Effective Prompts

**For Domain Logic:**
```
Implementing GitHub issue: [number and title]
Acceptance Criteria: [list criteria]
Architecture: Clean Architecture (Domain/Application/Infrastructure)
Keep domain layer pure (no infrastructure deps)
```

**For Tests:**
```
Generate comprehensive unit tests for: [method name]
Requirements: Arrange-Act-Assert, edge cases, 70%+ coverage
Include concurrency and edge case testing
```

**For PR Description:**
```
Generate GitHub PR description for issue #[number]
Map each acceptance criterion to implementation
Include test coverage and edge cases
```

---

## ✅ Quality Checklist Before Creating PR

- [ ] Domain logic is in Domain layer (zero infrastructure deps)
- [ ] Tests cover 70%+ of core logic
- [ ] Edge cases tested (concurrency, timing, failures)
- [ ] Tests follow Arrange-Act-Assert pattern
- [ ] Code follows naming conventions
- [ ] No business logic in controllers
- [ ] Layers are loosely coupled (using interfaces)
- [ ] PR description links to GitHub Issue
- [ ] Acceptance criteria are all checked off
- [ ] Build passes locally

---

## 🔗 Related Documentation

- Clean Architecture: `/wiki/technical-architecture.md`
- Issue Implementation Workflow: `.github/instructions/issue-implementation.instructions.md`
- Copilot Workflow Example: `/wiki/sprint-3/copilot-workflow-example.md`
- Sprint 3 Analysis: `/wiki/sprint-3/functional-analysis-sprint-3.md`
