# NuGet Restore Under Restricted Proxy Networks

This repository pins `nuget.org` in `NuGet.config` and documents a deterministic fallback flow for environments where outbound access is restricted.

## Baseline checks

```bash
dotnet nuget list source
```

Expected source:

- `https://api.nuget.org/v3/index.json`

## Clear local caches

```bash
dotnet nuget locals all --clear
```

## Proxy-aware restore flow

1. Try normal restore first:

```bash
dotnet restore --disable-parallel
dotnet restore --no-cache
```

2. If your proxy blocks `nuget.org`, temporarily restore without inherited proxy variables:

```bash
env -u HTTP_PROXY -u HTTPS_PROXY -u http_proxy -u https_proxy dotnet restore --disable-parallel
```

3. If the environment is fully offline, seed `./local-packages` with required `.nupkg` files (including `Swashbuckle.AspNetCore` and transitive dependencies), then add that source:

```bash
dotnet nuget add source ./local-packages -n local
dotnet restore --disable-parallel
```

## Validate

```bash
dotnet restore
dotnet build
```
