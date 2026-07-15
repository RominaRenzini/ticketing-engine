# Technical Analysis — Sprint 1

## Objective
Provide a minimal but end-to-end reservation flow for the ticketing engine: domain state transitions, application orchestration, API ingress, and local infrastructure wiring.

## Implementation sequence
1. Domain core
   - Implement the ConcertEvent aggregate and seat state rules.
   - Introduce domain exceptions and domain events for seat locking.
2. Application layer
   - Define the reservation contract and handler used by the API.
   - Keep the application layer free of infrastructure concerns.
3. Infrastructure layer
   - Implement a reservation publisher abstraction and a concrete stub publisher.
   - Provide a lightweight background service placeholder for future Kafka consumption.
4. API layer
   - Expose POST /api/v1/events/{eventId}/reserve.
   - Return 202 Accepted for accepted reservations and validate the request body.
5. Local environment and documentation
   - Add docker-compose for MongoDB and Kafka.
   - Document how to run the infrastructure and verify the endpoint.

## Architectural decisions
- Keep the aggregate root responsible for seat locking invariants.
- Keep domain exceptions and events inside the domain layer.
- Use a simple publisher abstraction in the application layer so the infrastructure implementation can evolve without changing domain logic.
- Favor a thin API surface that delegates to the application layer instead of embedding business logic in controllers.

## Acceptance evidence
- Unit tests cover locking success and rejection cases for already locked seats.
- The API returns Accepted for a valid reservation request and uses the reservation service under the hood.
- Local infrastructure configuration is available through docker compose and documented for developers.
