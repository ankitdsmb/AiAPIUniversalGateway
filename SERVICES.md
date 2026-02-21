# Services Specification

## 1. Service Catalog

## gateway-api
- **Type:** ASP.NET Core .NET 8 Web API
- **Responsibilities:**
  - Unified external API
  - AuthN/AuthZ
  - Validation and normalization
  - Correlation and telemetry propagation
  - Delegation to orchestrator use cases
- **Scaling:** Stateless, horizontal replicas
- **Depends on:** orchestrator interfaces, quota/policy endpoint, scoring endpoint

## orchestrator (logical application service, deployable with gateway or standalone)
- **Type:** Application service layer
- **Responsibilities:**
  - Provider candidate resolution
  - Strategy execution for provider selection
  - Async invocation + fallback orchestration
  - Unified result shaping for gateway
- **Depends on:** provider adapter ports, quota contract, scoring contract

## policy-quota
- **Type:** Internal service
- **Responsibilities:**
  - Tenant/application quota policies
  - Rate and daily limit evaluation
  - Reconciliation events for usage accounting
- **Data stores:** Redis (hot counters), Postgres (durable ledger)

## provider-score
- **Type:** Internal analytics service
- **Responsibilities:**
  - Compute provider scores from latency, error, cost, capability fit
  - Serve score snapshots to orchestrator strategy
  - Maintain short-horizon and long-horizon rolling windows

## provider adapters
- **Type:** Infrastructure components (in-process initially)
- **Responsibilities:**
  - Provider-specific protocol translation
  - Async external calls
  - Error normalization into unified taxonomy
- **Pattern:** Adapter pattern; one adapter per provider

---

## 2. Runtime Interaction Flow
1. Client -> `gateway-api`
2. `gateway-api` validates/authenticates and requests quota pre-check
3. `gateway-api` calls orchestrator use case
4. Orchestrator pulls score snapshot + capabilities and executes strategy selection
5. Orchestrator invokes primary adapter asynchronously
6. On failure/degradation, orchestrator invokes fallback adapter
7. Gateway returns normalized response
8. Usage reconciliation + telemetry emitted asynchronously

---

## 3. Strategy Model

Provider selection strategy combines:
- hard constraints (capability, compliance, tenant policy)
- dynamic score (latency/error/cost/capability fit)
- resilience state (circuit breaker status)

This is implemented by `IProviderSelectionStrategy` so alternate strategies can be introduced without modifying gateway controllers or adapters.

---

## 4. Docker and Deployment Baseline
- `gateway-api`: externally exposed
- `policy-quota`: internal network only
- `provider-score`: internal network only
- `redis`: internal
- `postgres`: internal persistent volume
- `otel-collector`: internal telemetry pipeline

Deployment requirements:
- healthcheck + readiness for every service
- environment-only configuration
- secrets through secure secret providers
- rolling deployment with zero-downtime readiness gating

---

## 5. 1M Requests/Day Capacity Notes
- Baseline 11.6 req/s, burst target 230 req/s.
- Recommended starting topology:
  - gateway-api: 3 replicas
  - orchestrator: 3 replicas (if split)
  - policy-quota: 2 replicas
  - provider-score: 2 replicas
  - redis: HA setup
  - postgres: managed HA or primary/replica
- Autoscaling signals:
  - CPU + memory
  - p95 latency
  - 5xx ratio
  - queue depth / pending reconciliations

---

## 6. Validation Checklist
- Clean architecture dependency direction preserved.
- SOLID + DI enforced through interface boundaries.
- Adapter and strategy patterns explicitly represented.
- Async-first call chain required in all provider paths.
- Docker-ready deployment and scaling plan defined.


## 7. Failure and Scale QA Scenarios
### Provider failure drill
- Trigger: primary provider times out for 30s window.
- Expected orchestration behavior: strategy excludes unhealthy provider using resilience state and routes to fallback adapter.
- Pass criteria: unified API contract preserved, request returns either successful fallback response or normalized transient error.

### Scale drill (1M/day with burst)
- Trigger: synthetic load ramps from 12 req/s to 230 req/s.
- Expected platform behavior: gateway and orchestrator scale horizontally, quota consistency remains atomic, score service freshness maintained.
- Pass criteria: p95 latency remains within SLO, no quota ledger mismatches, and no dependency inversion violations introduced.
