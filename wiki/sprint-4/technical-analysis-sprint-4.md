# AGILE TECHNICAL ANALYSIS: SPRINT 4

**Sprint Target:** Sprint 4 (Persistence and repository backbone)
**Goal:** Replace the current in-memory state handling with a real persistence layer so the reservation engine can store and recover concert-event state safely, durably, and in a way that is easy to explain in interviews.

---

## 1. TECHNICAL OBJECTIVES

Sprint 4 should introduce the first real storage abstraction for the platform. The project already demonstrates business logic and lifecycle behavior, but it still relies on an in-memory store. That is acceptable for a proof of concept, but it is not yet credible as a ticketing platform.

This sprint should make the architecture more convincing by introducing:
- a repository abstraction in the application layer;
- a concrete persistence implementation in the infrastructure layer;
- a clear mapping between the domain model and the persisted representation;
- a design that preserves correctness under concurrency and retries.

The most natural fit for this project is MongoDB, because it aligns well with the current event-driven and document-oriented mental model and is commonly used in interview demos for flexible, scalable write models.

---

## 2. WHY MONGODB IS A GOOD FIT

MongoDB is a strong choice for this sprint for several reasons:

### 2.1 It fits the current domain model well
The current aggregate is relatively simple: an event contains seats and reservation state. That maps naturally to a document-like shape, especially when the goal is to store an event aggregate and its nested seat state in one place.

### 2.2 It supports fast iteration for a pet project
MongoDB is easy to run locally, easy to reason about, and easy to demonstrate to interviewers.

### 2.3 It reinforces the platform story
A ticketing platform needs to store evolving state under load. MongoDB gives the project a concrete persistence story that feels much closer to a real system than an in-memory dictionary.

---

## 3. RECOMMENDED ARCHITECTURAL DESIGN

### 3.1 Repository abstraction in the application layer
Introduce an interface such as:

```csharp
public interface IConcertEventRepository
{
    Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
}
```

This makes the persistence detail an implementation concern while keeping the application layer focused on use cases.

### 3.2 Concrete MongoDB implementation in the infrastructure layer
Create a MongoDB-backed repository implementation that:
- talks to MongoDB through the official .NET driver;
- maps the domain aggregate to a document;
- stores the seat list and hold metadata in a single collection.

A practical collection shape would be:
- `id` for the event identifier;
- `name` and `startsAt`;
- `seats` array containing seat rows, numbers, status, and lock expiration metadata.

### 3.3 Keep the domain model pure
The domain model should remain free of infrastructure dependencies. The repository implementation should be responsible for translation between the aggregate and document format.

That separation is important for interview readiness because it demonstrates clean architecture rather than a leaky persistence model.

---

## 4. BEST PRACTICES TO FOLLOW

### 1. Preserve the aggregate boundary
Do not let the repository expose partial or low-level state that bypasses the domain. The aggregate should still be the single authority for state transitions.

### 2. Keep persistence mappings explicit
Use a dedicated mapping layer or repository implementation to translate between the domain and MongoDB documents. Avoid mixing serialization concerns into the domain object.

### 3. Use optimistic concurrency where possible
The reservation system is correctness-sensitive. If the project evolves toward concurrent updates, consider using a version field in the document and a compare-and-set strategy when updating the aggregate.

### 4. Use idempotent write patterns
The repository should be safe for repeated writes or retries. This is especially important because the reservation flow can already be replayed or retried by background workers.

### 5. Favor simple, durable writes over premature complexity
This sprint should not overengineer the persistence design. The goal is to introduce durability and repository discipline in a way that is easy to explain and easy to demo.

### 6. Keep the API response flow unaffected
The controller should continue to behave as an adapter. The domain and application layers should remain the place where business behavior is enforced.

---

## 5. CONCRETE FEATURES TO IMPLEMENT

### Feature 4.1: Repository abstraction
Introduce a repository contract for concert events and make the reservation service depend on it instead of the in-memory state store.

This is the first visible architectural improvement of Sprint 4 because it removes the last demo-only shortcut from the design.

### Feature 4.2: MongoDB repository implementation
Implement a concrete MongoDB repository with:
- connection configuration via app settings;
- a collection for concert events;
- insert and update operations for aggregate persistence.

The repository should support:
- retrieving an existing event by id;
- persisting a newly created event;
- updating an event after a lock or release transition.

### Feature 4.3: Document shape for reservation state
Persist the following information in the event document:
- event id;
- event name;
- start time;
- array of seats with status and expiration metadata;
- reservation-related timestamps.

This is a strong interview story because it shows that the system can now recover its state after a restart and is no longer limited to transient memory.

### Feature 4.4: Application service adaptation
Refactor the reservation flow so the application layer uses the repository to load the event aggregate, apply the domain operation, and persist the updated state.

This makes the business flow explicit and keeps the service logic readable.

### Feature 4.5: Basic persistence tests
Add tests that demonstrate:
- a concert event can be saved and loaded from the repository;
- a lock transition persists correctly;
- an expiration release persists correctly.

These tests should be lightweight and focused on behavior, not on low-level driver details.

---

## 6. IMPLEMENTATION PLAN

### Step 1: Add the repository contract
Create the abstraction in the application layer and keep it intentionally small.

### Step 2: Add the MongoDB infrastructure implementation
Introduce the infrastructure project dependency on the MongoDB .NET driver and create a repository implementation around a single collection.

### Step 3: Replace the in-memory store in the reservation flow
The reservation service should use the repository instead of the current in-memory state store to load and save the aggregate.

### Step 4: Make the background worker use the repository
The expiration reconciliation worker should read and update persisted state instead of relying on transient memory.

### Step 5: Add configuration and local setup
Add configuration entries for MongoDB connection details and document how to run the local database with Docker.

### Step 6: Add tests and validation
Create tests that prove the repository behaves as expected and that the domain invariants remain intact when state is loaded from persistence.

---

## 7. WHY THIS SPRINT IS A STRONG INTERVIEW STORY

Sprint 4 is valuable because it demonstrates the jump from a conceptual domain model to a durable system. It shows that the project is not just a toy example: it can now persist business state, recover it after restarts, and support the same domain workflow through a real data layer.

This is exactly the kind of progression that interviewers like to see:
- first: business rules;
- then: asynchronous recovery;
- then: persistence and durability.

That progression makes the project feel intentional and mature.

---

## 8. DEFINITION OF DONE FOR SPRINT 4

The sprint is complete when:

1. A repository abstraction exists for concert events.
2. The reservation flow persists and retrieves aggregate state through that repository.
3. A concrete MongoDB implementation is wired into the infrastructure layer.
4. The background expiration worker uses persisted state rather than in-memory state.
5. Tests confirm that persistence preserves lock and release behavior.
6. The implementation remains aligned with Clean Architecture and domain-driven design principles.
