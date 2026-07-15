# AGILE FUNCTIONAL & TECHNICAL ANALYSIS: PHASE 1
**Sprint Target:** Sprint 1 (The Steel Thread / MVP Init)  
**Goal:** Establish the Domain Core, spin up local infrastructure, and validate the E2E reservation ingestion pipeline.

---

## 1. PHASE 1 SCOPE & BUSINESS VALUE
In an Agile product development lifecycle, attempting to build a fully optimized, highly concurrent transactional engine in a single iteration is an anti-pattern. 

Phase 1 focuses on building the **"Steel Thread"** (Tracer Bullet): the thinnest possible vertical slice of the application. The goal is to prove that our technical choices (.NET, Kafka, MongoDB) can communicate seamlessly in a local containerized environment. By the end of this sprint, a client will be able to dispatch a single seat reservation command and verify its schema-compliant propagation through Kafka down to the database.

---

## 2. AGILE BOARD: EPIC & USER STORIES

### EPIC: Core Ingestion & Infrastructure Setup
> **US1.1 - Local Development Sandbox (Dev Experience)** > *As a* Developer,  
> *I want* a containerized development environment with MongoDB and Apache Kafka,  
> *So that* I can run, test, and debug the entire ticketing infrastructure locally without cloud dependencies.  
>
> **Acceptance Criteria:** > * Running `docker-compose up` spins up MongoDB and Kafka (KRaft mode).  
> * Both services are reachable from localhost on standard ports (`27017`, `9092`).  
> * MongoDB operates as a replica set (required for transactions/change streams).

> **US1.2 - Core Event Booking Aggregate (Domain Modeling)** > *As a* Ticketing System,  
> *I want* to define the domain boundaries of a Concert Event and its Seats,  
> *So that* I can evaluate state transitions (Available -> Locked) using strict domain rules.  
>
> **Acceptance Criteria:** > * The system rejects a lock attempt if the target seat is not currently `Available`.  
> * Domain validations are fully encapsulated inside the `ConcertEvent` Aggregate Root (no anemic domain models).  
> * All domain state transitions emit a deterministic `SeatLocked` domain event.

> **US1.3 - Reservation Ingestion Pipeline (The Tracer Bullet)** > *As a* Ticket Buyer,  
> *I want* my reservation command to be immediately queued,  
> *So that* the application remains responsive under load.  
>
> **Acceptance Criteria:** > * A minimal `POST /api/v1/reservations` endpoint accepts a JSON reservation command.  
> * The endpoint validates the payload structure and immediately publishes a `ReserveSeatCommand` event to the Kafka topic.  
> * The endpoint returns an immediate `202 Accepted` response.

---

## 3. TECHNICAL TASKS BREAKDOWN (BACKLOG ITEMS)

### Task 1.1: Local Sandbox Orchestration (`docker-compose`)
* Create a root-level `docker-compose.yml` file.
* Configure **Apache Kafka** using a modern, lightweight Zookeeper-less configuration (using KRaft mode via `confluentinc/cp-kafka`).
* Configure **MongoDB** initialized as a single-member Replica Set via an entrypoint bash script (enabling transactional features for future sprints).

### Task 1.2: Clean Architecture Solution & DDD Domain Core
* Initialize a multi-project .NET Solution:
    * `Ticketing.Domain` (Zero external dependencies).
    * `Ticketing.Application` (Depends only on Domain).
    * `Ticketing.Infrastructure` (Depends on Application).
    * `Ticketing.Api` (Depends on Application and Infrastructure).
* Implement the `ConcertEvent` Aggregate Root in `Ticketing.Domain`:
    * Create a nested `Seat` Entity containing: `Id` (Guid), `Row` (string), `Number` (int), and `Status` (`Available`, `Locked`, `Sold`).
    * Implement `LockSeat(Guid seatId, TimeSpan duration)` on the Aggregate Root.
    * Implement domain verification invariants: raise a custom domain exception if the seat is already locked or sold.

### Task 1.3: Schema Registry & Kafka integration Contract
* Establish the serialization contract in `Ticketing.Application/Events/`:
    ```csharp
    public record SeatLockedIntegrationEvent(
        Guid EventId, 
        Guid SeatId, 
        DateTime LockedUntilUtc
    );
    ```
* Implement a high-performance **Kafka Producer** in `Ticketing.Infrastructure` using the `Confluent.Kafka` library. 
* Configure the producer with high-reliability defaults: `Acks = All`, `EnableIdempotence = true`, `LingerMs = 5`.

### Task 1.4: API Gateway & MediatR Integration
* Configure **MediatR** in `Ticketing.Application` to decouple HTTP requests from the business engine.
* Write a `ReserveSeatCommand` and its corresponding handler in the Application layer.
* Build a minimal API endpoint in `Ticketing.Api`:
    ```
    POST /api/v1/events/{eventId}/seats/{seatId}/reserve
    ```
    This endpoint dispatches the MediatR Command, which immediately routes the intent into the Kafka producer, returning `202 Accepted`.

---

## 4. DEFINITION OF DONE (DoD) FOR PHASE 1

A backlog item is only marked as **Done** when:
1.  **Code Quality:** Clean Architecture separation is strictly respected. No database or broker dependencies are referenced inside the `Domain` or `Application` layers.
2.  **Testing:** * Unit Tests cover 100% of the domain invariants in `ConcertEvent` (e.g., verifying that locking an already locked seat throws the correct domain exception).
    * An integration test successfully runs a local roundtrip: firing an HTTP POST to the API, verifying the event lands on the local Kafka topic, and confirming a `202 Accepted` response.
3.  **Documentation:** The local docker-compose configuration contains a README explaining how to spin up the infrastructure and run the project locally in under 3 commands.