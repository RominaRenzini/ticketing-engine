# AGILE TECHNICAL ANALYSIS: SPRINT 2

**Sprint Target:** Sprint 2 (API contract refinement and versioned reservation entrypoint)
**Goal:** Refine the HTTP layer so the reservation endpoint is cleaner, versionable, and aligned with the team’s preferred API conventions.

---

## 1. TECHNICAL OBJECTIVES

Sprint 2 focuses on the API surface rather than the reservation engine itself. The goal is to improve the entrypoint shape while preserving the current business behavior.

The implementation should preserve Clean Architecture boundaries:

- The domain layer remains responsible for reservation rules.
- The application layer remains responsible for command handling.
- The API layer remains responsible for transport contracts, request mapping, and HTTP concerns.

---

## 2. TECHNICAL TASKS

### Task 2.1: Request contract ownership

- Keep the transportation request model in the API layer as a dedicated contract type.
- Map the request contract into the application command model inside the controller or a small adapter layer.
- Avoid coupling the controller action directly to request-specific logic that belongs to the API contract boundary.

### Task 2.2: Endpoint shape refinement

- Change the reservation endpoint to use a query string for the event identifier.
- Prefer an endpoint shape such as:

```text
POST /api/events/reserve?eventId={guid}&api-version=1.0
```

- Keep the action signature focused on input binding and response shaping rather than route parsing.

### Task 2.3: API versioning strategy

- Enable ASP.NET Core API versioning in the API project.
- Configure version reading from the query string using the `api-version` parameter.
- Decorate the controller or action with the `ApiVersion` attribute, for example:

```csharp
[ApiVersion("1.0")]
```

- Keep versioning explicit and consistent for future expansion.

### Task 2.4: Testing and validation

- Add or update controller-level tests to validate request mapping and response behavior.
- Add tests to cover the new query-string-based endpoint contract.
- Validate that versioning works when `api-version` is supplied and that requests without it are handled according to the configured default.

---

## 3. ARCHITECTURE DECISIONS

### Decision A: Keep the controller thin
The controller should remain an adapter between HTTP and the application layer. It should not own complex request behavior or embed transport concerns in a way that obscures the application boundary.

### Decision B: Prefer query-string event selection
The event identifier should be supplied as a query parameter instead of a route path parameter. This keeps the URL simpler and avoids overloading the route with resource identifiers that are not naturally part of the resource hierarchy.

### Decision C: Use query-string versioning
API versioning should be explicit and query-driven, using `api-version` and the `ApiVersion` attribute. This makes the contract visible in the request and avoids route-segment versioning for this endpoint.

---

## 4. DEFINITION OF DONE FOR SPRINT 2

The sprint is complete when:

1. The reservation endpoint uses query-string event selection.
2. API versioning is implemented with `api-version` and the `ApiVersion` attribute.
3. The request contract is clearly treated as an API transport concern rather than controller-local behavior.
4. Documentation and tests reflect the new endpoint contract.
