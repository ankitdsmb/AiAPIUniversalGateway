# Bugs / Half-Implemented Areas (Audit)

## Confirmed / high-confidence findings

1. **Environment bootstrap blocker**
   - `dotnet` CLI is not available in this execution environment, preventing build/test/runtime validation.
   - Attempting to install via `apt-get` fails due upstream/proxy `403 Forbidden` on Ubuntu repositories.
   - Impact: no executable verification possible in current session.

2. **Runtime composition gap risk**
   - API startup uses `AddApiRuntimeDefaults()` only (local echo + in-memory registry/scoring/cache).
   - Infrastructure composition (`AddInfrastructure`) exists separately but is not wired in API startup.
   - Impact: production-like dependencies (Postgres/Redis/real provider adapters/jobs) are not activated by default.

3. **Potential legacy placeholders still present**
   - `WeatherForecastController` and `WeatherForecast` artifacts are still in the API project and appear non-core to gateway behavior.
   - Impact: surface-area noise and maintenance overhead.

## Medium-confidence observations (need runtime confirmation)
- Background jobs and DB integrations exist in Infrastructure but cannot be validated without executable environment.
- End-to-end provider fallback behavior appears implemented in application services and tests, but runtime behavior cannot be confirmed in-session.
