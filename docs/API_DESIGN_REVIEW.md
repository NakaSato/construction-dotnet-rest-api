# API Design Review — Solar Projects API

Date: 2026-07-09 · Scope: all HTTP controllers (200 endpoints across 20 controllers) · Target: `net10.0`

---

## 1. API Design Inventory

### 1.1 Resource map

| Controller | Base route | Endpoints | Versioning | Class-level auth |
|---|---|---|---|---|
| Auth | `api/v{version}/[controller]` → `/Auth` | 4 | `[ApiVersion("1.0")]` | anonymous (logout `[Authorize]`) |
| Projects | `api/v{version}/[controller]` → `/Projects` | 19 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Tasks | `api/v{version}/tasks` | 10 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Users | `api/v{version}/users` | 11 | `[ApiVersion("1.0")]` | `[Authorize]` |
| DailyReports | `api/v{version}/daily-reports` | 26 | `[ApiVersion("1.0")]` | `[Authorize]` |
| WeeklyReports | `api/v{version}/weekly-reports` + nested `projects/{id}/weekly-reports` | 9 | `[ApiVersion("1.0")]` | `[Authorize]` |
| WeeklyWorkRequests | `api/v{version}/weekly-requests` + nested | 9 | `[ApiVersion("1.0")]` | `[Authorize]` |
| WorkRequests | `api/v{version}/work-requests` | 10 | `[ApiVersion("1.0")]` | `[Authorize]` |
| MasterPlans | `api/v{version}/master-plans` | 31 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Phases | `api/v{version}/phases` | 4 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Wbs | `api/v1/wbs` (hardcoded) | 12 | **none** | `[Authorize]` |
| Calendar | `api/v1/[controller]` (hardcoded) | 14 | **none** | none at class level |
| Documents | `api/v{version}/documents` | 5 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Images | `api/v{version}/images` | 8 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Resources | `api/v{version}/resources` | 5 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Notifications | `api/v{version}/notifications` | 10 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Dashboard | `api/v{version}/dashboard` | 6 | `[ApiVersion("1.0")]` | `[Authorize]` |
| Debug | `api/[controller]` → `/api/Debug` | 5 | none | **none — anonymous** |
| Health | `/[controller]` → `/Health` | 2 | none | anonymous (by design) |
| (Program.cs) | `/healthz`, `/ready`, `/notificationHub` | — | — | hub uses JWT via `?access_token` |

### 1.2 Backing implementation status

Wired in `Program.cs` DI:

| Feature | Service | Status |
|---|---|---|
| Projects, ProjectAnalytics, Tasks, MasterPlans, WBS, Auth, Users, Query/Cache | real classes | **Real** |
| DailyReports | `StubDailyReportService` | **Stub** |
| Notifications | `StubNotificationService` | **Stub** |
| WorkRequests + Approval | `StubWorkRequestService` / `StubWorkRequestApprovalService` | **Stub** |
| WeeklyReports / WeeklyWorkRequests | `StubWeeklyReportService` / `StubWeeklyWorkRequestService` | **Stub** |
| Calendar, Images, Resources, Documents | `StubCalendarService` / `StubImageService` / `StubResourceService` / `StubDocumentService` | **Stub** |

≈ 87 of 200 endpoints (Calendar 14, DailyReports 26, WeeklyReports 9, WeeklyWorkRequests 9, WorkRequests 10, Documents 5, Images 8, Resources 5, part of Notifications) are served by stubs returning canned data while appearing fully documented in Swagger.

---

## 2. Analysis (findings, ranked)

### P0-1 — SECURITY: `DebugController` is anonymous and ungated
`Controllers/DebugController.cs:15` — `[Route("api/[controller]")]`, no `[Authorize]`, no environment check.
Exposed to any unauthenticated caller in every environment:
- `GET /api/debug/config` — leaks configuration/environment details
- `POST /api/debug/migrate-database` — mutates the database schema
- `GET /api/debug/database`, `/database-info`, `/cache-stats`

**Impact:** information disclosure + unauthenticated destructive action in production.

### P0-2 — AUTHZ BROKEN: role names in `[Authorize]` don't exist in the database
Seeded roles (`Data/ApplicationDbContext.cs:190-193`): `Admin`, `Manager`, `User`, `Viewer`. JWT emits `user.Role.RoleName` verbatim (`Services/Users/AuthService.cs:316`).

Role strings used in controllers vs reality:

| Attribute string | Occurrences | Matches a real role? |
|---|---|---|
| `Administrator,ProjectManager` | 25 | **never** |
| `Administrator` | 10 | **never** |
| `Administrator,Manager,ProjectManager` | 8 | only `Manager` |
| `Administrator,ProjectManager,Planner` | 4 | **never** |
| `Administrator,Manager` | 4 | only `Manager` |
| `Administrator,ProjectManager,SiteSupervisor` | 3 | **never** |
| `Administrator,ProjectManager,Technician` | 2 | **never** |
| `Administrator,ProjectManager,Executive` | 1 | **never** |
| `Admin,Manager,Supervisor` | 3 | `Admin`, `Manager` only |
| `Admin,ProjectManager` | 2 | `Admin` only |
| `Admin` / `Admin,Manager` | 18 | ✅ valid |

**Impact:** ~45 endpoints (every write path in DailyReports, MasterPlans, Documents, WeeklyReports, WeeklyWorkRequests, WorkRequests, Resources, Notifications admin actions) return **403 for every user including admin**. The API's approval workflows are unusable.

### P1-1 — Versioning inconsistency
- `WbsController` (`api/v1/wbs`) and `CalendarController` (`api/v1/[controller]`) hardcode `v1` and omit `[ApiVersion]` — they ignore the query-string/header version readers configured in `Program.cs` and group incorrectly in Swagger/ApiExplorer.
- `CalendarController` also has no class-level `[Authorize]` (endpoints rely on whatever method-level attributes exist).

### P1-2 — Debug/test endpoints mixed into the production v1 surface
- `GET /api/v1/projects/test` (`ProjectsController.cs:603`)
- `POST /api/v1/notifications/test`, `/test-signalr`, `GET /connection-info`
- `POST /api/v1/wbs/seed-data/{projectId}`
- `POST /api/v1/dashboard/broadcast-progress/{projectId}`, `/system-announcement` (duplicated in Notifications)

### P1-3 — POSTs return 200 instead of 201
`CreatedAtAction` is used only once (WbsController). All other creates return 200 with the envelope — clients can't rely on standard REST semantics or `Location` headers.

### P2-1 — Three competing sub-resource URL styles
- `GET /images/project/{projectId}` (child-first)
- `GET /daily-reports/projects/{projectId}` (child-first, plural)
- `GET /projects/{projectId}/weekly-reports` (canonical nested)
- plus `wbs/hierarchy/{projectId}`, `wbs/progress/{projectId}`, `calendar/project/{projectId}`

### P2-2 — Inconsistent state-transition verbs
- `PATCH /users/{id}/activate` vs `POST /master-plans/{id}/activate`
- `PATCH /tasks/{id}/status` and `PATCH /wbs/{wbsId}/status` vs `POST /daily-reports/{id}/submit|approve|reject`
Same concept (workflow action), three shapes.

### P2-3 — Route-token vs kebab-case naming
`[controller]` token yields `/api/v1/Auth`, `/api/v1/Projects`, `/api/v1/Calendar` (Pascal-case in Swagger); everything else is explicit kebab-case. Cosmetic but visible in generated clients.

### P2-4 — `Program.cs` duplicate registrations
- `AddSignalR` called twice (lines ~72 and ~222) with **conflicting** keep-alive/timeout options — last registration wins silently.
- `AddCors` called twice (FlutterAppPolicy; later default + SignalRPolicy). `SignalRPolicy` references placeholder domain `https://your-frontend-domain.com` and is never applied to the hub route.
- CORS `AllowAnyOrigin` on the default + Flutter policies — acceptable for dev, should be tightened per environment.

### P2-5 — RPC-style representation variants
`/projects/rich`, `/users/rich`, `/users/advanced`, `/tasks/advanced`, `/images/project/{id}/advanced|rich`, `/daily-reports/enhanced` — representation choice encoded in the path instead of query parameters (`?view=rich` / `?include=`), multiplying endpoint count and Swagger noise.

### P3 — Minor
- `GET /users/username/{username}` — alternate-key lookup as a path segment; a `?username=` filter on the collection is more conventional.
- `DashboardController` uses `Roles = "Admin,Manager"` while its sibling announcement endpoint in Notifications uses `Administrator` — same operation, different (and broken) gate.
- README documents .NET 9 and only a fraction of the surface.

---

## 3. Plan — fix & refactor

Phased so each step ships independently. Phases 1–2 are non-breaking bug/security fixes; 3–4 are surface refactors with deprecation aliases; 5 is cleanup.

### Phase 1 — Security & authz correctness (P0, do first)

1. **Lock down Debug endpoints** — either delete `DebugController` or:
   - add `[Authorize(Roles = Roles.Admin)]`,
   - register it only when `env.IsDevelopment()`,
   - remove `POST /api/debug/migrate-database` outright (migrations belong to deploy, not HTTP).
2. **Create a single role-constants class** `Common/Roles.cs`:
   ```csharp
   public static class Roles
   {
       public const string Admin = "Admin";
       public const string Manager = "Manager";
       public const string User = "User";
       public const string Viewer = "Viewer";
       public const string AdminOrManager = Admin + "," + Manager;
   }
   ```
3. **Decide the canonical role set.** Two options:
   - **A (recommended, minimal):** map every attribute to seeded roles — `Administrator→Admin`, `ProjectManager→Manager`, `SiteSupervisor/Supervisor/Technician/Planner/Executive→Manager or User` (product decision per endpoint).
   - **B:** extend the seed to the richer role set and migrate existing users. More disruptive; only if the domain genuinely needs 7 roles.
4. Replace all 69 string literals with `Roles.*` constants; forbid raw strings via review convention.
5. **Verify:** run `scripts/test-auth.sh`, `scripts/test-tasks-permissions.sh`, `scripts/test-admin-manager-*.sh` against a locally running instance; every previously-403 admin action must succeed.

### Phase 2 — Program.cs & pipeline hygiene (P2-4)

1. Collapse to one `AddSignalR` call with intended timeouts.
2. Collapse to one `AddCors` block; per-environment origin lists from configuration; apply `SignalRPolicy` (with real origins) to `/notificationHub`.
3. Move the ~50-line DI block into `Extensions/` (a `AddApplicationServices(this IServiceCollection)`) — `RefactoredServiceExtensions.cs` already exists as the pattern.

### Phase 3 — Version & route normalization (P1-1, P2-3)

1. `WbsController`: `[Route("api/v{version:apiVersion}/wbs")]` + `[ApiVersion("1.0")]`.
2. `CalendarController`: same treatment + class-level `[Authorize]`.
3. Replace `[controller]` tokens with explicit lowercase literals (`auth`, `projects`, `calendar`) so URL casing is deterministic. ASP.NET routing is case-insensitive → non-breaking.

### Phase 4 — REST surface consistency (P1-2, P1-3, P2-1, P2-2, P2-5)

Breaking changes — introduce alongside old routes, mark old with `[Obsolete]`/Swagger deprecation, remove in v2.

1. **Creates return 201**: adopt `CreatedAtAction` in `BaseApiController.ToApiResponse` overload for create results.
2. **Canonical nested reads**: standardize on `projects/{projectId}/<child>` (pattern already proven by WeeklyReports). Add nested aliases for images, daily-reports, master-plans, calendar, wbs; deprecate `<child>/project/{id}` forms.
3. **Workflow actions as POST**: `POST /{resource}/{id}/activate|deactivate|submit|approve|reject|complete`. Convert the two `PATCH .../activate` user endpoints; keep `PATCH .../status` only where the client sets an arbitrary status value.
4. **Representation variants → query params**: `?view=rich|summary` and `?include=` on the collection endpoints; deprecate `/rich`, `/advanced`, `/enhanced` paths.
5. **Remove test endpoints from v1**: delete `projects/test`, `notifications/test`, `test-signalr`; move `wbs/seed-data` behind Development-only registration; keep `connection-info` only if the mobile client uses it (then rename to `notifications/connection`).
6. Consolidate the duplicated system-announcement/broadcast endpoints (Dashboard vs Notifications) into one resource.

### Phase 5 — Stub reconciliation (P0-adjacent, product-driven)

1. Inventory each stub (`Services/Infrastructure/Stub*`) → decide implement / hide / delete per feature.
2. Until implemented, tag stub-backed endpoints in Swagger (`[ApiExplorerSettings(GroupName = "preview")]` or a `[Preview]` doc filter) so clients know data is fake.
3. Track real implementations feature-by-feature: DailyReports (26 endpoints, biggest surface) first — it has full workflow semantics already designed.

### Suggested order & risk

| Step | Risk | Breaking? | Effort |
|---|---|---|---|
| Phase 1 (authz/security) | fixes prod-broken behavior | No (unlocks 403s) | S |
| Phase 2 (Program.cs) | low | No | S |
| Phase 3 (versioning/routes) | low | No | S |
| Phase 4 (REST shape) | medium | Yes → alias + deprecate | M–L |
| Phase 5 (stubs) | product decisions needed | No | L |

### Verification per phase
- Build: `/usr/local/share/dotnet/dotnet build` (0 warnings today — keep it).
- No unit-test project exists: use `tests/http/*.http` + `scripts/test-*.sh` against `http://localhost:5001`; add a real test project (`xunit` + `WebApplicationFactory`, in-memory DB via `USE_IN_MEMORY_DB=true`) in Phase 1 so authz fixes get regression coverage — this is currently the largest quality gap in the repo.
