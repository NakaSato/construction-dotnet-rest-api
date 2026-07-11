# Phase 5 — Stub Inventory & Reconciliation

Date: 2026-07-11 · Ref: `docs/API_DESIGN_REVIEW.md` §Phase 5 · Scope: 10 stub services / ~82 endpoints

Phase 5 step 1: inventory each `Services/Infrastructure/Stub*` → decide **implement / hide / delete**.
Basis for "implementable now" = backing EF model **and** `DbSet` already exist (schema ready, no migration needed).

---

## 1. Schema-readiness matrix

| Stub service | Endpoints | Model | DbSet | Extra deps | Behavior today |
|---|---|---|---|---|---|
| `StubDailyReportService` | 26 | ✅ `DailyReport` (+Attachment, WorkProgressItem, EquipmentLog, PersonnelLog, MaterialUsage) | ✅ all | — | mostly `ErrorResult("not implemented")`; list endpoints return empty page |
| `StubWorkRequestService` | 8 | ✅ `WorkRequest` (+Comment, Task) | ✅ | — | list empty; rest error |
| `StubWorkRequestApprovalService` | (in 10 WR) | ✅ `WorkRequestApproval`, `WorkRequestNotification` | ✅ | — | all error |
| `StubNotificationService` | 10 | ✖ (DTO only) | ✖ | needs `IHubContext<NotificationHub>` | sends = no-op; reads return empty |
| `StubImageService` | 8 | ✅ `ImageMetadata` | ✖ | file storage (`uploads/` wired) | list empty; rest error/"not found" |
| `StubWeeklyReportService` | 9 (shared) | ✅ `WeeklyReport` | ✖ | migration | list empty; rest error |
| `StubWeeklyWorkRequestService` | 9 (shared) | ✅ `WeeklyWorkRequest` | ✖ | migration | list empty; rest error |
| `StubCalendarService` | 14 | ✅ `CalendarEvent` | ✖ | migration + recurrence logic | project/task/user lists return empty (look real!); rest error |
| `StubResourceService` | 5 | ✖ | ✖ | full model + migration | list empty; rest error |
| `StubDocumentService` | 5 | ✖ | ✖ | full model + storage | (in `Services/Shared/IDocumentService.cs`) |

**Danger note:** Calendar/DailyReport/Weekly/Image list endpoints return `Success=true` + empty page — Swagger + clients see a working, empty feature, not a stub. These are the misleading ones.

---

## 2. Disposition (recommended)

Ordered by ROI (schema-ready + biggest surface + workflow already designed first).

| # | Feature | Decision | Why |
|---|---|---|---|
| 1 | **DailyReports** | **IMPLEMENT** | 26 endpoints, full model+DbSet+children present, workflow (submit/approve/reject/bulk) already designed. Highest ROI. |
| 2 | **WorkRequests + Approval** | **IMPLEMENT** | schema fully present incl. approval/comment/notification tables; approval workflow is a core product path. |
| 3 | **Notifications** | **IMPLEMENT (partial)** | wire `IHubContext<NotificationHub>` so real-time push actually fires (currently no-op everywhere). Persistence model optional/later. Blocks value of SignalR already wired in `Program.cs`. |
| 4 | **Images** | **IMPLEMENT (verify mobile use)** | `ImageMetadata` model + `uploads/` static serving exist; needs DbSet + save pipeline. Likely needed (construction site photos on daily reports). Confirm Flutter client uploads. |
| 5 | **WeeklyReports / WeeklyWorkRequests** | **HIDE** now, implement after DailyReports | models exist but need DbSet+migration; lower priority than daily. Tagged `[Preview]`. |
| 6 | **Calendar** | **HIDE** | 14 endpoints, needs DbSet + recurrence engine (largest effort, unclear mobile demand). Tagged `[Preview]`; revisit as product decision. |
| 7 | **Resources** | **DELETED** ✅ | no model/schema. Controller + stub + DTOs removed (commit-ready). |
| 8 | **Documents** | **DELETED** ✅ | no model/storage; overlaps with Images. Controller + stub + DTOs removed. |

### Decisions taken (2026-07-11, user)
- **Resources & Documents → DELETED.** Removed `ResourcesController`, `DocumentsController`, `StubResourceService`, `StubDocumentService`/`IDocumentService`, their DI registrations, and the Document*/Resource* DTOs + `DocumentCategory`/`DocumentStatus`/`ResourceStatus` enums. Kept `DailyReportAttachmentDto`/`WeeklySummaryDto` (used by DailyReports) and `ResourceType` (shared with MasterPlans).
- **Images → prioritized** for implementation (mobile client uploads site photos).
- **Step 2 (Swagger preview tagging) → DONE.** `[Preview]` attribute + `PreviewOperationFilter` mark all 80 remaining stub operations as `deprecated` with a warning note (verified in `swagger/v1/swagger.json`). Tagged controllers: DailyReports, WorkRequests, WeeklyReports, WeeklyWorkRequests, Calendar, Images, Notifications. Remove `[Preview]` per-controller as each is implemented.

---

## 3. Immediate action (Phase 5 step 2 — non-breaking)

Tag every stub-backed endpoint so clients know data is fake, BEFORE any implementation:

- Add `[ApiExplorerSettings(GroupName = "preview")]` (or a `[Preview]` Swagger doc filter) to the stub-backed controllers/actions.
- Priority: Calendar, Weekly*, Resources, Documents (the empty-success ones that look real).

This ships independently, needs no schema change, and removes the "documented but fake" trap called out in the design review (§1.2).

---

## 4. Open questions (need product/user input)

1. Does the Flutter mobile client currently call Images upload? (drives #4 priority)
2. Resources & Documents — real planned features, or dead surface to delete?
3. Notifications — is persistence (notification history) required, or is real-time push enough for v1?
4. Weekly reports — separate feature, or derivable aggregate of DailyReports?

---

## 5. Suggested execution order

1. **Now:** Swagger `preview` tagging (step 2) — non-breaking, closes the misleading-stub gap.
2. **Next PR:** implement DailyReports (biggest ROI, schema ready).
3. WorkRequests + Approval.
4. Notifications hub wiring.
5. Images (pending mobile confirmation).
6. Product decision gate: Weekly / Calendar / Resources / Documents.
