# Provider Intelligence Architecture

## Scoring Model

The gateway now supports score-based provider selection through `IProviderScoringService`.

```
Score =
(successRate * SuccessRateWeight)
+ (quotaRemaining * QuotaRemainingWeight)
- (latency * LatencyWeight)
- (recentFailures * RecentFailuresWeight)
```

By default, weights map to:
- success rate: `50`
- quota remaining: `30`
- latency: `10`
- recent failures: `20`

All weights and normalization limits are configured via `ProviderIntelligence` settings.

## Data Model

Redis keys:
- `provider_health:{id}` (hash): keeps request outcomes and health metrics.
- `provider_score:{id}` (string): latest computed score.

Tracked metrics:
- success rate (`successes` / `totalRequests`)
- average latency (`totalLatencyMs` / `totalRequests`)
- recent failures (`recentFailures`)
- quota (`quotaRemaining`, `quotaLimit`)

## Clean Architecture Placement

- Domain: scoring contract in `IProviderScoringService`.
- Application: strategy (`DefaultProviderSelectionStrategy`) consumes scores but does not know storage details.
- Infrastructure: `ProviderIntelligenceEngine` handles Redis + quota integration.

This keeps scoring implementation isolated while still enabling strategy-driven selection.
