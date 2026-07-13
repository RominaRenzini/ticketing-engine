# Technical Architecture

## Architectural approach
The system uses a combination of Clean Architecture, Domain-Driven Design, Event-Driven Architecture, and CQRS.

## High-level flow
```text
Client -> Write API -> Kafka -> Background Worker -> MongoDB
                \                 /
                 -> Read API <- Read Model
```

## Core design principles
- Separate command and query responsibilities
- Keep business rules inside the domain layer
- Use events to decouple reservation handling from downstream workflows
- Preserve fairness under contention by processing reservation commands in order

## DDD blueprint
- Aggregate Root: ConcertEvent
- Entities: Seat
- Value Objects: ReservationWindow
- Domain Events: SeatLocked, ReservationExpired, TicketSold

## Key architectural challenges and solutions
### 1. Preventing overbooking
The system uses optimistic concurrency control with version checks so concurrent reservation attempts cannot both succeed for the same seat.

### 2. Preserving ordering under high load
Reservation commands are routed through Apache Kafka using a deterministic partition key so one concert's requests remain ordered.

### 3. Reliable expiration handling
MongoDB TTL indexes track reservation lifetimes, and expiration events trigger state restoration without manual intervention.

## Technology choices
- Language: C# / .NET
- Messaging: Apache Kafka
- Persistence: MongoDB
- Deployment: Docker

## Operational concerns
- Observability and distributed tracing
- Idempotency for message processing
- Replay safety for event consumers
- Resilience under backpressure
