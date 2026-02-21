# Architecture Evolution Proposal Report

## Scope and guardrails
- Language/runtime remains **C# on .NET 8**.
- **Clean Architecture boundaries are preserved** (`Api -> Application -> Domain <- Infrastructure via ports`).
- Evolution is **incremental**, **reversible**, and avoids full rewrites.
- Priority order: **stability first**, then scalability.

---

## Step 1 — Production metrics analysis

### Metric trend snapshot (from current telemetry model and operating targets)
Based on the gateway telemetry dimensions already collected (latency/success/failure/task-type/token-usage) and the current scaling target of 1M req/day with ~230 req/s burst design:

| Metric | Current trend signal | Interpretation |
|---|---|---|
| Request growth | Upward and approaching burst envelope during peak windows | Capacity margin is shrinking at peak even if daily average is acceptable |
| Latency trends | p95 tail inflation under peak (stable at baseline, degrades with concurrency) | Downstream/provider saturation and retry amplification are likely contributors |
| Retries | Increased retry/fallback activations during short degradation windows | Resilience works, but extra retries are adding load and latency |
| Queue depth | Pending post-processing/reconciliation work accumulates at spikes | Synchronous critical path is still healthy, but async backlog can spill into freshness delays |
| CPU usage | API/orchestration nodes show burst-driven CPU hotspots | Scale-out triggers may be too late or not multi-signal enough |
| Memory pressure | Moderate but rising during fallback storms | Transient object churn and buffered responses under failure paths |

### Net assessment
The system is not failing, but it is showing **pre-saturation behavior** under burst traffic and partial provider degradation.

---

## Step 2 — Detected architecture stress signals
1. **Retry amplification loop**: provider degradation causes retries/fallbacks, which increases load and worsens tail latency.
2. **Control-plane coupling pressure**: scoring/health/reconciliation freshness competes with request-path resources during spikes.
3. **Latency-tail sensitivity**: p95 is more volatile than average latency, signaling contention and queueing effects.
4. **Elasticity lag**: CPU-only autoscaling is insufficiently predictive for latency and queue backlog stress.

---

## Step 3 — Safe evolution proposal

### 3.1 Scaling strategy (incremental)
- Introduce **multi-signal autoscaling policy**: CPU + p95 latency + queue depth + 5xx ratio.
- Set conservative step scaling to avoid oscillation (small replica increments, shorter cool-down for scale-out, longer for scale-in).
- Keep API and orchestration stateless; no change to domain contracts.

### 3.2 Service split recommendation (safe seam, no rewrite)
- Keep current runtime behavior, but extract a **background control-plane worker** for:
  - provider score recalculation,
  - provider health aggregation,
  - reconciliation/reporting tasks.
- Request path remains in existing API/Application flow; only async background duties move behind existing interfaces.
- This is a **strangler-style split** of non-critical-path workloads, not a full microservice decomposition.

### 3.3 Caching improvements
- Add/strengthen **short TTL cache** for provider registry + provider health summaries.
- Add cache stampede protection (single-flight lock per provider/task key).
- Keep cache invalidation event-driven where possible, with fallback TTL expiry for safety.

### 3.4 Async pipeline improvements
- Move non-blocking audit/analytics/reconciliation writes onto a queue-backed worker path.
- Add bounded queues with clear backpressure policy (drop non-critical analytics first, never drop quota integrity events).
- Add dead-letter handling and replay tooling for safe recovery.

---

## Step 4 — Risk analysis

| Risk area | Risk level | Why | Mitigation | Rollback safety |
|---|---|---|---|---|
| Migration risk | Medium | New worker/queue and autoscaling tuning introduce operational complexity | Feature flags, canary rollout, expand-contract for schema changes | Immediate disable of worker path and fallback to current synchronous behavior |
| Rollback safety | Low | Proposed changes are additive and interface-preserving | Keep old code path enabled until parity verified | One-switch rollback per feature flag |
| Architecture consistency | Low | Domain/Application contracts remain unchanged | Enforce dependency rules and composition-root validation in CI | Revert composition registration only |

---

## Step 5 — Evolution scoring
Using `ScalabilityGain × StabilityImpact − ComplexityCost` on a 1..5 relative scale:

- ScalabilityGain = **4.0** (multi-signal scaling + async control-plane isolation)
- StabilityImpact = **4.5** (reduced retry storms on hot path, better backlog control)
- ComplexityCost = **2.5** (moderate ops/config complexity increase)

**Score = 4.0 × 4.5 − 2.5 = 15.5**

Interpretation: strong positive evolution value with acceptable complexity tradeoff.

---

## Step 6 — Phased rollout plan

### Phase 0 — Baseline hardening (1 sprint)
- Define SLOs and alert thresholds for p95 latency, retries, queue depth, CPU, memory.
- Add dashboards for retry amplification and fallback-rate correlation.
- No behavior changes yet.

### Phase 1 — Safe toggles + observability (1 sprint)
- Introduce feature flags for:
  - queue-based post-processing,
  - control-plane worker activation,
  - cache TTL profile.
- Deploy dark/inactive; verify telemetry and no regressions.

### Phase 2 — Control-plane offload (1–2 sprints)
- Activate worker for scoring/health/reconciliation in canary slice.
- Keep synchronous fallback path enabled.
- Success gate: no increase in error ratio, p95 improvement under peak.

### Phase 3 — Autoscaling policy upgrade (1 sprint)
- Enable multi-signal HPA/scale rules progressively.
- Tune thresholds from canary data; prevent thrash via hysteresis.

### Phase 4 — Cache and queue tuning (ongoing)
- Tune TTLs, queue bounds, and consumer concurrency.
- Add runbooks for DLQ replay and emergency disable.

### Rollback plan (all phases)
- Disable feature flags in reverse order.
- Keep previous composition and synchronous flows deployable.
- No destructive schema removals until post-stabilization window.

---

## Step 7 — Architecture rule validation

| Rule | Validation |
|---|---|
| .NET 8 / C# lock | Preserved |
| Clean Architecture | Preserved; no domain leakage |
| Safe evolution | Additive, seam-based changes only |
| Incremental change | Phased rollout with canary gates |
| Rollback always possible | Feature-flag + parallel path strategy |
| Stability over speed | Conservative scaling and fallback-first policy |

**Final recommendation**: proceed with phased, additive evolution focused on control-plane offload + multi-signal autoscaling + bounded async pipelines. Avoid broad service decomposition until these changes stabilize and metrics confirm sustained benefit.
