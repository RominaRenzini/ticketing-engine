# Ticketing Engine Wiki

## Purpose
This wiki captures the core context for the Cloud-Scale Event Ticketing Engine, a high-concurrency reservation platform designed to guarantee seat integrity during flash sales.

## What this project solves
The system is built to handle extreme traffic spikes while preserving two non-negotiable guarantees:
- no overbooking
- fair seat allocation under contention

## Document map
- [Functional Analysis](functional-analysis.md) - product vision, user stories, and business rules
- [Technical Architecture](technical-architecture.md) - architecture patterns, data flow, and design tradeoffs
- [Development Roadmap](roadmap.md) - milestones and implementation phases
- [Decision Log](decision-log.md) - key architectural decisions and rationale
- [Sprint 2 Functional Analysis](sprint-2/functional-analysis-sprint-2.md) - API contract refinement scope and requirements
- [Sprint 2 Technical Analysis](sprint-2/technical-analysis-sprint-2.md) - implementation approach for the versioned reservation endpoint

## Recommended reading order
1. Start with the functional analysis to understand the business problem.
2. Review the technical architecture to understand the implementation strategy.
3. Use the roadmap to plan delivery milestones.
4. Refer to the decision log when evaluating future changes.
5. Review the sprint 2 analysis for the current API contract iteration.

## Working assumptions
- The platform targets high-traffic live events and flash sales.
- The reservation flow must remain responsive even under heavy load.
- The system should prioritize correctness over convenience.
