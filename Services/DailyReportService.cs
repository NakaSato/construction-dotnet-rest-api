using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public class DailyReportService : IDailyReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public DailyReportService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ApiResponse<DailyReportDto>> GetDailyReportByIdAsync(Guid reportId)
    {
        try
        {
            var report = await _context.DailyReports
                .Include(r => r.Project)
                .Include(r => r.Reporter)
                .Include(r => r.WorkProgressItems)
                    .ThenInclude(wpi => wpi.Task)
                .Include(r => r.PersonnelLogs)
                    .ThenInclude(pl => pl.User)
                .Include(r => r.MaterialUsages)
                .Include(r => r.EquipmentLogs)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.DailyReportId == reportId);

            if (report == null)
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            var reportDto = MapToDto(report);

            return new ApiResponse<DailyReportDto>
            {
                Success = true,
                Data = reportDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<DailyReportDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        try
        {
            var query = _context.DailyReports
                .Include(r => r.Project)
                .Include(r => r.Reporter)
                .Include(r => r.WorkProgressItems)
                .Include(r => r.PersonnelLogs)
                .Include(r => r.MaterialUsages)
                .Include(r => r.EquipmentLogs)
                .Include(r => r.Images)
                .AsQueryable();

            // Apply filters
            if (parameters.ProjectId.HasValue)
                query = query.Where(r => r.ProjectId == parameters.ProjectId.Value);

            if (parameters.ReporterId.HasValue)
                query = query.Where(r => r.ReporterId == parameters.ReporterId.Value);

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                if (Enum.TryParse<DailyReportStatus>(parameters.Status, true, out var status))
                {
                    query = query.Where(r => r.Status == status);
                }
            }

            if (!string.IsNullOrEmpty(parameters.WeatherCondition))
            {
                if (Enum.TryParse<WeatherCondition>(parameters.WeatherCondition, true, out var weatherCondition))
                {
                    query = query.Where(r => r.WeatherCondition == weatherCondition);
                }
            }

            if (parameters.ReportDateAfter.HasValue)
                query = query.Where(r => r.ReportDate >= parameters.ReportDateAfter.Value);

            if (parameters.ReportDateBefore.HasValue)
                query = query.Where(r => r.ReportDate <= parameters.ReportDateBefore.Value);

            if (parameters.CreatedAfter.HasValue)
                query = query.Where(r => r.CreatedAt >= parameters.CreatedAfter.Value);

            if (parameters.CreatedBefore.HasValue)
                query = query.Where(r => r.CreatedAt <= parameters.CreatedBefore.Value);

            if (parameters.HasWorkProgress.HasValue)
                query = query.Where(r => r.WorkProgressItems.Any() == parameters.HasWorkProgress.Value);

            if (parameters.HasIssues.HasValue)
                query = query.Where(r => (!string.IsNullOrEmpty(r.Issues)) == parameters.HasIssues.Value);

            var result = await _queryService.ExecuteQueryAsync(query.Select(r => MapToDto(r)), parameters);

            return new ApiResponse<EnhancedPagedResult<DailyReportDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EnhancedPagedResult<DailyReportDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving daily reports",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<DailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.DailyReports
                .Include(r => r.Project)
                .Include(r => r.Reporter)
                .Include(r => r.WorkProgressItems)
                .Include(r => r.PersonnelLogs)
                .Include(r => r.MaterialUsages)
                .Include(r => r.EquipmentLogs)
                .Include(r => r.Images)
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.ReportDate);

            var totalCount = await query.CountAsync();
            var reports = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = reports.Select(MapToDto).ToList();

            return new ApiResponse<PagedResult<DailyReportDto>>
            {
                Success = true,
                Data = new PagedResult<DailyReportDto>
                {
                    Items = reportDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<DailyReportDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving project daily reports",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid reporterId)
    {
        try
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            // Check if a report already exists for this date and project
            var existingReport = await _context.DailyReports
                .AnyAsync(r => r.ProjectId == request.ProjectId && r.ReportDate.Date == request.ReportDate.Date);

            if (existingReport)
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "A daily report already exists for this project and date"
                };
            }

            // Parse weather condition
            if (!Enum.TryParse<WeatherCondition>(request.WeatherCondition, true, out var weatherCondition))
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Invalid weather condition"
                };
            }

            var report = new DailyReport
            {
                DailyReportId = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                ReporterId = reporterId,
                ReportDate = request.ReportDate,
                WeatherCondition = weatherCondition,
                TemperatureHigh = request.TemperatureHigh,
                TemperatureLow = request.TemperatureLow,
                Summary = request.Summary,
                WorkAccomplished = request.WorkAccomplished,
                WorkPlanned = request.WorkPlanned,
                Issues = request.Issues,
                Status = DailyReportStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DailyReports.Add(report);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdReport = await _context.DailyReports
                .Include(r => r.Project)
                .Include(r => r.Reporter)
                .Include(r => r.WorkProgressItems)
                .Include(r => r.PersonnelLogs)
                .Include(r => r.MaterialUsages)
                .Include(r => r.EquipmentLogs)
                .Include(r => r.Images)
                .FirstAsync(r => r.DailyReportId == report.DailyReportId);

            return new ApiResponse<DailyReportDto>
            {
                Success = true,
                Data = MapToDto(createdReport),
                Message = "Daily report created successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<DailyReportDto>
            {
                Success = false,
                Message = "An error occurred while creating the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<DailyReportDto>> UpdateDailyReportAsync(Guid reportId, UpdateDailyReportRequest request)
    {
        try
        {
            var report = await _context.DailyReports
                .Include(r => r.Project)
                .Include(r => r.Reporter)
                .Include(r => r.WorkProgressItems)
                .Include(r => r.PersonnelLogs)
                .Include(r => r.MaterialUsages)
                .Include(r => r.EquipmentLogs)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.DailyReportId == reportId);

            if (report == null)
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            // Only allow updates if report is in Draft or Revision status
            if (report.Status != DailyReportStatus.Draft && report.Status != DailyReportStatus.RevisionRequested)
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Only draft or revision-requested reports can be updated"
                };
            }

            // Parse weather condition
            if (!Enum.TryParse<WeatherCondition>(request.WeatherCondition, true, out var weatherCondition))
            {
                return new ApiResponse<DailyReportDto>
                {
                    Success = false,
                    Message = "Invalid weather condition"
                };
            }

            // Update fields
            report.WeatherCondition = weatherCondition;
            report.TemperatureHigh = request.TemperatureHigh;
            report.TemperatureLow = request.TemperatureLow;
            report.Summary = request.Summary;
            report.WorkAccomplished = request.WorkAccomplished;
            report.WorkPlanned = request.WorkPlanned;
            report.Issues = request.Issues;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<DailyReportDto>
            {
                Success = true,
                Data = MapToDto(report),
                Message = "Daily report updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<DailyReportDto>
            {
                Success = false,
                Message = "An error occurred while updating the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteDailyReportAsync(Guid reportId)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            // Only allow deletion of draft reports
            if (report.Status != DailyReportStatus.Draft)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only draft reports can be deleted"
                };
            }

            _context.DailyReports.Remove(report);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Daily report deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> SubmitDailyReportAsync(Guid reportId)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            if (report.Status != DailyReportStatus.Draft && report.Status != DailyReportStatus.RevisionRequested)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only draft or revision-requested reports can be submitted"
                };
            }

            report.Status = DailyReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Daily report submitted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while submitting the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> ApproveDailyReportAsync(Guid reportId)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            if (report.Status != DailyReportStatus.Submitted)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only submitted reports can be approved"
                };
            }

            report.Status = DailyReportStatus.Approved;
            report.ApprovedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Daily report approved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while approving the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> RejectDailyReportAsync(Guid reportId, string? rejectionReason)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            if (report.Status != DailyReportStatus.Submitted)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only submitted reports can be rejected"
                };
            }

            report.Status = DailyReportStatus.RevisionRequested;
            report.RejectionReason = rejectionReason;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Daily report rejected successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while rejecting the daily report",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #region Work Progress Items

    public async Task<ApiResponse<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<WorkProgressItemDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            // Validate task exists
            if (request.TaskId.HasValue)
            {
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.TaskId == request.TaskId.Value);
                if (!taskExists)
                {
                    return new ApiResponse<WorkProgressItemDto>
                    {
                        Success = false,
                        Message = "Task not found"
                    };
                }
            }

            var workProgressItem = new WorkProgressItem
            {
                WorkProgressItemId = Guid.NewGuid(),
                DailyReportId = reportId,
                TaskId = request.TaskId,
                Description = request.Description,
                PercentageComplete = request.PercentageComplete,
                HoursWorked = request.HoursWorked,
                WorkersAssigned = request.WorkersAssigned,
                Notes = request.Notes
            };

            _context.WorkProgressItems.Add(workProgressItem);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdItem = await _context.WorkProgressItems
                .Include(wpi => wpi.Task)
                .FirstAsync(wpi => wpi.WorkProgressItemId == workProgressItem.WorkProgressItemId);

            return new ApiResponse<WorkProgressItemDto>
            {
                Success = true,
                Data = MapWorkProgressItemToDto(createdItem),
                Message = "Work progress item added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkProgressItemDto>
            {
                Success = false,
                Message = "An error occurred while adding the work progress item",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, CreateWorkProgressItemRequest request)
    {
        try
        {
            var item = await _context.WorkProgressItems
                .Include(wpi => wpi.Task)
                .FirstOrDefaultAsync(wpi => wpi.WorkProgressItemId == itemId);

            if (item == null)
            {
                return new ApiResponse<WorkProgressItemDto>
                {
                    Success = false,
                    Message = "Work progress item not found"
                };
            }

            // Validate task exists if provided
            if (request.TaskId.HasValue)
            {
                var taskExists = await _context.ProjectTasks.AnyAsync(t => t.TaskId == request.TaskId.Value);
                if (!taskExists)
                {
                    return new ApiResponse<WorkProgressItemDto>
                    {
                        Success = false,
                        Message = "Task not found"
                    };
                }
            }

            item.TaskId = request.TaskId;
            item.Description = request.Description;
            item.PercentageComplete = request.PercentageComplete;
            item.HoursWorked = request.HoursWorked;
            item.WorkersAssigned = request.WorkersAssigned;
            item.Notes = request.Notes;

            await _context.SaveChangesAsync();

            return new ApiResponse<WorkProgressItemDto>
            {
                Success = true,
                Data = MapWorkProgressItemToDto(item),
                Message = "Work progress item updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkProgressItemDto>
            {
                Success = false,
                Message = "An error occurred while updating the work progress item",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteWorkProgressItemAsync(Guid itemId)
    {
        try
        {
            var item = await _context.WorkProgressItems.FindAsync(itemId);
            if (item == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work progress item not found"
                };
            }

            _context.WorkProgressItems.Remove(item);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work progress item deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the work progress item",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Personnel Logs

    public async Task<ApiResponse<PersonnelLogDto>> AddPersonnelLogAsync(Guid reportId, PersonnelLogDto request)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<PersonnelLogDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == request.UserId);
            if (!userExists)
            {
                return new ApiResponse<PersonnelLogDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var personnelLog = new PersonnelLog
            {
                PersonnelLogId = Guid.NewGuid(),
                DailyReportId = reportId,
                UserId = request.UserId,
                HoursWorked = request.HoursWorked,
                Position = request.Position,
                Notes = request.Notes
            };

            _context.PersonnelLogs.Add(personnelLog);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdLog = await _context.PersonnelLogs
                .Include(pl => pl.User)
                .FirstAsync(pl => pl.PersonnelLogId == personnelLog.PersonnelLogId);

            return new ApiResponse<PersonnelLogDto>
            {
                Success = true,
                Data = MapPersonnelLogToDto(createdLog),
                Message = "Personnel log added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PersonnelLogDto>
            {
                Success = false,
                Message = "An error occurred while adding the personnel log",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeletePersonnelLogAsync(Guid logId)
    {
        try
        {
            var log = await _context.PersonnelLogs.FindAsync(logId);
            if (log == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Personnel log not found"
                };
            }

            _context.PersonnelLogs.Remove(log);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Personnel log deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the personnel log",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Material Usage

    public async Task<ApiResponse<MaterialUsageDto>> AddMaterialUsageAsync(Guid reportId, MaterialUsageDto request)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<MaterialUsageDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            var materialUsage = new MaterialUsage
            {
                MaterialUsageId = Guid.NewGuid(),
                DailyReportId = reportId,
                MaterialName = request.MaterialName,
                QuantityUsed = request.QuantityUsed,
                Unit = request.Unit,
                Cost = request.Cost,
                Supplier = request.Supplier,
                Notes = request.Notes
            };

            _context.MaterialUsages.Add(materialUsage);
            await _context.SaveChangesAsync();

            return new ApiResponse<MaterialUsageDto>
            {
                Success = true,
                Data = MapMaterialUsageToDto(materialUsage),
                Message = "Material usage added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MaterialUsageDto>
            {
                Success = false,
                Message = "An error occurred while adding the material usage",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteMaterialUsageAsync(Guid usageId)
    {
        try
        {
            var usage = await _context.MaterialUsages.FindAsync(usageId);
            if (usage == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Material usage not found"
                };
            }

            _context.MaterialUsages.Remove(usage);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Material usage deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the material usage",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Equipment Logs

    public async Task<ApiResponse<EquipmentLogDto>> AddEquipmentLogAsync(Guid reportId, EquipmentLogDto request)
    {
        try
        {
            var report = await _context.DailyReports.FindAsync(reportId);
            if (report == null)
            {
                return new ApiResponse<EquipmentLogDto>
                {
                    Success = false,
                    Message = "Daily report not found"
                };
            }

            var equipmentLog = new EquipmentLog
            {
                EquipmentLogId = Guid.NewGuid(),
                DailyReportId = reportId,
                EquipmentName = request.EquipmentName,
                HoursUsed = request.HoursUsed,
                OperatorName = request.OperatorName,
                MaintenanceRequired = request.MaintenanceRequired,
                MaintenanceNotes = request.MaintenanceNotes,
                Notes = request.Notes
            };

            _context.EquipmentLogs.Add(equipmentLog);
            await _context.SaveChangesAsync();

            return new ApiResponse<EquipmentLogDto>
            {
                Success = true,
                Data = MapEquipmentLogToDto(equipmentLog),
                Message = "Equipment log added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EquipmentLogDto>
            {
                Success = false,
                Message = "An error occurred while adding the equipment log",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteEquipmentLogAsync(Guid logId)
    {
        try
        {
            var log = await _context.EquipmentLogs.FindAsync(logId);
            if (log == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Equipment log not found"
                };
            }

            _context.EquipmentLogs.Remove(log);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Equipment log deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the equipment log",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Mapping Methods

    private static DailyReportDto MapToDto(DailyReport report)
    {
        return new DailyReportDto
        {
            DailyReportId = report.DailyReportId,
            ProjectId = report.ProjectId,
            ProjectName = report.Project?.ProjectName ?? string.Empty,
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter?.FullName ?? string.Empty,
            ReportDate = report.ReportDate,
            WeatherCondition = report.WeatherCondition.ToString(),
            TemperatureHigh = report.TemperatureHigh,
            TemperatureLow = report.TemperatureLow,
            Summary = report.Summary,
            WorkAccomplished = report.WorkAccomplished,
            WorkPlanned = report.WorkPlanned,
            Issues = report.Issues,
            Status = report.Status.ToString(),
            SubmittedAt = report.SubmittedAt,
            ApprovedAt = report.ApprovedAt,
            RejectionReason = report.RejectionReason,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            WorkProgressItems = report.WorkProgressItems?.Select(MapWorkProgressItemToDto).ToList() ?? new List<WorkProgressItemDto>(),
            PersonnelLogs = report.PersonnelLogs?.Select(MapPersonnelLogToDto).ToList() ?? new List<PersonnelLogDto>(),
            MaterialUsage = report.MaterialUsages?.Select(MapMaterialUsageToDto).ToList() ?? new List<MaterialUsageDto>(),
            EquipmentLogs = report.EquipmentLogs?.Select(MapEquipmentLogToDto).ToList() ?? new List<EquipmentLogDto>(),
            ImageCount = report.Images?.Count ?? 0
        };
    }

    private static WorkProgressItemDto MapWorkProgressItemToDto(WorkProgressItem item)
    {
        return new WorkProgressItemDto
        {
            WorkProgressItemId = item.WorkProgressItemId,
            TaskId = item.TaskId,
            TaskTitle = item.Task?.Title ?? string.Empty,
            Description = item.Description,
            PercentageComplete = item.PercentageComplete,
            HoursWorked = item.HoursWorked,
            WorkersAssigned = item.WorkersAssigned,
            Notes = item.Notes
        };
    }

    private static PersonnelLogDto MapPersonnelLogToDto(PersonnelLog log)
    {
        return new PersonnelLogDto
        {
            PersonnelLogId = log.PersonnelLogId,
            UserId = log.UserId,
            UserName = log.User?.FullName ?? string.Empty,
            HoursWorked = log.HoursWorked,
            Position = log.Position,
            Notes = log.Notes
        };
    }

    private static MaterialUsageDto MapMaterialUsageToDto(MaterialUsage usage)
    {
        return new MaterialUsageDto
        {
            MaterialUsageId = usage.MaterialUsageId,
            MaterialName = usage.MaterialName,
            QuantityUsed = usage.QuantityUsed,
            Unit = usage.Unit,
            Cost = usage.Cost,
            Supplier = usage.Supplier,
            Notes = usage.Notes
        };
    }

    private static EquipmentLogDto MapEquipmentLogToDto(EquipmentLog log)
    {
        return new EquipmentLogDto
        {
            EquipmentLogId = log.EquipmentLogId,
            EquipmentName = log.EquipmentName,
            HoursUsed = log.HoursUsed,
            OperatorName = log.OperatorName ?? string.Empty,
            MaintenanceRequired = log.MaintenanceRequired,
            MaintenanceNotes = log.MaintenanceNotes,
            Notes = log.Notes
        };
    }

    #endregion
}
