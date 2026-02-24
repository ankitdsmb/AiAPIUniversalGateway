# Repo Cleanup Plan (Safe-Delete First)

## Approach
- No blind deletion.
- Classify candidates as: `Keep`, `Verify Then Remove`, `Remove Safe`.

## Candidates

### Verify Then Remove
1. `src/UniversalAPIGateway.Api/Controllers/WeatherForecastController.cs`
2. `src/UniversalAPIGateway.Api/WeatherForecast.cs`
- Rationale: likely template leftovers; remove only after compile + route-map verification.

### Keep
1. Existing architecture and deployment docs in root and `/docs`
- Rationale: despite overlap, they provide historical context and decision trail.

2. `tests/UniversalAPIGateway.Application.Tests/UnitTest1.cs`
- Rationale: naming is generic, but file should only be removed/renamed after test-run verification.

## Pre-cleanup validation checklist
1. Build solution.
2. Run tests.
3. Run API and verify route table.
4. Confirm no docs/tools reference cleanup targets.

## Post-cleanup validation checklist
1. Build + tests green.
2. Swagger generation successful.
3. No broken links in docs.
