# AGILE TECHNICAL ANALYSIS: SPRINT 3

**Sprint Target:** Sprint 3 (Reservation lifecycle enforcement and recovery)
**Goal:** Implement the most visible and interview-relevant correctness feature of the platform: temporary reservation holds that expire safely and release inventory automatically without breaking responsiveness.

---

## 1. TECHNICAL OBJECTIVES

Sprint 3 should focus on the core problem that makes this project interesting: preserving correctness under concurrency while keeping the user experience responsive.

The implementation should demonstrate the following engineering qualities:
- strong domain modeling rather than procedural handling;
- explicit lifecycle state transitions;
- background processing for asynchronous recovery;
- idempotent behavior so retries and replays do not corrupt state;
- a clear separation between command handling, domain rules, and infrastructure concerns.

This sprint is a strong interview candidate because it showcases a real distributed-systems problem: the system must behave correctly even when the reservation window expires after the request has already been accepted.

---

## 2. IMPLEMENTATION GOALS

### Goal A - Model reservation expiry as a first-class domain concept
The reservation flow should not be treated as a simple one-time lock. Instead, the system should model a temporary hold with a precise expiration boundary.

This means introducing a small lifecycle model around seat claims:
- Available
- TemporarilyLocked
- Expired
- Sold

The domain aggregate should own the transition logic so that the state machine remains explicit and impossible to bypass from the application layer.

### Goal B - Ensure automatic release of stale holds
When the expiration time is reached, the seat must return to an available state. This is the most visible functionality of the sprint and the one that most clearly demonstrates correctness under pressure.

The implementation should not simply rely on a timer in the API layer. Instead, it should route expiration handling through a domain event and a background worker that can reconcile stale holds.

### Goal C - Make the recovery path resilient and idempotent
The background worker may be retried, replayed, or delayed. The system should handle that naturally.

This means the release logic should be safe to run more than once. Re-processing an already-expired reservation should not create double-release side effects or inconsistent state.

---

## 3. CONCRETE FEATURES TO IMPLEMENT

### Feature 3.1: Reservation hold expiration metadata
Add explicit expiration information to reservation state.

Recommended shape:
- `LockedUntilUtc` on the reservation or hold record
- a `Status` transition to `Expired` after the timestamp passes
- an event such as `ReservationExpiredDomainEvent`

Why this matters:
- it makes the lifecycle explicit;
- it provides a clean boundary for the worker to operate on;
- it turns an implicit timeout into a visible business concept.

### Feature 3.2: Expiration domain rule in the aggregate
Extend the domain aggregate so it can evaluate whether a seat hold is still active.

Suggested domain operations:
- `TryExpire()` or `ReleaseExpiredHold()`
- `IsActive(DateTimeOffset now)`
- `CanBeReservedAgain()`

The aggregate should enforce that:
- an expired hold is no longer considered active;
- a seat can be re-reserved only after its hold has been released;
- the state transition is deterministic and centralized.

### Feature 3.3: Background worker for lifecycle reconciliation
Introduce a background service whose responsibility is to look for expired holds and trigger the release path.

Suggested behavior:
- periodically scan for holds that have passed their expiration time;
- publish or process an expiration event;
- update the seat state to available once the hold is no longer active.

This is a strong technical story because it demonstrates asynchronous recovery without sacrificing correctness.

### Feature 3.4: Idempotent expiration handling
The release path should be safe on retries.

Best practice:
- guard the release logic with a state check before mutating the seat;
- treat the operation as a transition from `TemporarilyLocked` to `Available` only when the reservation is still active;
- avoid duplicate side effects by making the operation atomic and state-driven.

### Feature 3.5: Observable lifecycle events
Emit domain and integration events for the expiration lifecycle.

Example events:
- `ReservationExpiredDomainEvent`
- `SeatReleasedIntegrationEvent`

This makes the solution more interview-ready because it shows event-driven design and easier observability.

---

## 4. RECOMMENDED ARCHITECTURAL DESIGN

### Layer responsibilities

#### Domain layer
The domain layer should own:
- seat state transitions;
- reservation expiry rules;
- domain events for lock and expiration;
- invariants that prevent rebooking an active hold.

This is the most important architectural boundary in Sprint 3. The domain should not depend on Kafka, timers, or persistence implementations.

#### Application layer
The application layer should own:
- command handling for reservation requests;
- a use case for expiration reconciliation;
- orchestration of domain behavior.

The application layer should not contain business rules that belong to the aggregate. Instead, it should coordinate between the domain and infrastructure.

#### Infrastructure layer
The infrastructure layer should own:
- background worker execution;
- repository or persistence adapters;
- event publishing to Kafka or an in-memory adapter for local development.

This layer is where the platform adapts the domain model to runtime concerns like timing, persistence, and messaging.

---

## 5. BEST PRACTICES TO FOLLOW

### 1. Keep the aggregate authoritative
The aggregate should be the only place that decides whether a seat can move from locked to available. Avoid letting the worker or service layer bypass that rule.

### 2. Prefer explicit state transitions over flags
A boolean like `IsExpired` is less expressive than a state transition such as `Expired` or `Available`. Explicit states make the system easier to reason about and much easier to explain in interviews.

### 3. Make the release path idempotent
A worker may process the same expiration event multiple times. The design should tolerate that gracefully.

### 4. Use events to decouple the lifecycle
The expiration workflow should be event-driven rather than hard-coded into the API response path. This makes the architecture more scalable and easier to evolve.

### 5. Preserve the current API responsiveness
The user-facing request should remain fast. The expiration-processing path should happen asynchronously in the background rather than block the reservation call.

### 6. Keep the implementation minimal but polished
This project is a pet project for interviews, so the implementation should be deliberately clear and elegant rather than overengineered. The most valuable version is one that demonstrates correctness, clean boundaries, and thoughtful failure handling.

---

## 6. CODE-LEVEL IMPLEMENTATION PLAN

### Step 1: Extend the domain model
Add the following to the seat or reservation domain model:
- an expiration timestamp field;
- a release operation;
- a method to determine whether the hold is still active.

The aggregate should expose a method such as:
```csharp
public bool TryReleaseExpiredHold(DateTimeOffset now)
```

and should transition the seat only when the hold is genuinely expired.

### Step 2: Introduce expiration-related domain events
Create domain events for:
- `SeatHoldExpired`
- optionally `SeatReleased`

These should be emitted from the aggregate so the infrastructure layer can react without embedding logic in the worker.

### Step 3: Add a reconciliation service in the application layer
Create a small application service that:
- fetches expired reservations or holds;
- asks the aggregate to release them;
- persists the new state.

This keeps the use case explicit and easy to test.

### Step 4: Implement a background worker
Add a background service that periodically evaluates active holds and invokes the reconciliation service.

The worker should be simple and deterministic:
- poll on a reasonable interval;
- process only expired items;
- log or publish the result.

### Step 5: Add tests that highlight the behavior
Tests should cover:
- successful reservation with a valid hold window;
- expiration causing the seat to become available again;
- repeated processing of the same expiration event without corrupting state;
- the worker invoking the release path at the correct time.

These tests are especially valuable because they show the project is not just “working,” but is behaving correctly under the most important edge cases.

---

## 7. WHY THIS SPRINT IS A STRONG INTERVIEW STORY

This sprint is valuable because it demonstrates several high-signal engineering practices at once:
- domain-driven state modeling;
- asynchronous recovery and background processing;
- correctness under race conditions and time-based state changes;
- idempotent event handling;
- a clean separation between business logic and infrastructure.

If implemented well, this sprint will make the project feel like a real reservation platform rather than a toy endpoint. It gives the interviewer a clear story to follow: the system accepts reservations, enforces a timed hold, releases stale inventory, and remains safe under retries and background processing.

---

## 8. DEFINITION OF DONE FOR SPRINT 3

The sprint is complete when:

1. The reservation lifecycle has an explicit expiration model.
2. Expired holds are released automatically and the seat becomes available again.
3. A background worker processes expiration reconciliation asynchronously.
4. The release logic is idempotent and safe under repeated execution.
5. Tests demonstrate the happy path, the expiration path, and the retry path.
6. The implementation is documented clearly enough that a reviewer can understand the design quickly.
