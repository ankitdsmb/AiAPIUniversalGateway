# Updated Routing Intelligence Report

## Current state
The gateway now captures richer production telemetry per `(provider, task type)` and uses it to improve routing in a safe, reversible loop.

## 1) Collected production metrics
For every request outcome, the adaptive loop now records:
- provider
- latency
- success/failure
- task type
- token usage (estimated from request/response payload length)

## 2) Pattern analysis
The adaptive scoring model continuously analyzes:
- best provider per task type
- failure trends via EMA failure rate
- latency patterns via EMA latency
- token efficiency via EMA token usage

## 3) Automatic scoring update
Provider routing score now combines:
- success rate
- response quality
- latency score
- token efficiency score
- stability score (`1 - failure rate`)

This lets production telemetry improve scoring automatically over time.

## 4) Safe rollout
Rollout remains gradual by design:
- existing exploration rate (10%) prevents hard lock-in
- confidence gating blends observed score with baseline until enough samples exist

This keeps updates reversible and reduces sudden routing swings.

## 5) Validation and fallback safety
Fallback execution path remains unchanged in behavior:
- failed providers are excluded from the current attempt chain
- adaptive outcomes are recorded on both success and failure
- if no fallback exists, original error propagates as before

## 6) Continuous learning loop
Each request updates per-task provider telemetry and future scores, creating an always-on closed loop:
collect -> analyze -> score -> route -> validate -> repeat.
