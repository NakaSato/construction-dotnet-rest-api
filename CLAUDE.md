# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Solar Projects API — ASP.NET Core Web API (`net10.0`, `RootNamespace` `dotnet_rest_api`) for solar construction project management. Serves a Flutter mobile app + web clients over REST + SignalR real-time.

Note the assembly/namespace split: `AssemblyName` is `dotnet-rest-api` (hyphens, so the entrypoint DLL is `dotnet-rest-api.dll`) while `RootNamespace` is `dotnet_rest_api` (underscores).

## Commands

`dotnet` is not on PATH in this environment — use the full path `/usr/local/share/dotnet/dotnet` (SDK 10.0.301).

```bash
dotnet build                                   # build
dotnet run --urls "http://localhost:5001"      # run locally (Swagger UI at root: http://localhost:5001)
dotnet ef migrations add <Name>                # add EF migration (Migrations/)
```

**Automated tests exist** under `tests/` (wired into `dotnet-rest-api.sln`) — 121 tests, all passing:

```bash
dotnet test dotnet-rest-api.sln                          # all 121 (81 unit + 40 integration)
dotnet test tests/UnitTests/UnitTests.csproj             # one project
dotnet test --filter "FullyQualifiedName~TaskServiceTests"   # one class
dotnet test --filter "FullyQualifiedName~ResultTests.NotFound_SetsNotFoundErrorType_And_DescriptiveMessage"  # one test
dotnet test dotnet-rest-api.sln --list-tests             # discover names for --filter
```

A `--filter` run from the repo root still probes both test projects, so it prints a harmless
`No test matches the given testcase filter` for whichever project doesn't hold the test. The
run has still passed — read the `Passed!` line, not that notice. Scope with an explicit
`.csproj` to silence it.

- `tests/UnitTests/` — xUnit unit tests (`Result<T>`, `BaseApiController` envelope mapping, `AuthService` refresh-token rotation/reuse, `ProjectService`, `TaskService`, validators).
- `tests/Api.IntegrationTests/` — xUnit + `WebApplicationFactory` (`ApiFactory`) integration tests (boot smoke, authorization, REST semantics, calendar); run against the in-memory DB.

Several integration tests assert on *removed* behaviour (`Removed_Get_Endpoints_Return_404`, `Admin_Passes_Authorization_On_Previously_Broken_Gates`) — they are regression guards for the Phase 1–5 cleanup, so a 404/403 there is the passing case. Don't "fix" them by reinstating the endpoint.

Manual/live-server testing is also available through:
- `.http` files in `tests/http/`
- Shell scripts in `scripts/` (e.g. `test-api-endpoints.sh`, `test-auth.sh`, `test-tasks-api.sh`, `test-wbs-api.sh`) — run against a live server.
- Health: `/health` (basic), `/health/detailed` (DB connectivity + memory, 503 when unhealthy), `/healthz` (liveness), `/ready` (readiness). `HealthController` is unversioned — no `/api/v1` prefix.

Default admin: `admin@example.com` / `Admin123!` (seeded by `Services/Infrastructure/DataSeeder.cs`).

## Architecture

**Layering:** `Controllers/V1/*` → feature service (`Services/<Feature>/`) → `Data/ApplicationDbContext.cs` (EF Core). Controllers stay thin; business logic lives in services.

**Request flow contracts:**
- Services return `Result<T>` / `ServiceResult<T>` (`Common/Result.cs`) — success/failure without throwing.
- Controllers extend `Controllers/BaseApiController.cs`, which converts `Result`/`ServiceResult` into the `ApiResponse<T>` envelope and centralizes error handling, current-user lookup, and pagination validation. Use its helpers (`ToApiResponse`, `CreateSuccessResponse`, `HandleException`) rather than building responses inline.
- AutoMapper mappings all live in `Common/MappingProfile.cs`; registered via `AddAutoMapper(cfg => cfg.AddMaps(assembly))`.
- FluentValidation validators (`Validators/`) auto-register from the assembly.
- A CQRS pattern (`Services/Interfaces/ICommandQueryInterfaces.cs`: `ICommand`/`IQuery` + handlers) is used by MasterPlans (`Services/Handlers/`); most other features use plain service classes.

**Services are feature-folder organized** (`Projects`, `Tasks`, `Users`, `MasterPlans`, `WBS`, `Infrastructure`, `Shared`). Every service registered in DI is now a real EF Core implementation — `ProjectService`, `ProjectAnalyticsService`, `TaskService`, `MasterPlanService`, `WbsService`, `AuthService`, `UserService`, `DailyReportService`, `NotificationService`, `WorkRequestService` (+`WorkRequestApprovalService`), `ImageService`, `WeeklyReportService`, `WeeklyWorkRequestService`, `CalendarService`. Resource/Document were removed in Phase 5.

Two dead stub files survive that cleanup and are **not** registered in `Program.cs`: `Services/Infrastructure/StubDailyReportService.cs` and `Services/Infrastructure/StubServices.cs` (0 bytes). Ignore them; don't wire them up.

**Cross-cutting (wired in `Program.cs`, in pipeline order):** `GlobalExceptionMiddleware` → CORS → `RateLimitMiddleware` (Redis-backed, `RateLimitService`; toggled by `RateLimit:Enabled`) → static files (`/files` → `uploads/`) → auth → `JwtBlacklistMiddleware` → authorization. Background work goes through `IBackgroundTaskQueue` + `QueuedHostedService`.

**Real-time:** SignalR `NotificationHub` at `/notificationHub`. JWT is passed via `?access_token=` query string for the hub (handled in `JwtBearerEvents.OnMessageReceived`).

## Key conventions & gotchas

- **DB init uses `EnsureCreatedAsync()`, NOT `MigrateAsync()`** (`Program.cs` startup). Migrations in `Migrations/` exist but are bypassed at startup so seed data applies without new migration files. Schema changes to a fresh DB come from the model, not migrations.
- **Database selection:** PostgreSQL (Npgsql) by default; in-memory when `USE_IN_MEMORY_DB=true` or environment is `Test`. The **Docker image sets `USE_IN_MEMORY_DB=true`** so it boots on hosts with no attached DB; both compose files override it to `false` and use their PostgreSQL service. If you add a new compose/deploy target with a real database, you must set `USE_IN_MEMORY_DB=false` *and* a connection string, or it will silently run in-memory.
- `appsettings.Docker.json` hardcodes `Host=postgres-dev`, which only resolves inside `docker-compose.dev.yml`. Any other host needs `CONNECTIONSTRINGS__DEFAULT`.
- The image also sets `DOTNET_USE_POLLING_FILE_WATCHER=true`: hosts with a low inotify limit crash in `WebApplication.CreateBuilder` when the config provider watches `appsettings*.json`.
- **Config priority is env-var → appsettings → hardcoded fallback** for connection string (`CONNECTIONSTRINGS__DEFAULT`), JWT key (`JWT_KEY`), etc. `.env` is loaded at startup via DotNetEnv. `JWT_KEY` is **required** in non-Development, but `appsettings.Docker.json` ships a `Jwt:Key`, so the `Docker` environment satisfies that check with a key committed to the repo — set `JWT_KEY` on any real deployment. `Program.cs` writes the resolved key back to `Jwt:Key` so signing and validation cannot diverge.
- **Use `Common/Roles.cs` constants** (`Roles.Admin`, `Roles.AdminOrManager`, …) in `[Authorize(Roles = ...)]` rather than raw strings. Role names must match the four seeded in `ApplicationDbContext` (`1 Admin`, `2 Manager`, `3 User`, `4 Viewer`) and the claim emitted by `AuthService`; a mismatch here was a P0 authorization bug (see `docs/API_DESIGN_REVIEW.md`).
- **API versioning:** default v1; version resolved from URL segment (`/api/v1/...`), `?version=` query, or `X-Version` header.
- Swagger UI is served at the **root path**, and only when the environment is `Development` or `Docker`.
- Config profiles: `appsettings.json` / `.Development.json` / `.Docker.json`.

## Deployment

Docker via `Dockerfile`, `docker-compose.yml` (+ `.dev.yml`); scripts `scripts/deploy-docker.sh`, `scripts/docker-deploy-test.sh`. Azure configs under `azure/`.

**Render** hosts the live service (`srv-d9j96i4tndls73fmq3ug`, <https://construction-dotnet-rest-api.onrender.com>), building from `Dockerfile` with **auto-deploy on every commit to `main`** — a push deploys. Inspect with the `render` CLI: `render deploys list <srv-id> -o json --confirm`, `render logs --resources <srv-id> --type build -o text --confirm`. The CLI has no `env` subcommand; env vars are dashboard-only.

Keep the `Dockerfile` base images in step with `TargetFramework` in `dotnet-rest-api.csproj` — a stale pin fails the Render build at `dotnet restore` with `NETSDK1045`.

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

1. Use `detect_changes` for code review.
2. Use `get_affected_flows` to understand impact.
3. Use `query_graph` pattern="tests_for" to check coverage.

**Known config bug — the graph may be empty or stale.** Both `.mcp.json` (`cwd`) and the
`.claude/settings.json` hooks hardcode `/Users/chanthawat/Developments/solar/construction-dotnet-rest-api`,
a *different* checkout from this one. So the "auto-update on file change" hook updates
that other repo, not this working copy. If graph queries return nothing, build it against
an explicit root first:

```
build_or_update_graph_tool(repo_root="<this repo's absolute path>", full_rebuild=true)
```

Fixing those two paths would make the hooks work as intended.
