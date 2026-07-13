# Development Roadmap

## Milestone 1 - Aggregates and base infrastructure
- Create a Clean Architecture solution structure
- Model the ConcertEvent aggregate and seat state transitions
- Set up Docker-based local infrastructure with Kafka and MongoDB

## Milestone 2 - Write-optimized API and streaming pipeline
- Implement CQRS command handling with MediatR
- Add a reservation endpoint for incoming booking commands
- Configure Kafka producers with reliability settings

## Milestone 3 - Background processing and concurrency validation
- Build a worker service to process reservation events
- Implement repository logic with optimistic concurrency control
- Add tests for race-condition scenarios and duplicate prevention

## Milestone 4 - Expiration workflow and load testing
- Add TTL-based reservation expiration handling
- Integrate event-driven cleanup for expired locks
- Perform stress testing to validate behavior under peak traffic

## Delivery focus
The roadmap emphasizes correctness first, then throughput, then resilience and observability.
