# eShop Microservices

An eShop-style e-commerce system built as a **.NET 8 microservices learning repo**, with a **Duende IdentityServer 7** security track layered on top. It covers the full shopping flow — browse the catalog, manage a cart, apply discounts, check out, and create orders — spread across independently deployable services behind a YARP API gateway.

> This is a personal learning project modeled on the eShop reference architecture (not a copy of it), plus an in-progress OIDC/JWT security implementation.

---

## Architecture

```
                                ┌──────────────────────────┐
                                │      Shopping.Web        │  Razor Pages (OIDC login,
                                │   (BFF / web frontend)   │  Refit clients + bearer token)
                                └────────────┬─────────────┘
                                             │ HTTPS + Bearer token
                              ┌──────────────▼──────────────┐
                              │      YarpApiGateway         │  JWT validation, route/cluster
                              │  (public token boundary)    │  transforms, rate limiting
                              └───┬───────────┬─────────┬───┘
                 /catalog-service │           │ /basket-service      │ /ordering-service
                      ┌───────────▼──┐  ┌─────▼──────┐  ┌────────────▼───────────┐
                      │ Catalog.API  │  │ Basket.API │  │     Ordering.API       │
                      │ Marten +     │  │ Marten +   │  │  EF Core (SQL Server)  │
                      │ PostgreSQL   │  │ Redis      │  │  DDD / CQRS / Clean    │
                      └──────────────┘  └──┬─────┬───┘  └───────────▲────────────┘
                                           │     │ gRPC + token      │ MassTransit /
                                    ┌──────▼───┐ │ forward           │ RabbitMQ
                                    │Discount  ◄┘                   │
                                    │.Grpc     │     BasketCheckoutEvent
                                    │ SQLite   │─────────────────────┘
                                    └──────────┘
        ┌───────────────────────────────────────────────────────────────┐
        │                     Infrastructure                           │
        │  identitydb (MSSQL) · catalogdb/basketdb (PostgreSQL)         │
        │  orderdb (MSSQL) · Redis · RabbitMQ                          │
        └───────────────────────────────────────────────────────────────┘
                     ▲
                     │ issues JWTs (auth code + PKCE, client credentials)
        ┌────────────┴─────────────┐
        │  IdentityServer (Duende) │  login UI, /connect/*, /.well-known/*
        └──────────────────────────┘
```

### Services

| Service | Tech | Data store | Responsibility |
|---|---|---|---|
| `catalog.api` | Minimal APIs + Carter, MediatR, Marten | PostgreSQL | Product catalog CRUD, categories, pagination, dev seed data |
| `basket.api` | Minimal APIs + Carter, MediatR, Marten, Redis cache | PostgreSQL + Redis | Shopping cart store/read/checkout, cache-aside decorator |
| `discount.grpc` | gRPC + EF Core | SQLite | Coupon/discount lookup (CRUD over `discount.proto`) |
| `ordering.api` | Carter, MediatR, EF Core, DDD/CQRS/Clean Architecture | SQL Server | Order placement, querying, domain events, auto-migrate + seed |
| `yarpapigateway` | YARP reverse proxy, JwtBearer | — | Public boundary: token validation, routing, header forwarding, rate limiting |
| `identityserver` | Duende IdentityServer 7 + ASP.NET Core Identity | SQL Server | Login UI, OIDC/OAuth 2.0 token issuance, clients/scopes/grants |
| `shopping.web` | Razor Pages + Bootstrap 5, Refit, OIDC | — | Storefront: browse, cart, checkout, orders, sign-in/sign-out |
| `buildingblocks*` | Shared libs | — | Auth extension, Carter/MediatR/FluentValidation/Mapster wiring, messaging |

### Solution layout

```
src/
├── eshop-microservices.sln        # all code lives under src/
├── ApiGateways/YarpApiGateway/    # YARP gateway (public token boundary)
├── BuildingBlocks/                # BuildingBlocks + BuildingBlocks.Messaging
├── Identity/IdentityServer/       # Duende IdentityServer 7 (login UI + auth server)
├── Services/
│   ├── Basket/Basket.API/
│   ├── Catalog/Catalog.API/
│   ├── Discount/Discount.Grpc/
│   └── Ordering/{Ordering.API, Ordering.Application, Ordering.Domain, Ordering.Infrastructure}
├── WebApps/Shopping.Web/          # Razor Pages storefront
└── docker-compose.yml / docker-compose.override.yml
postman/                           # collections + Localhost/Docker environments
```

---

## Features

### Architecture & patterns
- **.NET 8 / C# 12** minimal APIs with **vertical slice (feature folder)** structure
- **CQRS** with **MediatR**, **FluentValidation** pipeline behaviours (validation + logging) and **Mapster** mapping
- **Carter** for minimal-API endpoint definition per feature
- **DDD tactical patterns** in Ordering: entities, value objects, aggregates/aggregate roots, domain events, plus a **Clean Architecture** layer split (Domain / Application / Infrastructure / API)
- **Marten** transactional document sessions on PostgreSQL (Catalog & Basket) with lightweight sessions; Catalog seeds sample products in Development
- **EF Core code-first** with migrations, DDD entity configurations and an audit interceptor (`Created*`/`LastModified*`); schema is applied on startup (migrations + seed data for Ordering and IdentityServer, migrations for Discount)
- **Cache-aside + decorator patterns**: `CachedBasketRepository` wraps the Marten repository via Scrutor, backed by **Redis** as a distributed cache
- **Pagination** building block, global **exception handling** (`CustomExceptionHandler`), structured **logging behaviours**
- **Feature flags** with `Microsoft.FeatureManagement` (e.g. `OrderFullfilment` gate on the order-created domain event)

### Security (Duende IdentityServer 7 — the security track)
- **OIDC login for Shopping.Web**: authorization code + **PKCE** (public client, no client secret), cookie + OIDC schemes, refresh tokens (`offline_access`), role/name claim mapping, sign-in/sign-out pages
- **Token propagation**: `UserAccessTokenHandler` attaches the user's access token to every Refit call through the gateway
- **Client credentials** machine tokens for the gateway (`yarp-gateway` / `gateway-secret`) — used by tooling/Postman
- **Gateway token validation**: JwtBearer with `aud = {authority}/resources` (`EmitStaticAudienceClaim`), `X-User-Id` / `X-User-Roles` request transforms
- **Defense in depth**: every backend service (incl. gRPC) validates JWTs itself through the shared `AddIdentityServerAuthentication` extension, with a **secure-by-default fallback policy** (all endpoints authenticated unless `[AllowAnonymous]`, e.g. `/health`)
- **gRPC token forwarding**: Basket → Discount calls forward the caller's `Authorization` header via gRPC call credentials
- **API scopes** `catalog-api`, `basket-api`, `ordering-api`, `discount-api`; identity resources `openid`, `profile`, `email`, `roles`
- **Duende stores** in SQL Server (`ConfigurationDbContext` / `PersistedGrantDbContext`), idempotent startup seeding, roles `Admin`/`Customer`/`Manager`
- Gateway **rate limiting** with `AddFixedWindowLimiter` (fixed-window policy on the ordering route)
- Dev login: **`admin@eshop.com` / `P@ssw0rd1!`**

### Async messaging
- **MassTransit over RabbitMQ** (kebab-case endpoints, topic publish/subscribe)
- Basket publishes **`BasketCheckoutEvent`** on checkout; **Ordering consumes it** and creates the order through MediatR
- Domain event `OrderCreatedEventHandler` with feature-flag gating

### Inter-service communication
- **gRPC** Basket → Discount for synchronous price/discount calculation (proto contract in `discount.proto`, SQLite-backed coupons)

### Gateway & web
- **YARP** route/cluster/path-transform configuration: `/catalog-service`, `/basket-service`, `/ordering-service` → container destinations
- **Rate limiting** (`fixed-window-policy`) and per-route authorization policies
- **Shopping.Web**: catalog browsing (list/detail/category), cart, checkout, order list/confirmation, login/logout — calling the gateway via **Refit** typed `HttpClientFactory` clients
- Bootstrap 5 storefront with banner carousel and product images

### Cross-cutting & DevOps
- **Health checks** on Catalog, Basket, Ordering and IdentityServer (`/health`, HealthChecks UI JSON writer) — DB-aware (Npgsql/Redis/SqlServer), always `AllowAnonymous`
- **Cross-cutting logging** via MediatR logging behaviours; global exception handling returns consistent error responses
- **Dockerfiles for all apps** + multi-container **docker-compose** (with DB health checks and dependency ordering)
- **Postman collections & environments** (`Localhost` / `Docker`) incl. a `client-token` request for client-credentials tokens

---

## Getting started

### Prerequisites
- .NET 8 SDK
- Docker Desktop (containers run the databases, broker, cache and services)

### Build

```powershell
dotnet build src\eshop-microservices.sln
```

### Run everything (Docker)

```powershell
cd src
docker compose up -d --build
```

Compose files live in **`src/`** (not the repo root).

### Ports

| Service | Host HTTP | Host HTTPS | Notes |
|---|---|---|---|
| catalog.api | 6000 | 6060 | catalogdb Postgres → 5432 |
| basket.api | 6001 | 6061 | basketdb Postgres → 5433 |
| discount.grpc | 6002 | 6062 | SQLite volume |
| ordering.api | 6003 | 6063 | orderdb MSSQL → 1434 |
| yarpapigateway | 6004 | 6064 | public API entry point |
| shopping.web | 6005 | 6065 | storefront |
| identityserver | 6006 | 6066 | identitydb MSSQL → 1436 |
| redis / rabbitmq | 6379 / 5672 | — | RabbitMQ UI → 15672 |

Running apps **locally** (`dotnet run` / launchSettings): catalog 5000/5050 · basket 5001/5051 · discount 5002/5052 · ordering 5003/5053 · gateway 5004/5054 · web 5005/5055 · identity 5006/5056.
The gateway/web/IdentityServer also support a `Local` environment (`ASPNETCORE_ENVIRONMENT=Local` + `appsettings.Local.json`) whose routes point at the dockerized backends' host ports.

### Verify it works

```powershell
# 1. Get a machine token (client_credentials: yarp-gateway / gateway-secret)
curl -k -X POST https://localhost:6066/connect/token `
  -d "grant_type=client_credentials&client_id=yarp-gateway&client_secret=gateway-secret&scope=catalog-api basket-api ordering-api discount-api"

# 2. Through the gateway (401 without token, 200 with Bearer)
curl http://localhost:6004/catalog-service/products
curl -H "Authorization: Bearer <token>" http://localhost:6004/catalog-service/products

# 3. Defense-in-depth: services reject direct calls too
curl http://localhost:6000/products
```

Or open the storefront at `https://localhost:6065` and sign in with the dev credentials above. `postman/` contains ready-made collections for every endpoint (direct + gateway), health checks, gRPC and token requests.

---

## Project status & known limitations

This repo is a **learning project**; security work is treated as planned, in-progress track:

- **No automated tests and no CI** (`.github/workflows/` is empty) — verification is manual via Postman + the curl checks above.
- `orderdb` has **no Docker volume** → order data is lost when the container is recreated (`identitydb`/`catalogdb`/`basketdb` do persist).
- `AddDeveloperSigningCredential()` is **dev-only**; swap for a real certificate/keys before any real deployment. Duende key-management license warnings in logs are benign in dev.
- IdentityServer seeding is **idempotent (only when tables are empty)** — changing `IdentityServerConfig.cs` won't update an existing DB; re-create the DB to re-seed.
- Client redirect URIs are hardcoded for local dev (documented in `IdentityServerConfig.cs`).
- Seeding/config assumes **local development** — credentials and connection strings in this repo are not production-safe.

## License

[MIT](LICENSE) © 2025 BassemBadr
