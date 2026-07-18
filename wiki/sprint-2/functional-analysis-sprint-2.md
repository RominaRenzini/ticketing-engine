# AGILE FUNCTIONAL ANALYSIS: SPRINT 2

**Sprint Target:** Sprint 2 (API contract refinement and versioned reservation entrypoint)
**Goal:** Harden the reservation API contract so it is easier to evolve, easier to consume, and aligned with the team’s preferred conventions.

---

## 1. SPRINT 2 SCOPE & BUSINESS VALUE

Sprint 1 proves the reservation flow can be ingested and processed. Sprint 2 focuses on the public contract exposed by the API so integration partners and future features can evolve without introducing unnecessary friction.

The work is not about adding business rules for seat inventory. It is about improving the contract surface of the reservation endpoint so it is cleaner, more explicit, and easier to version.

---

## 2. AGILE BOARD: EPIC & USER STORIES

### EPIC: API contract refinement and versioning

> **US2.1 - Clean request contract ownership**  
> As an API consumer, I want reservation requests to be represented through a dedicated API contract, so that the controller stays thin and the request model is not mixed into controller-specific behavior.
>
> **Acceptance Criteria:**
> - The reservation request contract is defined as a transport model in the API layer.
> - The controller delegates request mapping to a command-oriented flow instead of embedding request behavior inside the action itself.
> - The request contract remains clearly separated from domain and infrastructure concerns.

> **US2.2 - Simplified reservation endpoint shape**  
> As an API consumer, I want to reserve a seat without putting the event identifier into the path, so that the endpoint remains concise and easier to evolve.
>
> **Acceptance Criteria:**
> - The endpoint uses a query string parameter for the event identifier.
> - The action is reachable through a stable route such as `/api/events/reserve?eventId=...`.
> - The endpoint remains easy to document and call from clients.

> **US2.3 - Explicit API versioning**  
> As an API consumer, I want the API version to be selected through a query string parameter, so that version selection is explicit and consistent.
>
> **Acceptance Criteria:**
> - Versioning is enabled with ASP.NET Core API versioning.
> - The version is supplied through the `api-version` query parameter.
> - The controller uses the `ApiVersion` attribute to declare supported versions.

---

## 3. FUNCTIONAL REQUIREMENTS

1. The reservation request payload should be treated as a public API contract and mapped into the application command model.
2. The reservation endpoint should accept the event identifier through the query string rather than as a route parameter.
3. The API version should be supplied through the query string parameter `api-version`.
4. The endpoint should continue to preserve the current reservation semantics: accept a reservation request, validate the input, and return a successful accepted response when the request is accepted.

---

## 4. CAPTURED PREFERENCES FOR IMPLEMENTATION

The following preferences are now part of the project guidance for Sprint 2:

- `ReserveRequest` should not be treated as controller-local behavior; it should remain a transport contract in the API layer and be mapped into the application command flow.
- The reservation endpoint should not expose `eventId` as a path segment.
- API versioning should be managed with query-string versioning and the `ApiVersion` attribute.

---

## 5. DEFINITION OF DONE FOR SPRINT 2

A backlog item is considered done only when:

1. The controller remains thin and delegates request translation to a dedicated contract-to-command flow.
2. The reservation endpoint supports event selection via query string and no longer depends on an `eventId` path parameter.
3. API versioning works correctly through `api-version` in the query string.
4. The public contract is documented clearly in the wiki and reflected in the implementation.
