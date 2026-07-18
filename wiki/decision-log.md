# Decision Log

## 2026-07-18 - Sprint 2 API contract preferences

The following implementation preferences were captured for the second sprint:

- Reservation request contracts should be treated as API transport contracts and mapped into the application command flow.
- The reservation endpoint should not expose `eventId` as a path parameter.
- API versioning should use the query string parameter `api-version` together with the `ApiVersion` attribute.

These preferences should guide implementation work for the reservation endpoint and related API contracts.
