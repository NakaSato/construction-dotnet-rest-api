# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Solar Projects API — ASP.NET Core Web API (`net10.0`, `RootNamespace` `dotnet_rest_api`) for solar construction project management. Serves a Flutter mobile app + web clients over REST + SignalR real-time.

> README says .NET 9; the project has since been upgraded to .NET 10 (see `dotnet-rest-api.csproj`).

## Commands

`dotnet` is not on PATH in this environment — use the full path `/usr/local/share/dotnet/dotnet` (SDK 10.0.301).

```bash
dotnet build                                   # build
dotnet run --urls "http://localhost:5001"      # run locally (Swagger UI at root: http://localhost:5001)
dotnet ef migrations add <Name>                # add EF migration (Migrations/)
```

**No unit-test project exists.** `tests/` contains only `.http` REST-client files. Testing is done through:
- `.http` files in `tests/http/`
- Shell scripts in `scripts/` (e.g. `test-api-endpoints.sh`, `test-auth.sh`, `test-tasks-api.sh`, `test-wbs-api.sh`) — run against a live server.
- `curl http://localhost:5001/health`

Default admin: `admin@example.com` / `Admin123!` (seeded by `Services/Infrastructure/DataSeeder.cs`).

## Architecture

**Layering:** `Controllers/V1/*` → feature service (`Services/<Feature>/`) → `Data/ApplicationDbContext.cs` (EF Core). Controllers stay thin; business logic lives in services.

**Request flow contracts:**
- Services return `Result<T>` / `ServiceResult<T>` (`Common/Result.cs`) — success/failure without throwing.
- Controllers extend `Controllers/BaseApiController.cs`, which converts `Result`/`ServiceResult` into the `ApiResponse<T>` envelope and centralizes error handling, current-user lookup, and pagination validation. Use its helpers (`ToApiResponse`, `CreateSuccessResponse`, `HandleException`) rather than building responses inline.
- AutoMapper mappings all live in `Common/MappingProfile.cs`; registered via `AddAutoMapper(cfg => cfg.AddMaps(assembly))`.
- FluentValidation validators (`Validators/`) auto-register from the assembly.
- A CQRS pattern (`Services/Interfaces/ICommandQueryInterfaces.cs`: `ICommand`/`IQuery` + handlers) is used by MasterPlans (`Services/Handlers/`); most other features use plain service classes.

**Services are feature-folder organized** (`Projects`, `Tasks`, `Users`, `MasterPlans`, `WBS`, `Infrastructure`, `Shared`). **Many are stubs** — check `Program.cs` DI registrations before assuming an implementation is real:
- Real: `ProjectService`, `ProjectAnalyticsService`, `TaskService`, `MasterPlanService`, `WbsService`, `AuthService`, `UserService`.
- Stub (`Services/Infrastructure/Stub*`): DailyReport, Notification, WorkRequest(+Approval), Weekly(Report/WorkRequest), Calendar, Image, Resource, Document.

**Cross-cutting (wired in `Program.cs`, in pipeline order):** `GlobalExceptionMiddleware` → CORS → `RateLimitMiddleware` (Redis-backed, `RateLimitService`; toggled by `RateLimit:Enabled`) → static files (`/files` → `uploads/`) → auth → `JwtBlacklistMiddleware` → authorization. Background work goes through `IBackgroundTaskQueue` + `QueuedHostedService`.

**Real-time:** SignalR `NotificationHub` at `/notificationHub`. JWT is passed via `?access_token=` query string for the hub (handled in `JwtBearerEvents.OnMessageReceived`).

## Key conventions & gotchas

- **DB init uses `EnsureCreatedAsync()`, NOT `MigrateAsync()`** (`Program.cs` startup). Migrations in `Migrations/` exist but are bypassed at startup so seed data applies without new migration files. Schema changes to a fresh DB come from the model, not migrations.
- **Database selection:** PostgreSQL (Npgsql) by default; in-memory when `USE_IN_MEMORY_DB=true` or environment is `Test`/`Testing`. Docker environment listens on `0.0.0.0:8080` and typically uses in-memory.
- **Config priority is env-var → appsettings → hardcoded fallback** for connection string (`CONNECTIONSTRINGS__DEFAULT`), JWT key (`JWT_KEY`), etc. `.env` is loaded at startup via DotNetEnv. JWT_KEY is **required** in non-Development; Development falls back to a dev key.
- **API versioning:** default v1; version resolved from URL segment (`/api/v1/...`), `?version=` query, or `X-Version` header.
- Config profiles: `appsettings.json` / `.Development.json` / `.Docker.json`.

## Deployment

Docker via `Dockerfile`, `docker-compose.yml` (+ `.dev.yml`); scripts `scripts/deploy-docker.sh`, `scripts/docker-deploy-test.sh`. Azure configs under `azure/`.

<!-- code-review-graph MCP tools -->
## MCP Tools: code-review-graph

**IMPORTANT: This project has a knowledge graph. ALWAYS use the
code-review-graph MCP tools BEFORE using Grep/Glob/Read to explore
the codebase.** The graph is faster, cheaper (fewer tokens), and gives
you structural context (callers, dependents, test coverage) that file
scanning cannot.

### When to use graph tools FIRST

- **Exploring code**: `semantic_search_nodes` or `query_graph` instead of Grep
- **Understanding impact**: `get_impact_radius` instead of manually tracing imports
- **Code review**: `detect_changes` + `get_review_context` instead of reading entire files
- **Finding relationships**: `query_graph` with callers_of/callees_of/imports_of/tests_for
- **Architecture questions**: `get_architecture_overview` + `list_communities`

Fall back to Grep/Glob/Read **only** when the graph doesn't cover what you need.

### Key Tools

| Tool | Use when |
| ------ | ---------- |
| `detect_changes` | Reviewing code changes — gives risk-scored analysis |
| `get_review_context` | Need source snippets for review — token-efficient |
| `get_impact_radius` | Understanding blast radius of a change |
| `get_affected_flows` | Finding which execution paths are impacted |
| `query_graph` | Tracing callers, callees, imports, tests, dependencies |
| `semantic_search_nodes` | Finding functions/classes by name or keyword |
| `get_architecture_overview` | Understanding high-level codebase structure |
| `refactor_tool` | Planning renames, finding dead code |

### Workflow

1. The graph auto-updates on file changes (via hooks).
2. Use `detect_changes` for code review.
3. Use `get_affected_flows` to understand impact.
4. Use `query_graph` pattern="tests_for" to check coverage.
