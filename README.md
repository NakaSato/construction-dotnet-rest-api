# Solar Projects API

REST + real-time backend for solar construction project management. ASP.NET Core Web API on **.NET 10**, serving a Flutter mobile app and web clients.

Manages projects, work-breakdown structures, tasks, daily/weekly reports, work requests with approvals, calendar scheduling, image uploads, and live notifications.

---

## Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`), ASP.NET Core Web API |
| Data | EF Core 10 + PostgreSQL (Npgsql); in-memory provider for tests/Docker |
| Auth | JWT bearer + BCrypt hashing, role-based, refresh-token rotation |
| Real-time | SignalR (`NotificationHub`) |
| Mapping / validation | AutoMapper 16, FluentValidation 11 |
| Versioning | `Asp.Versioning.Mvc` (v1 default) |
| Rate limiting | Redis-backed, optional |
| Storage | Local `uploads/` served at `/files`; AWS S3 SDK available |
| Docs | Swagger / Swashbuckle (Development + Docker only) |

## Prerequisites

- .NET SDK **10.0.301**
- PostgreSQL — or skip it with `USE_IN_MEMORY_DB=true`
- Redis — only if `RateLimit:Enabled` is true

> `dotnet` may not be on PATH. On this machine use the full path: `/usr/local/share/dotnet/dotnet`.

## Quick start

```bash
cp .env.example .env          # set JWT_KEY (32+ chars) and the connection string
dotnet build
dotnet run --urls "http://localhost:5001"
```

Swagger UI is served at the **root**: <http://localhost:5001>

No database handy? Run fully in memory:

```bash
USE_IN_MEMORY_DB=true dotnet run --urls "http://localhost:5001"
```

### Docker

```bash
docker compose up --build      # API on :5001, PostgreSQL on :5432
docker compose -f docker-compose.dev.yml up   # dev profile
```

The image itself defaults to `USE_IN_MEMORY_DB=true` so it boots on hosts with no
attached database. Both compose files override it to `false` and use their PostgreSQL
service; set `CONNECTIONSTRINGS__DEFAULT` and `USE_IN_MEMORY_DB=false` to do the same
anywhere else. It also sets `DOTNET_USE_POLLING_FILE_WATCHER=true` — hosts with a low
inotify limit crash on startup otherwise.

Deploy helpers: `scripts/deploy-docker.sh`, `scripts/docker-deploy-test.sh`. Azure configs live in `azure/`.

### Seeded admin

`Services/Infrastructure/DataSeeder.cs` creates:

- **admin@example.com** / `Admin123!`

Roles are seeded in `Data/ApplicationDbContext.cs`: `1 Admin`, `2 Manager`, `3 User`, `4 Viewer`.

## Authentication

```bash
# Login -> access token + refresh token
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}'

# Authenticated call
curl http://localhost:5001/api/v1/projects -H "Authorization: Bearer $TOKEN"
```

`POST /api/v1/auth/refresh` rotates the refresh token; reuse of a rotated token is detected and rejected. `POST /api/v1/auth/logout` blacklists the access token (`JwtBlacklistMiddleware`).

## Response envelope

Every controller response is wrapped in `ApiResponse<T>` (`DTOs/ApiResponse.cs`):

```json
{
  "success": true,
  "message": "Success",
  "data": { },
  "errors": [],
  "error": null
}
```

Services return `Result<T>` / `ServiceResult<T>` instead of throwing; `BaseApiController` maps them to the envelope and the right HTTP status.

## API surface

Routes are versioned: `/api/v1/...`. The version resolves from the URL segment, a `?version=` query param, or an `X-Version` header (default v1).

| Area | Base route |
|---|---|
| Auth | `/api/v1/auth` — login, register, refresh, logout |
| Projects | `/api/v1/projects` — CRUD, `/me`, `/rich`, `/analytics`, `/{id}/status`, `/{id}/performance`, `/{id}/milestones` |
| Mobile (Flutter) | `/api/v1/projects/mobile`, `/mobile/{id}`, `/mobile/dashboard` |
| Tasks | `/api/v1/tasks` — CRUD, `/advanced`, `/{id}/status`, `/{id}/progress-reports` |
| WBS | `/api/v1/wbs` |
| Phases | `/api/v1/phases` |
| Daily reports | `/api/v1/daily-reports` |
| Weekly reports | `/api/v1/weekly-reports`, `/api/v1/projects/{projectId}/weekly-reports` |
| Work requests | `/api/v1/work-requests` (with approval workflow) |
| Weekly work requests | `/api/v1/weekly-requests`, `/api/v1/projects/{projectId}/weekly-requests` |
| Calendar | `/api/v1/calendar` — events, `/project/{id}`, `/task/{id}`, `/upcoming`, `/conflicts`, `/recurring` |
| Dashboard | `/api/v1/dashboard` — `/overview`, `/statistics`, `/live-activity`, `/project-progress` |
| Images | `/api/v1/images` — upload/metadata; files served from `/files` |
| Notifications | `/api/v1/notifications` — list, mark read, preferences |
| Users | `/api/v1/users` |

### Real-time

SignalR hub at **`/notificationHub`**. Browsers/WebSocket clients cannot set headers on the handshake, so the JWT is passed as a query string — `/notificationHub?access_token=<jwt>` (handled in `JwtBearerEvents.OnMessageReceived`).

### Health

| Endpoint | Purpose |
|---|---|
| `GET /health` | Basic status, version, environment |
| `GET /health/detailed` | Adds DB connectivity + memory stats (503 when unhealthy) |
| `GET /healthz` | Liveness probe |
| `GET /ready` | Readiness probe (includes DB health check) |

## Configuration

Priority: **environment variable → appsettings → hardcoded fallback**. `.env` is loaded at startup via DotNetEnv.

| Variable | Purpose |
|---|---|
| `CONNECTIONSTRINGS__DEFAULT` | PostgreSQL connection string |
| `JWT_KEY` | JWT signing key — **required** outside Development (32+ chars); Development falls back to a dev key with a warning |
| `USE_IN_MEMORY_DB` | `true` → in-memory EF provider (also automatic when environment is `Test`/`Testing`) |
| `RateLimit:Enabled` | Toggle the Redis-backed rate limiter |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Docker` / `Production` |
| `ForceHttpsRedirection` | Force HTTPS redirect in Development |

Profiles: `appsettings.json`, `appsettings.Development.json`, `appsettings.Docker.json`.

## Architecture

```
Controllers/V1/*  ->  Services/<Feature>/  ->  Data/ApplicationDbContext.cs (EF Core)
```

- **Thin controllers.** All business logic lives in feature services (`Projects`, `Tasks`, `Users`, `MasterPlans`, `WBS`, `Infrastructure`, `Shared`).
- **No exceptions for flow control.** Services return `Result<T>` (`Common/Result.cs`); `BaseApiController` converts results to `ApiResponse<T>` and centralizes error handling, current-user lookup, and pagination validation.
- **Mapping** is centralized in `Common/MappingProfile.cs`; **validators** in `Validators/` auto-register from the assembly.
- **CQRS** (`ICommand`/`IQuery` + handlers in `Services/Handlers/`) is used by MasterPlans; other features use plain service classes.
- **Background work** goes through `IBackgroundTaskQueue` + `QueuedHostedService`.

Middleware pipeline order (`Program.cs`):

```
GlobalExceptionMiddleware -> CORS -> RateLimitMiddleware -> static files (/files)
  -> authentication -> JwtBlacklistMiddleware -> authorization
```

### Gotcha: schema comes from the model, not migrations

Startup calls **`EnsureCreatedAsync()`**, not `MigrateAsync()`. Files in `Migrations/` exist but are bypassed at boot so seed data applies without generating new migration files. A fresh database is created from the current EF model.

```bash
dotnet ef migrations add <Name>   # still available if you need one
```

## Tests

```bash
dotnet test dotnet-rest-api.sln
```

- `tests/UnitTests/` — xUnit: `Result<T>`, `BaseApiController` envelope mapping, `AuthService` refresh-token rotation/reuse, `ProjectService`, `TaskService`, validators.
- `tests/Api.IntegrationTests/` — xUnit + `WebApplicationFactory` (`ApiFactory`): boot smoke, authorization, REST semantics, calendar. Runs against the in-memory DB.

Manual / live-server testing:

- `tests/http/*.http` — request files for VS Code REST Client or Rider
- `scripts/*.sh` — e.g. `test-api-endpoints.sh`, `test-auth.sh`, `test-tasks-api.sh`, `test-wbs-api.sh`
- `curl http://localhost:5001/health`

## Repository layout

```
Controllers/       V1 endpoints + BaseApiController + HealthController
Services/          Feature-folder services (Projects, Tasks, WBS, MasterPlans, ...)
Data/              ApplicationDbContext, seed data
Models/            EF entities
DTOs/              Request/response contracts + ApiResponse envelope
Common/            Result<T>, MappingProfile
Validators/        FluentValidation validators
Middleware/        Exception handling, rate limiting, JWT blacklist
Hubs/              SignalR NotificationHub
Migrations/        EF migrations (bypassed at startup)
tests/             UnitTests, Api.IntegrationTests, http/
scripts/           Shell utilities, deploy + live API test scripts
docs/              API design review, phase inventories
azure/             Azure deployment configs
```
