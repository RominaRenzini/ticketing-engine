# AGILE FUNCTIONAL ANALYSIS: SPRINT 4

**Sprint Target:** Sprint 4 (Persistence and repository backbone)
**Goal:** Give reservations a durable home. A concert event and its seats must survive an API restart, so a hold or a sale is never lost just because a process recycled.

---

## 1. SPRINT 3 SCOPE & BUSINESS VALUE

Sprint 3 made the reservation lifecycle correct in memory: holds expire, releases are idempotent, and a background worker reconciles state. Sprint 4 makes that lifecycle durable. Without persistence, every deploy, crash, or restart silently forgets who holds which seat — which is unacceptable for a platform that promises no overbooking.

The business value:
- a seat lock or sale survives a service restart;
- the reservation and expiration worker operate on the same durable source of truth, instead of a store that resets when the process does;
- the platform can scale to more than one API instance without instances disagreeing about seat state.

---

## 2. AGILE BOARD: EPIC & USER STORIES

### EPIC: Durable reservation state

> **US4.1 - Reservations survive a restart**
> As a platform operator, I want reservation and seat state stored outside process memory, so that a deploy or crash does not silently lose who holds which seat.
>
> **Acceptance Criteria:**
> - A newly reserved seat is retrievable by event id after the process that created it has restarted.
> - Reading an event returns the same seat statuses and lock expirations that were last written.
> - No reservation-relevant state lives only in an in-memory dictionary.

> **US4.2 - Consistent lifecycle across API and worker**
> As the reservation engine, I want the API and the background expiration worker to read and write the same persisted state, so that a lock created by the API and released by the worker never disagree.
>
> **Acceptance Criteria:**
> - The background worker loads events from the repository, not from an in-memory cache shared by reference with the API.
> - A hold released by the worker is visible to the next API read for that event.
> - The worker only mutates events it actually changed (no redundant writes for untouched events).

> **US4.3 - Safe concurrent writes**
> As a platform operator, I want concurrent updates to the same event to be detected instead of silently overwritten, so that two writers can never clobber each other's changes.
>
> **Acceptance Criteria:**
> - Each stored event carries a version marker that advances on every update.
> - An update built from a stale read is rejected rather than silently applied.
> - The rejection is explicit (a thrown error or comparable signal), not a silent no-op.

---

## 3. FUNCTIONAL REQUIREMENTS

1. Creating or updating a reservation must persist the concert event and its seats outside process memory.
2. Reading a concert event by id must return the latest persisted seat statuses and lock expirations.
3. The expiration worker must reconcile state through the same persisted source the API writes to.
4. A write based on an outdated version of an event must fail instead of overwriting newer state.
5. Persistence must not leak into the domain model — `ConcertEvent` and `Seat` stay free of storage-specific types.

---

## 4. CAPTURED PREFERENCES FOR IMPLEMENTATION

- MongoDB was chosen over a relational store because the event/seat aggregate maps naturally to a single document, and it keeps local setup fast (see `docker-compose.yml`).
- Optimistic concurrency (a version field, compare-and-swap on update) was preferred over pessimistic locking to avoid blocking readers under contention.
- The repository interface (`IConcertEventRepository`) lives in the Application layer; only its implementation (`MongoConcertEventRepository`) knows about MongoDB.
- Writes should be idempotent-friendly: retried updates should fail loudly on version conflict rather than silently double-apply.

---

## 5. DEFINITION OF DONE FOR SPRINT 4

A backlog item is considered done only when:

1. A repository abstraction exists for concert events, with no MongoDB types leaking into the Application or Domain layers.
2. The reservation flow persists and retrieves aggregate state exclusively through that repository.
3. The background expiration worker reads and writes through the same repository, not an in-memory store.
4. Concurrent updates to the same event are detected via a version check and rejected on conflict.
5. Tests demonstrate that the **concrete** repository implementation (not just an in-memory stand-in) preserves lock and release behavior across save/load cycles.
6. The implementation remains aligned with Clean Architecture: Domain has zero infrastructure references.
