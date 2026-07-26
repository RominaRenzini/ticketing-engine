# Example Project Brief - Sprint 5: Query Optimization

Use this as a template when running the Orchestrator Agent for the first time.

---

## Problem Statement

The ticketing engine's reservation queries are experiencing performance degradation under high load. When multiple users are attempting to reserve seats simultaneously, query latency increases from 50ms (idle) to 500ms+ (peak), impacting user experience.

**Business Impact:**
- Users see slow feedback during flash sales
- Timeout risk when server can't respond in time
- Potential lost sales due to poor UX

**Goal:**
Optimize reservation queries to maintain sub-100ms latency even at peak concurrency (10,000 concurrent users).

---

## Technology Stack

**Language:** C# (.NET 8)  
**Web Framework:** ASP.NET Core  
**Database:** MongoDB  
**Caching:** Redis (to be added)  
**Message Queue:** Kafka  
**Testing:** xUnit, Testcontainers  

**Current Architecture:**
- Clean Architecture (API → Application → Domain → Infrastructure)
- CQRS pattern (Commands + Queries)
- Event-driven processing
- MongoDB aggregate repository

---

## Constraints & Preferences

1. **Maintain Clean Architecture** - Query optimization shouldn't break layer separation
2. **Keep test coverage 70%+** - New code must be thoroughly tested
3. **No breaking changes** - Current API contracts remain stable
4. **Backward compatible** - Existing data formats unchanged
5. **Observable** - Add metrics/logging for query performance tracking

---

## Assumptions

1. Bottleneck is in MongoDB queries (not application logic)
2. Redis is available as caching layer
3. Query patterns are known (mostly seat availability checks)
4. Can add indexes without downtime
5. Load testing environment available

---

## Success Criteria

- [ ] Query latency < 100ms at 10,000 concurrent users
- [ ] 70%+ test coverage on query layer
- [ ] Backward compatible API
- [ ] Redis integration working
- [ ] Metrics/logging in place
- [ ] Documentation updated
- [ ] Performance benchmark shows 5x improvement

---

## Questions for Analysis Agent

1. What query patterns are most frequent? (seat availability, event details, user reservations?)
2. Which queries account for most latency?
3. Should we cache entire queries or parts (e.g., event details)?
4. What's the acceptable cache staleness?
5. Are there query patterns we're missing?

---

## Desired Outcomes

**From Analysis:**
- Functional analysis: exact query bottlenecks + user impact
- Technical analysis: proposed optimization approaches + tradeoffs

**From PM:**
- 3-5 GitHub Issues with clear acceptance criteria
- Priority order (which queries to optimize first?)
- Dependencies (does X need to complete before Y?)

**From Dev:**
- Code changes with query optimizations
- Unit tests for query performance
- Integration tests with MongoDB
- PRs for each issue with 70%+ coverage

**From Review:**
- Verification that latency targets met
- Code quality and architecture compliance
- Test coverage verified
- Performance metrics documented
- Approvals for merge

---

## Timeline

**Expected Duration:** 1 week
- Day 1: Analysis (4 hours)
- Day 2: Issues (1-2 hours)
- Days 3-5: Implementation (2-3 days)
- Days 5-6: Review (1 day)

---

## References

**Related Issues:**
- #45 - Performance degradation reports from users
- #46 - Query latency metrics needed

**Existing Docs:**
- Architecture: `/wiki/technical-architecture.md`
- Database schema: `/wiki/mongodb-schema.md`
- Query patterns: `/src/TicketingEngine.Infrastructure/Queries/`

---

## Next Steps

1. **Copy this brief** to use with Analysis Agent
2. **Open Copilot Chat** in VS Code
3. **Copy** `.github/instructions/analysis.instructions.md`
4. **Paste** into chat
5. **Add** this project brief
6. **Ask:** "Analyze this feature using these instructions"
7. **Wait** for functional and technical analysis
8. **Advance** to PM phase when complete

---

**This is a realistic example.** Adapt it to your actual project needs!
