# Future Risks Register

## HIGH

1. **Non-reproducible CI/local setup if SDK toolchain is missing**
   - If .NET SDK availability is not enforced, build/test drift can go undetected.
   - Mitigation: pin SDK via `global.json` and validate in CI bootstrap.

2. **Default runtime may diverge from intended production architecture**
   - API currently composes local defaults; infra path may receive less real-world exercise.
   - Mitigation: explicit environment-driven composition profile and startup diagnostics showing active providers/storage.

## MEDIUM

1. **Operational complexity in provider adapter matrix**
   - Many adapters/options increase config and incident blast radius.
   - Mitigation: startup validation of provider config, health probes, and adapter registration diagnostics.

2. **Persistence + telemetry growth management**
   - Infrastructure includes many telemetry/performance entities; retention and index strategy must be monitored.
   - Mitigation: retention jobs, partition strategy, and performance SLO monitoring.

## LOW

1. **Legacy placeholder artifacts**
   - Non-core files/controllers increase cognitive load.
   - Mitigation: remove after confirming no external dependency.
