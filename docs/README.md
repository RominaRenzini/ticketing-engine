# Project Documentation

## Overview
This project is a cloud-scale event ticketing engine for high-concurrency seat reservation and purchase workflows.

## Key capabilities
- real-time seat availability
- temporary seat locking
- asynchronous checkout support
- automatic reservation expiration

## Recommended stack
- C# .NET
- Apache Kafka
- MongoDB
- Docker

## Documentation index
- [Wiki home](../wiki/README.md)
- [Functional analysis](../wiki/functional-analysis.md)
- [Technical architecture](../wiki/technical-architecture.md)
- [Development roadmap](../wiki/roadmap.md)

## Sprint 1 local setup
Run the local infrastructure:

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project src/TicketingEngine.Api/TicketingEngine.Api.csproj
```

Verify the reservation endpoint:

```bash
curl -X POST http://localhost:5000/api/v1/events/00000000-0000-0000-0000-000000000000/reserve -H "Content-Type: application/json" -d '{"row":"A","number":1}'
```

## Notes for AI-assisted development
Keep documentation aligned with the core constraints of correctness, fairness, and high availability. When making changes, prefer updates that strengthen reliability and observability.
