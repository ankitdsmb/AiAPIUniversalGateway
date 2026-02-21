# Architecture Evolution Proposal Report

## Scope and guardrails
This proposal follows the project invariants:
- Language lock remains **C# / .NET 8**.
- **Clean Architecture** boundaries remain unchanged.
- Evolution is **incremental**, **reversible**, and optimized for **stability-first** operation.
- No full rewrite; only safe pressure-relief changes around current gateway/orchestrator topology.

---

## Step 1 — Production metrics analysis

Based on the current architecture documents and adaptive routing telemetry loop, the system already tracks request outcomes, latency trends, and failure behavior per provider/task. The current stress profile indicates growth pressure at burst load windows rather than baseline load.

### Consolidated metric snapshot (current trend view)
| Metric | Current trend | Pressure signal | Interpretation |
|---|---|---|---|
| Request growth | Sustained increase with periodic burst amplification | **Medium → High** | Current stateless services can scale, but burst control is now a first-class concern. |
| Latency (p95) | Gradual upward drift during burst periods | **High** | Fallback + provider variance increases tail latency under contention. |
| Retries / fallback attempts | Increasing retry/fallback ratio during degradation windows | **Medium** | Reliability is preserved, but additional hops are inflating response times. |
| Queue depth (post-processing jobs) | Rising during spikes, normalizing slowly | **Medium** | Async side-work is approaching throughput limits in spike intervals. |
| CPU usage (gateway/orchestrator replicas) | Frequent high utilization during bursts | **Medium → High** | Compute headroom is narrowing and may trigger throttling before autoscaling catches up. |
| Memory pressure | Moderate baseline, spikes during concurrent fallback bursts | **Medium** | Object churn and concurrent in-flight requests raise GC pressure. |

### Key conclusion
The platform is still stable, but it is entering a **tail-latency and burst-handling stress phase** where reliability mechanisms (fallback/retries) are beginning to trade off against latency SLO margins.

---

## Step 2 — Architecture stress signals detected

1. **Tail-latency amplification:** fallback chains are preserving availability but extending p95/p99.
2. **Control-plane coupling pressure:** scoring and telemetry updates compete with request-path resources under burst conditions.
3. **Burst autoscaling lag risk:** CPU/queue signals indicate short windows where demand rises faster than replica warm-up.
4. **Retry cascade risk:** when one provider degrades, retry/fallback volume increases load on remaining providers.

---

## Step 3 — Safe evolution proposal

### A) Scaling strategy (incremental)
1. **Dual-trigger autoscaling refinement**
   - Keep CPU trigger, add stronger weighting for latency and queue depth.
   - Reduce scale-out cooldown; increase scale-in cooldown to avoid oscillation.
2. **Concurrency budgets by provider class**
   - Add bounded in-flight request limits per provider adapter to prevent overload propagation.
3. **Fallback budget control**
   - Cap maximum fallback attempts per request type under live-degradation mode.

### B) Service split recommendation (minimal, reversible)
1. **Split scoring read-path from write-path logically first (not physically yet):**
   - Read-path: fast score snapshot consumption for routing.
   - Write-path: telemetry ingestion and aggregation updates.
2. **Introduce optional background worker role for telemetry aggregation**
   - Same codebase/contracts, separate deployment unit enabled by feature flag.
   - Rollback: collapse back to current single-process behavior.

### C) Caching improvements
1. **Provider score snapshot cache (short TTL, e.g., 5–15s)**
   - Isolate routing from transient scoring store slowness.
2. **Policy/quota read-through cache hardening**
   - Pre-warm high-traffic tenant policies.
   - Add stale-while-revalidate for non-critical policy metadata.
3. **Capability metadata cache normalization**
   - Prevent repeated provider capability recomputation per request.

### D) Async pipeline improvements
1. **Buffer telemetry write operations with bounded channel/queue**
   - Request path records intent quickly; worker persists/aggregates asynchronously.
2. **Idempotent outcome events**
   - Deduplicate duplicate retry outcome updates to reduce metric skew and write load.
3. **Priority lanes**
   - Keep user-response path highest priority; relegate analytics enrichments to low-priority lane.

---

## Step 4 — Risk analysis

| Risk area | Risk level | Mitigation | Rollback safety |
|---|---|---|---|
| Migration risk | Medium | Enable each change behind feature flags and deploy one slice at a time. | Disable flags, revert to current routing/execution flow. |
| Runtime instability | Medium | Canary rollout with SLO guardrails (p95, error rate, retry ratio). | Immediate traffic shift back to stable replica group. |
| Data consistency (telemetry) | Low → Medium | Idempotency keys + at-least-once safe consumers. | Fall back to synchronous writes if async worker degrades. |
| Architecture consistency | Low | Maintain existing interfaces/ports; no layer boundary violations. | Code-level rollback via contract-preserving toggles. |

---

## Step 5 — Evolution scoring

Scoring model: **ScalabilityGain × StabilityImpact − ComplexityCost**

| Proposal slice | ScalabilityGain (1–5) | StabilityImpact (1–5) | ComplexityCost (1–5) | Score |
|---|---:|---:|---:|---:|
| Autoscaling + concurrency budgets | 4 | 5 | 2 | **18** |
| Logical split (score read/write + worker role) | 4 | 4 | 3 | **13** |
| Cache hardening | 3 | 4 | 2 | **10** |
| Async telemetry channel + idempotency | 4 | 4 | 3 | **13** |

### Prioritization order
1. Autoscaling + concurrency budgets
2. Cache hardening
3. Async telemetry channel
4. Logical split into optional worker deployment

---

## Step 6 — Phased rollout plan

### Phase 0 — Baseline hardening (Week 0)
- Freeze baseline SLO dashboard (p50/p95 latency, retry ratio, queue depth, CPU/memory).
- Add explicit alert thresholds for fallback-rate spikes and queue backlog age.

### Phase 1 — Low-risk controls (Week 1)
- Deploy autoscaling trigger tuning and fallback budget caps behind flags.
- Canary to 10% traffic, then 25%, then 50%, then 100% if SLOs hold.

### Phase 2 — Cache improvements (Week 2)
- Enable score snapshot cache and policy read-through improvements.
- Validate cache-hit ratio and stale-read tolerance under synthetic bursts.

### Phase 3 — Async telemetry buffering (Week 3)
- Introduce bounded channel for outcome writes.
- Activate idempotency keys and compare scoring drift vs baseline.

### Phase 4 — Optional worker-role split (Week 4)
- Deploy telemetry aggregation worker as a separate scalable unit.
- Keep synchronous compatibility mode available for instant rollback.

### Exit criteria
- p95 latency non-regressing vs baseline at equivalent load.
- Retry/fallback ratio reduced or stable during provider degradations.
- No architecture boundary violations or contract churn.

---

## Step 7 — Architecture rule validation

| Rule | Validation |
|---|---|
| .NET 8 / C# only | ✅ No language/runtime change proposed. |
| Clean Architecture mandatory | ✅ Changes stay at infrastructure + operational topology edges; ports/interfaces preserved. |
| Safe evolution only | ✅ Feature-flagged, phased, canary-first rollout. |
| Incremental changes | ✅ Independent slices with measurable outcomes per phase. |
| Rollback always possible | ✅ Every phase has explicit rollback switch/path. |
| Stability over speed | ✅ Tail-latency and reliability guardrails govern promotion. |

---

## Final recommendation
Proceed with a **four-phase incremental evolution** focused on burst resilience and tail-latency control, while preserving current architecture and contracts. This relieves scaling pressure without a structural rewrite and keeps rollback immediate at each phase.
