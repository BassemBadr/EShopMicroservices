# AGENTS.md

.NET 8 eShop-style microservices **learning repo** (not the official reference architecture). A Duende IdentityServer security track has been layered on top and is **in progress** — treat security gaps as planned work, not bugs (see "Known gaps").

## Layout & entry points

- Solution: `src/eshop-microservices.sln`. **All code lives under `src/`** (compose files and the solution are there, not at repo root).
- `src/Identity/IdentityServer` — Duende IdentityServer **7.4.12** (login UI + auth-code/OIDC + client_credentials). Must stay on 7.x; 8.x targets net10.
- `src/ApiGateways/YarpApiGateway` — YARP gateway = the public token-validation boundary (JwtBearer).
- `src/Services/{Catalog,Basket,Discount,Ordering}` — backend services, private on the Docker network but each validates tokens itself (defense-in-depth).
- `src/BuildingBlocks/BuildingBlocks` — shared lib (Carter, MediatR, FluentValidation, Mapster) **plus the shared auth extension** `Authentication/IdentityServerAuthenticationExtensions.cs`.
- `src/WebApps/Shopping.Web` — Razor-pages client; OIDC **not wired yet**.
- Quirk: `Ordering.API` registers auth + middleware inside `Ordering.API/DependancyInjection.cs` (**filename typo is intentional**) via `AddApiServices()`; the other services do it inline in `Program.cs`.

## Build / run / verify

- Build: `dotnet build src\eshop-microservices.sln`
- **No tests and no CI** (`.github/workflows/` is empty). Verification is manual:
  - Postman collections in `postman/` (environments: `Localhost`, `Docker`; the `Identity-Client-Token` request sets a `client-token` env var: client_credentials `yarp-gateway` / `gateway-secret`, all 4 API scopes).
  - Compose files are in `src/`: `docker compose up -d --build` from `src/`.
- Local-dev convention: services run in Docker; gateway/web/IdentityServer can run locally via `ASPNETCORE_ENVIRONMENT=Local` + `appsettings.Local.json` (Local gateway routes point at the dockerized backends' host ports).

## Port map (docker host ports)

| Service | Port | DB (host port) |
|---|---|---|
| catalog.api | 6000 | catalogdb postgres 5432 |
| basket.api | 6001 | basketdb postgres 5433 |
| discount.grpc | 6002 | SQLite (volume) |
| ordering.api | 6003 | orderdb mssql 1434 |
| yarpapigateway | 6004 | — |
| shopping.web | 6005 | — |
| identityserver | 6006 (6066 https) | identitydb mssql 1436 |
| — | redis 6379, rabbitmq 5672/15672 | — |

IdentityServer local `dotnet run`: http 5006 / https **5056**. Gateway local: 5004/5054. Web local: 5005/5055.

## Identity/security model (drives all changes)

- **JWT `aud` is always `{IdentityServer:Authority}/resources`** (`EmitStaticAudienceClaim = true`). Every validator must accept that exact audience — **never** a client name like `yarp-gateway` and never a per-API name. Re-issuing per-API audiences would require re-seeding the config DB.
- Duende stores: **built-in** `ConfigurationDbContext` / `PersistedGrantDbContext` (do NOT switch to derived contexts — DI base-options trap), migrations in `src/Identity/IdentityServer/Migrations/{ConfigurationDb,PersistedGrantDb}`.
- Seeding is **idempotent (only when tables are empty)**: changing `IdentityServerConfig.cs` does not update existing DB rows — delete rows/re-create the DB to re-seed.
- Clients: `shopping-web` (auth code + PKCE) and `yarp-gateway` (client_credentials). Dev login: `admin@eshop.com` / `P@ssw0rd1!`.
- To secure a service: `builder.Services.AddIdentityServerAuthentication(config)` (namespace `BuildingBlocks.Authentication`) + `UseAuthentication()`/`UseAuthorization()` before endpoint mapping. The extension sets **FallbackPolicy = RequireAuthenticatedUser** (protects every endpoint incl. gRPC) — health endpoints must be `.AllowAnonymous()`.
- Gateway adds `X-User-Id`/`X-User-Roles` forwarding headers. Services must read identity from the **validated JWT** (`HttpContext.User`), never trust those headers.

## Docker gotchas (Windows / Docker Desktop)

- `identitydb` **requires `user: "0:0"`** in `docker-compose.override.yml`: fresh named volumes are root-owned on Docker Desktop, and mssql 2025 runs as non-root → crash-loop "Access is denied copying master.mdf" without it. ⚠️ The comment above that line claims privileges drop back to `mssql` — **wrong**; `sqlservr` actually runs as root there.
- Volume-identity trap: `src_mssql_identity` (CLI `docker compose` from `src/`) vs `dockercompose<random-id>_mssql_identity` (Visual Studio `docker-compose.dcproj`) are **different named volumes** — DB data is not shared between run modes.
- SQL 2025 containers can crash with a non-yielding scheduler assert under Docker Desktop memory pressure (~1.5 GB); raise the VM memory or cap with `MSSQL_MEMORY_LIMIT_MB`.
- `orderdb` has **no volume** → data lost on every container recreate. Adding one will hit the same `user: "0:0"` requirement.
- SQL access: `sa` / `Strong@Passw0rd` (identitydb), `sa` / `SwN12345678` (orderdb).

## Verification one-liners

```powershell
# token (client_credentials)
curl -k -X POST https://localhost:6066/connect/token -d "grant_type=client_credentials&client_id=yarp-gateway&client_secret=gateway-secret&scope=catalog-api basket-api ordering-api discount-api"
# gateway 401 without token / 200 with Bearer
curl http://localhost:6004/catalog-service/products
# service direct 401 (defense-in-depth check)
curl http://localhost:6000/products
# inspect identitydb
docker exec identitydb /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Strong@Passw0rd" -d IdentityDb -Q "SELECT TOP 5 * FROM Clients"
```

## Known gaps / planned work (don't "fix" silently)

- `Shopping.Web` OIDC login/logout not implemented — the web app currently cannot call the protected APIs.
- Basket→Discount gRPC calls send **no token**, so they 401 now that Discount is secured — expected; token forwarding is planned integration work.
- `AddDeveloperSigningCredential()` is dev-only; Duende key-management license warnings in logs are benign in dev.

The canonical, step-by-step record of the security track (phases, decisions, incidents) lives at `C:\Users\CFast\.opencode\plan\eShop-IdentityServer-Implementation-Plan.md` — read it before resuming identity work.