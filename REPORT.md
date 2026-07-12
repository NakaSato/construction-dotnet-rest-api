# Refactor Report — Solar Projects API

Companion to [REFACTOR_PLAN.md](loop/REFACTOR_PLAN.md). Tracks the **current phase** and an
append-only **change history** — one entry per step. Add a new entry at the bottom of the
history for every future change; never rewrite past entries.

---

## Current status

| | |
|---|---|
| **Active phase** | All 6 phases applied (Phases 4 & 5 partial — see deferred) |
| **Branch** | `main` (uncommitted working tree) |
| **Build** | Clean — 0 warnings, 0 errors |
| **Tests** | 17 unit + 40 integration = **57 passing**, 0 failing |
| **Net diff** | +700 / −5163 lines across 50 files |
| **Date** | 2026-07-12 |

### Phase status

| Phase | Title | Status | Note |
|---|---|---|---|
| 0 | Safety net (tests) | ✅ Done | Unit project + smoke + calendar tests added |
| 1 | MasterPlan decomposition | ✅ Done (deletion) | Dead CQRS island removed; monolith kept |
| 2 | Unify result types | ✅ Done | `ServiceResult<T>` → `Result<T>` |
| 3 | ApiResponse layering leak | ✅ Done | Calendar converted; ResponseBuilder removed |
| 4 | Break up god files | 🟡 Partial | DTOs split; controller thinning deferred |
| 5 | Housekeeping | 🟡 Partial | BG service registered; seeder extraction deferred |

### Deferred (risk > mechanical certainty)

- Thin `DailyReportsController` (959), `ProjectsController` (828), `DashboardController` (546).
- Decompose `DailyReportService` (1053) and the monolith `MasterPlanService` (1266).
- Extract `DataSeeder` / `WbsDataSeeder` seed methods (behaviour-preserving readability only).
- Remove `[Obsolete]` endpoint aliases (needs Flutter access-log confirmation first).
- DB-init strategy (`EnsureCreated` vs `Migrate`) — operational decision, unchanged.

---

## Change history (append-only)

### [Phase 0] Safety net — test infrastructure
- **Added** `tests/UnitTests/UnitTests.csproj` (xUnit) + registered in `dotnet-rest-api.sln`.
- **Added** `tests/UnitTests/ResultTests.cs` — pins `Common/Result<T>` success/failure + error-type factories.
- **Added** `tests/UnitTests/BaseApiControllerTests.cs` — pins `ToApiResponse`/`ToCreatedResponse` HTTP mapping (200/400/404/201) + pagination validation.
- **Added** `tests/Api.IntegrationTests/ControllerSmokeTests.cs` — authenticated collection-GET returns success envelope for every live V1 controller + `/health`.
- **Added** `scripts/test-unit.sh` — runs `dotnet test` (unit + integration, in-memory DB).
- **Result:** 17 unit tests green. Smoke suite surfaced 2 real defects (see Phase 1 / bug fixes).

### [Phase 1] MasterPlan decomposition — deleted dead CQRS island
- **Finding:** `MasterPlansController` fully commented out; CQRS + split services never registered (only cross-referenced by each other). Only live consumer of MasterPlan code is `PhasesController` → monolith `MasterPlanService`.
- **Deleted** `Controllers/V1/MasterPlansController.cs` (fully commented).
- **Deleted** `Services/Handlers/MasterPlanCommandHandlers.cs`, `MasterPlanQueryHandlers.cs` (commented).
- **Deleted** `Services/Interfaces/ICommandQueryInterfaces.cs` (CQRS interfaces, unused).
- **Deleted** `Extensions/RefactoredServiceExtensions.cs` (never called).
- **Deleted** `Services/MasterPlans/` split services: `MasterPlanCrudService`, `MasterPlanAnalyticsService`, `PhaseManagementService`, `MilestoneService`, `MasterPlanReportingService`, `MasterPlanOrchestratorService`.
- **Kept** `MasterPlanService.cs` + `IMasterPlanService.cs` (live via `PhasesController`).
- **Result:** ~2000 lines dead code removed; build clean; 38 integration tests green.

### [Fix] DashboardController DI + master-plans smoke correction
- **Bug:** `DashboardController` injected concrete `NotificationBackgroundService`, never registered → 400 on every request.
- **Changed** `Extensions/ApplicationServiceExtensions.cs` — registered `NotificationBackgroundService` as singleton + hosted service (drains its queue and is injectable).
- **Changed** `ControllerSmokeTests.cs` — removed `/api/v1/master-plans` case (controller is retired by design; the feature has no HTTP surface).
- **Result:** `dashboard/overview` smoke test passes.

### [Phase 2] Unify result types — ServiceResult → Result
- **Changed** `Common/Result.cs` — added `StatusCode` property + `SuccessResult`/`ErrorResult`/`NotFoundResult` compatibility factories (merged from ServiceResult).
- **Renamed** `ServiceResult<…>` → `Result<…>` across all Services + Controllers (209 refs); added `using dotnet_rest_api.Common;` to 18 files.
- **Changed** `Controllers/BaseApiController.cs` — `ToApiResponse`/`ToCreatedResponse` now take `Result<T>`; `.Success` → `.IsSuccess`.
- **Fixed** 7 instance `.Success` reads (compiler-pinpointed) → `.IsSuccess`.
- **Deleted** `DTOs/ServiceResult.cs`.
- **Result:** single result type; 55 tests green.

### [Phase 3] ApiResponse layering leak — Calendar + ResponseBuilder
- **Changed** `Services/Infrastructure/CalendarService.cs` — all 14 `ApiResponse<T>` methods → `Result<T>`; "not found" → `NotFoundResult` (→404).
- **Rewrote** `Controllers/V1/CalendarController.cs` — every action maps service `Result` via `ToApiResponse`/`ToCreatedResponse`; removed hand-rolled status logic.
- **Changed** `Controllers/BaseApiController.cs` — dropped the `IResponseBuilderService` dependency; always uses inline envelope builders.
- **Deleted** `Services/Shared/ResponseBuilderService.cs`, `IResponseBuilderService.cs`; removed DI registration.
- **Added** `tests/Api.IntegrationTests/CalendarTests.cs` — create→get→delete round-trip + missing→404 envelope.
- **Result:** no response-envelope leak left in Services (only pagination data DTO remains); 57 tests green.

### [Phase 4] Break up god files — DTO split (safe part)
- **Split** `DTOs/CommonDTOs.cs` (1108 lines, 53 types) into 10 feature files: `AuthDTOs`, `UserDTOs`, `ProjectDTOs`, `TaskDTOs`, `PatchDTOs`, `ImageDTOs`, `PaginationDTOs`, `DailyReportCommonDTOs`, `WorkRequestDTOs`, `NotificationDTOs` (largest 339 lines).
- **Deleted** `DTOs/MasterPlanDto.cs`, `DTOs/ProgressSummaryDto.cs` (0-byte; real types live in `MasterPlanDTOs.cs`).
- **Deferred:** controller thinning + `DailyReportService` decomposition (behavioural, regression risk).
- **Result:** no DTO file > 500 lines; build verifies no type dropped/duplicated; 57 tests green.

### [Phase 5] Housekeeping (partial)
- `NotificationBackgroundService` registration already handled in the DashboardController DI fix above.
- **Deferred:** seeder-method extraction, `[Obsolete]` alias removal, DB-init strategy decision.
- **Result:** no further code change this pass; documented in [REFACTOR_PLAN.md](loop/REFACTOR_PLAN.md) "Applied — Outcome".
