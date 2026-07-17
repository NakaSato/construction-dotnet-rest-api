# Solar Projects API

ASP.NET Core Web API (`net10.0`) for solar construction project management. Serves a Flutter mobile app and web clients over REST + SignalR real-time.

## Tech Stack

- **.NET 10.0** (ASP.NET Core Web API)
- **EF Core** — PostgreSQL (Npgsql); in-memory provider for tests/Docker
- **JWT** auth with role-based access + refresh-token rotation
- **SignalR** for real-time notifications
- **AutoMapper**, **FluentValidation**
- **Redis**-backed rate limiting (optional)

## Prerequisites

- .NET SDK 10.0 (`10.0.301`)
- PostgreSQL (or set `USE_IN_MEMORY_DB=true`)
- Redis (optional — only if rate limiting enabled)

## Quick Start

```bash
# Build
dotnet build

# Run locally (Swagger UI at root)
dotnet run --urls "http://localhost:5001"
# -> http://localhost:5001
```

> `dotnet` not on PATH in some environments — use the full SDK path, e.g. `/usr/local/share/dotnet/dotnet`.

### Default Admin Account

Seeded by `Services/Infrastructure/DataSeeder.cs`:

- **Username**: `admin@example.com`
- **Password**: `Admin123!`

## Authentication

```bash
# Login
curl -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin@example.com","password":"Admin123!"}'

# Register
curl -X POST http://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"user@example.com","password":"SecurePass123!","fullName":"New User","roleId":3}'
```

## API Endpoints

Versioned under `/api/v1/...`. Version resolves from URL segment, `?version=` query, or `X-Version` header (default v1).

### Authentication
- `POST /api/v1/auth/login` — user login
- `POST /api/v1/auth/register` — user registration
- `POST /api/v1/auth/refresh` — rotate refresh token

### Core Resources
- `GET /api/v1/projects` — list projects
- `POST /api/v1/projects` — create project
- `GET /api/v1/tasks` — list tasks
- `POST /api/v1/daily-reports` — submit daily report
- `GET /api/v1/users` — list users

### Notifications
- `GET /api/v1/notifications` — list user notifications
- `PATCH /api/v1/notifications/{id}/read` — mark as read
- `GET /api/v1/notifications/preferences` — get preferences
- `PUT /api/v1/notifications/preferences` — update preferences
- `/notificationHub` — SignalR hub (JWT via `?access_token=` query string)

See [Notifications API Documentation](./docs/api/NOTIFICATIONS_API.md).

### Mobile (Flutter) Support
- `GET /api/v1/projects/mobile` — lightweight project list
- `GET /api/v1/projects/mobile/{id}` — optimized project details
- `GET /api/v1/projects/mobile/dashboard` — dashboard data

See [Flutter API Support Documentation](./docs/FLUTTER_API_SUPPORT.md).

### Health
- `GET /health` — API health status

## Configuration

Config priority: env var -> appsettings -> hardcoded fallback. `.env` loaded at startup (DotNetEnv).

| Variable | Purpose |
|----------|---------|
| `CONNECTIONSTRINGS__DEFAULT` | PostgreSQL connection string |
| `JWT_KEY` | JWT signing key (required in non-Development) |
| `USE_IN_MEMORY_DB` | `true` -> in-memory EF provider |
| `RateLimit:Enabled` | toggle Redis-backed rate limiting |

Profiles: `appsettings.json` / `.Development.json` / `.Docker.json`.

## Architecture

Layering: `Controllers/V1/*` -> feature service (`Services/<Feature>/`) -> `Data/ApplicationDbContext.cs` (EF Core).

- Services return `Result<T>` / `ServiceResult<T>` (no throwing on failure).
- Controllers extend `BaseApiController`, which maps results to the `ApiResponse<T>` envelope.
- AutoMapper mappings in `Common/MappingProfile.cs`; FluentValidation validators auto-register.
- DB init uses `EnsureCreatedAsync()` (migrations in `Migrations/` are bypassed at startup).

## Testing

```bash
# All automated tests (xUnit)
dotnet test dotnet-rest-api.sln

# Health check against a live server
curl http://localhost:5001/health
```

- `tests/UnitTests/` — unit tests (`Result<T>`, envelope mapping, auth refresh-token rotation).
- `tests/Api.IntegrationTests/` — `WebApplicationFactory` integration tests (in-memory DB).
- `tests/http/` — `.http` request files for manual/live testing.
- `scripts/*.sh` — shell test/utility scripts against a live server.

## Deployment

Docker via `Dockerfile` + `docker-compose.yml` (`.dev.yml` for dev). Scripts: `scripts/deploy-docker.sh`, `scripts/docker-deploy-test.sh`. Azure configs under `azure/`.
