# Decision Log

## 2026-07-18 - Sprint 2 API contract preferences

The following implementation preferences were captured for the second sprint:

- Reservation request contracts should be treated as API transport contracts and mapped into the application command flow.
- The reservation endpoint should not expose `eventId` as a path parameter.
- API versioning should use the query string parameter `api-version` together with the `ApiVersion` attribute.

These preferences should guide implementation work for the reservation endpoint and related API contracts.

## 2026-07-26 - Sprint 4 review: known gap deferred to Sprint 5

A retroactive review of the Sprint 4 persistence work (`MongoConcertEventRepository`) found that the optimistic-concurrency check in `UpdateAsync` re-reads the document's *current* version at write time instead of comparing against the version the caller originally loaded. It only throws on the narrow case of two writers reading the same version at nearly the same instant; a writer that reads later (after another writer already committed) is allowed to silently overwrite the intervening change with its own stale snapshot ("lost update"). This is proven by the characterization test `MongoConcertEventRepositoryTests.UpdateAsync_SilentlyDropsAnEarlierChange_WhenCallerReconcilesAgainstTheLatestVersionInstead`.

**Decision:** defer the real fix to Sprint 5. Fixing it properly requires the `ConcertEvent` aggregate to carry the version it was loaded with, and `IConcertEventRepository`/`ReservationService` to thread that version through the update call — a bigger change than the immediate Sprint 4 fixes (concurrency retry in `ReservationService`, correct Save-vs-Update branching, Mongo integration test coverage), which have already landed.
