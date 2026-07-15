# Copilot Instructions for Ticketing Engine

- Follow the Clean Architecture boundaries in this repository.
- Keep domain logic in the domain layer and avoid leaking infrastructure details into it.
- Preserve correctness, fairness, and inventory integrity under high concurrency.
- When changing behavior, update the relevant documentation and tests.
- Prefer concise, structured implementation notes and clear assumptions.
- Be concise and avoid conversational fluff, pleasantries, or long explanations unless explicitly requested.
- When editing code, make minimal targeted changes only; do not rewrite entire files or classes when a smaller patch is sufficient.
- Provide architectural or technical explanations only when explicitly asked.
- Before executing any code task, check the project board state through the available MCP board tool and keep board updates silent and atomic: move work to In Progress when starting, and to Done only after tests or build verification succeed.
- Keep responses short, implementation-oriented, and easy to scan with bullets and brief structure.
