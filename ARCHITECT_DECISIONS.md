# ARCHITECT_DECISIONS

## Purpose
This file is the persistent architecture memory for the project. Every implementation cycle must be checked against these decisions before any code is merged.

## Non-negotiable invariants
1. Language lock: C# on .NET 8.
2. Clean Architecture is mandatory.
3. Dependency direction always points inward.
4. Domain layer remains pure.
5. Application layer must not depend on Infrastructure.
6. Infrastructure only implements interfaces from inner layers.
7. Adapter pattern is required for provider integration.
8. Strategy pattern is required for provider routing.
9. Async/await only in request processing paths.
10. Architecture safety has priority over feature speed.
11. Tests are required for behavior changes.
12. QA validation is required before completion.
13. Refactor is required when duplication or leakage appears.

## Stable decisions and rationale
- **Provider logic isolation**: Provider-specific mapping, transport and error handling stay in adapters, avoiding business logic leaks into Domain/Application.
- **Orchestrator stability**: The orchestration flow is a protected use-case boundary; extending providers should not require orchestrator rewrites.
- **Strict dependency direction**: Prevents coupling regressions and keeps replacement/testing costs low.
- **Strategy-based provider selection**: Routing behavior can evolve independently of adapters and use-cases.
- **Extension-only architecture**: New providers are onboarded by adding adapters/strategies, not by modifying core abstractions.

## Historical risk memory
Avoid reintroducing:
- God classes in orchestration or gateway layers.
- Hardcoded provider conditionals in use cases.
- Business rules in Infrastructure adapters.
- Layer violations via direct cross-layer references.

## Guardian checklist (must pass each cycle)
- Extension safety preserved.
- New provider onboarding requires only a new adapter + registration.
- Domain layer remains untouched by infrastructure concerns.
- Orchestration contract stays stable.

## Required cycle report format
At the end of architecture-impacting changes, emit:

```text
ARCHITECT_MEMORY_SUMMARY:
- Architectural rules remembered
- Decisions influencing current change
- Risks prevented
- Design consistency status
```

Status line policy:
- If unsafe: `ARCHITECT_MEMORY_STATUS = VIOLATION` and `SAFE_TO_CONTINUE = FALSE`, then refactor before merge.
- If safe: `ARCHITECT_MEMORY_STATUS = CONSISTENT` and `SAFE_TO_CONTINUE = TRUE`.
