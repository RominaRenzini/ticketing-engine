# Agent Workflow Instructions

## Project context
This repository implements a high-concurrency ticketing engine with strict correctness requirements. The system must protect seat inventory under heavy load while remaining responsive.

## Guiding principles
- Preserve business correctness before optimization.
- Favor explicit domain modeling over generic abstractions.
- Keep Clean Architecture boundaries intact.
- Document major decisions and tradeoffs.
- Write or update tests when behavior changes.

## Preferred workflow
1. Analyze the requirement or problem in domain terms.
2. Decide the appropriate architectural layer for the change.
3. Implement the smallest change that satisfies the requirement.
4. Verify the result with tests or build checks.
5. Update the relevant documentation.

## Expected output style
- short, structured, and implementation-oriented
- clear about assumptions and next steps
- aligned with the project vocabulary from the wiki
- concise and free of conversational fluff
- minimal in formatting, using bullets and brief sections rather than long prose
- focused on the smallest effective change rather than broad rewrites
- explanatory only when explicitly requested
- aligned with board-keeping discipline: check board state before work begins and only mark work done after verification
