# Refactor Plan — Solar Projects API

> Generated 2026-07-12 from a structural audit (knowledge graph + source scan).
> Scope: `net10.0` ASP.NET Core REST API, ~29k LOC of C# (excl. Migrations).

---

## Applied — Outcome (2026-07-12)

All six phases were worked. Net change: **+700 / −5163 lines across 50 files**. Every
step was verified with `dotnet build` + the full test suite. Final state: **build clean,
17 unit + 40 integration tests passing.**

**Phase 0 — Safety net: DONE.** An integration-test project already existed (26 tests);
added a `tests/UnitTests` xUnit project (`Result` behavior + `BaseApiController` mapping,
17 tests), a per-controller smoke suite, Calendar round-trip tests, and `scripts/test-unit.sh`.
The smoke suite immediately caught two real defects (below).

**Phase 1 — MasterPlan decomposition: DONE, via deletion (the plan's alternative).**
Discovery: `MasterPlansController` was **entirely commented out**, and the whole
CQRS/decomposition apparatus (Crud/Analytics/Phase/Milestone/Reporting/Orchestrator
services, both handler files, `ICommandQueryInterfaces`, `RefactoredServiceExtensions`)
was **dead** — referenced only by each other and never registered. The only live consumer
of MasterPlan code is `PhasesController`, which uses the working monolith `MasterPlanService`.
"Finishing the split" would have meant resurrecting a disabled controller whose endpoints
have no backing service methods — net-new feature work, not refactoring. So the dead island
(11 files) was deleted and the working monolith kept. *master-plans still has no HTTP surface
by design; re-exposing it is a product decision, not a refactor.*

**Phase 2 — Result unification: DONE.** `ServiceResult<T>` (209 refs) merged into
`Common/Result<T>` (added `StatusCode` + `SuccessResult`/`ErrorResult`/`NotFoundResult`
compat factories), all call sites migrated, `DTOs/ServiceResult.cs` deleted. One naming
style remains (`.SuccessResult()` alongside `.Success()`) — one *type*, two factory aliases,
kept for a zero-risk migration.

**Phase 3 — ApiResponse leak: DONE.** `CalendarService` (41 refs) converted to return
`Result<T>`; `CalendarController` rewritten to map via `ToApiResponse`/`ToCreatedResponse`
(404 now flows through `NotFoundResult`). `ResponseBuilderService`/`IResponseBuilderService`
deleted (BaseApiController's inline builders already replicated it). Only residual `ApiResponse*`
token in Services is `ApiResponseWithPagination` in `QueryService` — a pagination **data DTO**,
not the response envelope; left in place.

**Phase 4 — God files: PARTIALLY DONE (safe parts).** `DTOs/CommonDTOs.cs` (1108 lines,
53 types) split into 10 feature files (largest 339 lines); two 0-byte DTO files deleted.
**Deferred (risky):** thinning `DailyReportsController` (959), `ProjectsController` (828),
`DashboardController` (546) and decomposing `DailyReportService` (1053) — these move logic
between layers and carry silent-regression risk that the current smoke-level tests would not
catch. Recommend doing them behind expanded per-endpoint tests, one controller per PR.

**Phase 5 — Housekeeping: PARTIALLY DONE.** `NotificationBackgroundService` is now registered
(singleton + hosted) — it was previously unregistered, which is what broke Dashboard.
**Deferred:** seeder-method extraction (`DataSeeder`/`WbsDataSeeder`) — behaviour-preserving
readability only, entities are wired via local object references so extraction threads state
through ~8 methods; low value / non-trivial risk. Deprecated `[Obsolete]` aliases left in place
(need Flutter access-log confirmation before removal). DB-init strategy (`EnsureCreated` vs
`Migrate`) is an operational decision — unchanged.

### Real defects found & fixed by the new tests
1. **`DashboardController` was 400-ing on every request** — it injected the concrete
   `NotificationBackgroundService`, which was never registered in DI. Fixed by registering it.
2. **`master-plans` returned 404** — not a bug per se: the controller is fully commented out.
   The smoke test was corrected to reflect that the feature has no HTTP surface.

---

## Guiding principles

- Keep the existing layering contract: `Controllers/V1/*` → feature service → `ApplicationDbContext`, services return `Result<T>`, controllers convert via `BaseApiController`.
- No behavior changes without a safety net. Phase 0 (tests) comes first for a reason.
- Each phase is independently mergeable; stop after any phase and the codebase is still healthier than before.

---

## Phase 0 — Safety net (prerequisite)

**Problem:** No unit-test project exists. Only `.http` files and shell scripts against a live server. Every later phase is risky without automated verification.

**Actions:**
1. Add `tests/UnitTests/` xUnit project (`dotnet new xunit`), reference the API project.
2. Add `tests/IntegrationTests/` using `WebApplicationFactory` + in-memory DB (already supported via `USE_IN_MEMORY_DB=true` / `Test` environment).
3. Cover the highest-blast-radius seams first:
   - `Common/Result.cs` / `DTOs/ServiceResult.cs` conversion behavior
   - `BaseApiController` response-envelope helpers
   - Auth flow (login, refresh rotation, reuse detection — recently changed, see commits `d5cc56f`, `6d1f5f3`)
   - One happy-path integration test per controller (smoke: 200/201 + envelope shape).
4. Wire `dotnet test` into CI (if CI exists) or at least a `scripts/test-unit.sh`.

**Exit criteria:** `dotnet test` green; smoke tests cover every V1 controller route group.

---

## Phase 1 — Delete or finish the abandoned MasterPlan decomposition

**Problem:** A previous refactor was started and never landed. Right now both worlds exist:

- `Services/MasterPlans/MasterPlanService.cs` — 1,266 lines, 39 async methods, registered as the real `IMasterPlanService` (`Extensions/ApplicationServiceExtensions.cs:47`).
- `MasterPlanCrudService.cs` (291), `MasterPlanAnalyticsService.cs` (285), `MasterPlanOrchestratorService.cs` — written but **never registered**: `Extensions/RefactoredServiceExtensions.cs` has the registrations commented out (lines 23–30).
- `Services/Handlers/MasterPlanCommandHandlers.cs` / `MasterPlanQueryHandlers.cs` — headers say "TODO: Implement Command classes and uncomment these handlers".
- `MasterPlanOrchestratorService.cs` carries 4 `// TODO: Implement …` stubs (lines 159–180).

**Decision required:** finish the split or delete it. Recommendation: **finish it** — the monolith is the largest service in the codebase and the split files already exist.

**Actions:**
1. Diff `MasterPlanService` methods against `MasterPlanCrudService` + `MasterPlanAnalyticsService` coverage; port the missing ~half into the focused services (Crud / Analytics / Reporting / Templates).
2. Complete `MasterPlanOrchestratorService` as the thin `IMasterPlanService` façade; resolve its 4 TODOs (template + validation methods).
3. Flip DI: enable the registrations in `RefactoredServiceExtensions`, remove the line in `ApplicationServiceExtensions`.
4. Decide CQRS fate: either implement the Command/Query classes the handler files expect, or delete `Services/Handlers/MasterPlan*Handlers.cs` and `ICommandQueryInterfaces.cs` usage for MasterPlans. Recommendation: delete — nothing else in the codebase uses CQRS, and a second dispatch pattern for one feature isn't paying rent.
5. Delete `MasterPlanService.cs` once all callers pass tests.

**Exit criteria:** No file in `Services/MasterPlans/` over ~400 lines; `RefactoredServiceExtensions.cs` has no commented-out registrations (or is deleted); Phase 0 tests green.

---

## Phase 2 — Unify the two result types

**Problem:** Two parallel success/failure envelopes:
- `Common/Result.cs` → `Result<T>` + non-generic `Result`
- `DTOs/ServiceResult.cs` → `ServiceResult<T>`

`BaseApiController` has to handle both (`ToApiResponse` overloads), and every new service author must pick one.

**Actions:**
1. Pick `Result<T>` (`Common/`) as canonical — it's richer (non-generic variant) and lives in the right layer.
2. Add any `ServiceResult<T>` members that `Result<T>` lacks (compare shapes first).
3. Mechanical migration: replace `ServiceResult<T>` usages service-by-service; keep a temporary implicit conversion during the transition if it shrinks PR size.
4. Delete `DTOs/ServiceResult.cs` and the duplicate `BaseApiController` overloads.

**Exit criteria:** Single result type; grep for `ServiceResult` returns nothing.

---

## Phase 3 — Fix the layering leak: `ApiResponse` inside Services

**Problem:** The architecture contract says services return `Result<T>` and only controllers build `ApiResponse<T>`. Five service files violate this:

- `Services/Shared/QueryService.cs` / `IQueryService.cs`
- `Services/Shared/ResponseBuilderService.cs` / `IResponseBuilderService.cs`
- `Services/Infrastructure/CalendarService.cs`

`IQueryService`/`IResponseBuilderService` are consumed by 6 controllers (Calendar, Images, Tasks, Projects, Users, MasterPlans), so this needs a deliberate seam change, not a drive-by.

**Actions:**
1. `CalendarService`: change return types to `Result<T>`; move envelope construction to `CalendarController` via existing `BaseApiController` helpers.
2. `QueryService`: return `Result<PagedResult<T>>` (or equivalent) instead of `ApiResponse`-shaped payloads; adjust the 6 consuming controllers.
3. `ResponseBuilderService`: its job (building `ApiResponse`) belongs to `BaseApiController`. Fold any unique logic (pagination metadata, HATEOAS-ish links) into `BaseApiController` or a `Common/` helper, then delete the service + interface + DI registration.

**Exit criteria:** `grep -r ApiResponse Services/` returns nothing.

---

## Phase 4 — Break up god files

**Problem / targets:**

| File | Size | Action |
|---|---|---|
| `DTOs/CommonDTOs.cs` | 1,108 lines | Split by feature into `ProjectDTOs.cs`, `TaskDTOs.cs`, `UserDTOs.cs`, etc. (match existing per-feature DTO file convention) |
| `DTOs/MasterPlanDto.cs`, `DTOs/ProgressSummaryDto.cs` | **0 bytes** | Delete (empty files) |
| `Controllers/V1/DailyReportsController.cs` | 959 lines | Push logic down into `DailyReportService`; controller methods should be ≤ ~15 lines (validate → call service → `ToApiResponse`) |
| `Controllers/V1/ProjectsController.cs` | 828 lines | Same treatment |
| `Controllers/V1/MasterPlansController.cs` | 792 lines | Shrinks naturally with Phase 1; re-check after |
| `Controllers/V1/DashboardController.cs` | 546 lines | Same treatment |
| `Services/Infrastructure/DailyReportService.cs` | 1,053 lines | Split query/reporting concerns; `CreateEnhancedDailyReportAsync` (104 lines) and `GetProjectDailyReportsAsync` (93) decompose into private helpers |
| `Common/MappingProfile.cs` | 490 lines | Optional: split into per-feature `Profile` classes (AutoMapper `AddMaps` already scans the assembly, so no registration change) |

**Exit criteria:** No controller over ~400 lines; no DTO file over ~500 lines; zero empty files.

---

## Phase 5 — Housekeeping

1. **Seeders:** `DataSeeder.SeedSampleDataAsync` (466 lines) and `WbsDataSeeder.SeedWbsDataAsync` (396 lines) → extract per-entity private methods (`SeedUsersAsync`, `SeedProjectsAsync`, …). Behavior-preserving, low risk, do anytime.
2. **Deprecated endpoint aliases:** `UsersController` activate/deactivate legacy routes, `DashboardController` + `NotificationsController` announcement routes are `[Obsolete]`. Confirm the Flutter app no longer calls them (check access logs), then remove on the next minor version.
3. **`NotificationBackgroundService` TODOs** (db storage, email sending — lines 251, 258): either implement or file as tracked issues; don't leave silent no-ops.
4. **DB init strategy:** startup uses `EnsureCreatedAsync()`, not `MigrateAsync()`, so `Migrations/` is dead weight at runtime and schema drift is invisible. Decide: (a) switch to `MigrateAsync()` + make seeding idempotent, or (b) delete stale migrations and document model-first as the strategy. This is an operational decision (affects prod DB) — **do not bundle with any other phase.**

---

## Suggested order & sizing

| Phase | Risk | Est. effort | Depends on |
|---|---|---|---|
| 0 — Tests | Low | M | — |
| 1 — MasterPlan split | High | L | 0 |
| 2 — Result unification | Medium | M | 0 |
| 3 — ApiResponse leak | Medium | M | 0, 2 |
| 4 — God files | Low-Med | L | 0 (1 helps for MasterPlansController) |
| 5 — Housekeeping | Low | S each | independent |

Phases 2→3 sequence matters (result-type churn touches the same seams). 1 and 2 can run in parallel branches if merged carefully.

## Verification per phase

- `dotnet build` + `dotnet test` (Phase 0 suite) green.
- Run `scripts/test-api-endpoints.sh`, `test-auth.sh`, `test-tasks-api.sh`, `test-wbs-api.sh` against a local server.
- `curl http://localhost:5001/health` returns healthy.
- Knowledge graph: `detect_changes` + `get_impact_radius` on touched hubs before merging.
