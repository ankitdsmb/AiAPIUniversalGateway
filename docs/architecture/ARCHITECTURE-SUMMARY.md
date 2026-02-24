# Architecture Summary (Verified from Source)

## Scope and verification method
This summary is based on direct source inspection of the solution and runtime composition files.

## Technology stack
- .NET 8 solution with C# projects for Domain, Application, Infrastructure, API, and test layers.
- ASP.NET Core Web API entry point in `src/UniversalAPIGateway.Api/Program.cs`.
- Clean-architecture style layering reflected in solution composition.

## Runtime composition and execution flow
1. **API host startup**
   - `Program.cs` wires controllers, Swagger, request adapter, application services, and API runtime defaults.
2. **Request entry**
   - `GatewayController` receives `POST v1/ai/execute` payloads.
   - Request is validated/adapted by `IGatewayRequestAdapter`.
3. **Application orchestration**
   - `IGatewayService` forwards to `IOrchestratorService`.
   - `OrchestratorService` selects a primary provider via `IProviderSelectionEngine`, executes with fallback via `IFallbackHandler`, then normalizes via `IResponseNormalizer`.
4. **Provider abstraction**
   - Providers are consumed through `IProviderAdapter` contracts.
   - Current API default composition registers local/in-memory adapters and services (not full infrastructure wiring by default).

## Module interaction map
- **API layer** depends on Application abstractions and adapters.
- **Application layer** depends on Domain ports/entities and internal service abstractions.
- **Infrastructure layer** provides persistence, cache, external provider adapters, resilience policies, and background jobs.
- **Domain layer** contains core entities and ports.

## Data flow
- Input JSON -> contract (`ExecuteAiRequest`) -> gateway domain request via adapter.
- Domain request -> selection + fallback orchestration across registered provider adapters.
- Provider raw result -> normalization -> API response contract (`ExecuteAiResponse`).

## Important architectural observation
- The API project currently references the Application project only, and startup uses `AddApiRuntimeDefaults()` (local echo + in-memory services). Full `AddInfrastructure(...)` composition exists but is not currently wired into the API startup path.
