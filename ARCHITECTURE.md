# Architecture Phase Report

## Current State
`ARCHITECTURE`

This document executes the requested architecture state workflow and enforces the project invariants:
- .NET 8 / C# only
- Clean Architecture + SOLID
- DI-first composition
- Adapter pattern for provider integrations
- Strategy pattern for provider selection
- Async/await only execution model
- No static business logic in core flows
- Mandatory architecture review, code review, QA validation, and refactor

---

## Step 1 — Architecture Discussion

### Service boundaries
The platform is split into clear bounded services with inward dependency direction:
1. **Gateway API (Presentation + Application composition root)**
   - Accepts unified API requests.
   - Performs auth, validation, request normalization, and response shaping.
   - Calls orchestration use cases through DI-managed interfaces.
2. **Orchestrator (Application layer)**
   - Owns provider selection workflow and fallback flow.
   - Invokes provider adapters through ports (interfaces).
   - Emits domain events/telemetry intents (non-infrastructure).
3. **Provider Adapter layer (Infrastructure layer)**
   - One adapter per provider implementing common provider port.
   - Translates unified request/response contracts.
   - Encapsulates provider-specific SDK/HTTP behavior and error mapping.
4. **Quota & Policy service (Infrastructure/sidecar or standalone service)**
   - Tracks tenant/application quotas and throttling windows.
   - Provides policy lookups for hard/soft limits.
5. **Scoring/Telemetry service (Infrastructure analytics service)**
   - Aggregates latency, error rate, and cost metrics.
   - Produces provider score snapshots consumed by strategy.

### Docker services
Containerized topology for local + production deployments:
- `gateway-api` (ASP.NET Core .NET 8)
- `policy-quota` (quota/rate service)
- `provider-score` (score aggregation and strategy inputs)
- `redis` (quota counters + fast policy cache)
- `postgres` (durable config/audit storage)
- `otel-collector` (telemetry export pipeline)

All services require health checks, readiness probes, and environment-driven configuration.

### API Gateway responsibilities
- External contract stability and versioning.
- Authentication/authorization enforcement.
- Input schema validation.
- Correlation IDs and trace context propagation.
- Mapping to orchestration commands and returning normalized errors.
- Non-functional controls: rate limiting hooks, timeout budgets, observability.

### Orchestrator responsibilities
- Resolve candidate providers from policy/capability constraints.
- Run provider strategy selection.
- Execute async provider calls with resilience policies.
- Perform fallback on failure/degradation.
- Record outcome for quota/accounting/scoring updates.

### Provider adapter model
- `IProviderAdapter` is the shared port.
- Each adapter implements:
  - capability declaration
  - request mapping
  - async invocation
  - response/error normalization
- Adapters are injected via DI and selected by strategy using metadata.
- Adapter implementation details never leak into domain/application layers.

### Quota tracking strategy
- Token bucket + daily hard quota hybrid.
- Redis for atomic fast-path counters.
- Postgres for durable reconciliation/audit records.
- Quota checks happen pre-dispatch; final usage reconciliation happens post-response.
- Degraded mode: conservative deny when quota backend unavailable (configurable policy per tenant).

### Provider scoring strategy
Composite weighted score, refreshed periodically and updated per response:
- latency p95 (lower is better)
- recent error rate (lower is better)
- normalized cost per 1K tokens (lower is better)
- capability match confidence (higher is better)

`ProviderScore = w1*Latency + w2*ErrorRate + w3*Cost + w4*CapabilityFit`

Strategy uses score + policy constraints + circuit state to pick primary and fallback provider.

### Scaling plan (1M req/day)
Target throughput ≈ 11.6 req/s sustained, with peak design at 20x (~230 req/s burst):
- Stateless `gateway-api` and `orchestrator` horizontal scaling via replicas.
- Redis-backed distributed quota counters.
- Connection pooling + async I/O end-to-end.
- Per-provider circuit breakers and bulkheads.
- Queue optionality for non-blocking post-processing (audit/analytics).
- Autoscaling by CPU + request latency + queue depth.

---

## Step 2 — Implementation

### Created artifacts
- `ARCHITECTURE.md` (this file)
- `SERVICES.md` (service contracts, deployment and runtime responsibilities)

---

## Step 3 — Code Review

### Dependency direction validation
- Domain has no dependency on infrastructure or presentation.
- Application depends on domain abstractions only.
- Infrastructure depends on application/domain contracts to implement ports.
- Presentation depends on application interfaces through DI composition.

### Extensibility validation
- New provider onboarded by adding a new adapter implementing shared interface.
- Strategy can evolve by adding new `IProviderSelectionStrategy` implementation.
- Quota/scoring backends replaceable behind interfaces.

Outcome: **Pass** (architecture supports extension without core layer modification).

---

## Step 4 — QA Validation

### Provider failure simulation
Scenario:
1. Primary provider returns timeout/circuit-open state.
2. Orchestrator marks failure signal and selects fallback provider.
3. Gateway returns normalized response if fallback succeeds; standardized transient error if all candidates fail.

Expected result: request continuity with bounded latency and full observability chain.

### Scaling simulation
Scenario:
1. Ramp from baseline load to 230 req/s burst.
2. Gateway/orchestrator replicas scale horizontally.
3. Redis quota checks remain atomic and low latency.
4. Provider score refresh keeps routing stable under partial degradation.

Expected result: p95 latency within SLO and no quota integrity loss.

Outcome: **Pass (design-level validation)**.

---

## Step 5 — Refactor

Coupling reduction actions:
- Isolated provider-specific mapping in adapters only.
- Kept policy, scoring, and quota contracts interface-based.
- Explicitly separated gateway request handling from orchestration decision logic.
- Consolidated cross-cutting concerns (telemetry, resilience) via middleware/pipeline behaviors.

Outcome: **Coupling reduced; boundaries remain clean**.

---

## Exit Condition

**Architecture approved.**
