using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for seeding sample data for testing
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds sample data if database is empty
    /// </summary>
    public async Task SeedSampleDataAsync()
    {
        try
        {
            // Check if sample data already exists
            if (await _context.Projects.AnyAsync() || 
                await _context.DailyReports.AnyAsync() || 
                await _context.WorkRequests.AnyAsync())
            {
                _logger.LogInformation("Sample data already exists, skipping seeding.");
                return;
            }

            _logger.LogInformation("Starting sample data seeding...");

            // Get the admin user for references
            var adminUser = await _context.Users.FirstAsync(u => u.Username == "admin");

            // Create additional users for testing
            var users = new List<User>
            {
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "manager1",
                    Email = "manager1@solarprojects.com",
                    PasswordHash = "$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu", // Admin123!
                    FullName = "Sarah Johnson",
                    RoleId = 2, // Manager
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "tech1",
                    Email = "tech1@solarprojects.com",
                    PasswordHash = "$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu", // Admin123!
                    FullName = "Mike Rodriguez",
                    RoleId = 3, // User (Field Technician)
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "tech2",
                    Email = "tech2@solarprojects.com",
                    PasswordHash = "$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu", // Admin123!
                    FullName = "Lisa Chen",
                    RoleId = 3, // User (Field Technician)
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            var manager = users[0];
            var tech1 = users[1];
            var tech2 = users[2];

            // Create sample projects
            var projects = new List<Project>
            {
                new Project
                {
                    ProjectId = Guid.NewGuid(),
                    ProjectName = "Residential Solar Installation - Smith House",
                    Address = "123 Main Street, Sacramento, CA 95814",
                    ClientInfo = "John Smith - Residential 6kW solar panel installation with battery backup system",
                    Status = ProjectStatus.InProgress,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EstimatedEndDate = DateTime.UtcNow.AddDays(15),
                    ProjectManagerId = manager.UserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Project
                {
                    ProjectId = Guid.NewGuid(),
                    ProjectName = "Commercial Solar Array - Office Building",
                    Address = "456 Business Blvd, San Francisco, CA 94102",
                    ClientInfo = "ABC Corporation - 50kW commercial solar installation on office building rooftop",
                    Status = ProjectStatus.Planning,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EstimatedEndDate = DateTime.UtcNow.AddDays(60),
                    ProjectManagerId = adminUser.UserId,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };

            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            var project1 = projects[0];
            var project2 = projects[1];

            // Create sample project tasks
            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    TaskId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    Title = "Site Survey and Measurements",
                    Description = "Conduct detailed site survey and take measurements for panel placement",
                    Status = dotnet_rest_api.Models.TaskStatus.Completed,
                    AssignedTechnicianId = tech1.UserId,
                    DueDate = DateTime.UtcNow.AddDays(-25),
                    CompletionDate = DateTime.UtcNow.AddDays(-23),
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new ProjectTask
                {
                    TaskId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    Title = "Electrical Panel Upgrade",
                    Description = "Upgrade electrical panel to support solar system connection",
                    Status = dotnet_rest_api.Models.TaskStatus.InProgress,
                    AssignedTechnicianId = tech2.UserId,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new ProjectTask
                {
                    TaskId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    Title = "Solar Panel Installation",
                    Description = "Install solar panels on rooftop according to approved layout",
                    Status = dotnet_rest_api.Models.TaskStatus.NotStarted,
                    AssignedTechnicianId = tech1.UserId,
                    DueDate = DateTime.UtcNow.AddDays(10),
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await _context.ProjectTasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();

            // Create sample daily reports
            var dailyReports = new List<DailyReport>
            {
                new DailyReport
                {
                    DailyReportId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    ReportDate = DateTime.UtcNow.AddDays(-1),
                    Status = DailyReportStatus.Submitted,
                    ReporterId = tech1.UserId,
                    SubmittedByUserId = tech1.UserId,
                    SubmittedAt = DateTime.UtcNow.AddDays(-1).AddHours(6),
                    GeneralNotes = "Good progress on electrical work. Weather conditions favorable.",
                    WeatherCondition = WeatherCondition.Sunny,
                    TemperatureHigh = 75,
                    TemperatureLow = 62,
                    Humidity = 45,
                    WindSpeed = 8.5,
                    Summary = "Completed electrical panel preparation work",
                    WorkAccomplished = "Installed new electrical panel and ran conduit for solar connections",
                    WorkPlanned = "Continue with inverter installation tomorrow",
                    PersonnelOnSite = 3,
                    TotalWorkHours = 24,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new DailyReport
                {
                    DailyReportId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    ReportDate = DateTime.UtcNow.AddDays(-2),
                    Status = DailyReportStatus.Approved,
                    ReporterId = tech2.UserId,
                    SubmittedByUserId = tech2.UserId,
                    SubmittedAt = DateTime.UtcNow.AddDays(-2).AddHours(5),
                    ApprovedAt = DateTime.UtcNow.AddDays(-1).AddHours(9),
                    GeneralNotes = "Site preparation completed successfully",
                    WeatherCondition = WeatherCondition.PartlyCloudy,
                    TemperatureHigh = 72,
                    TemperatureLow = 58,
                    Humidity = 52,
                    WindSpeed = 12.0,
                    Summary = "Completed site preparation and material delivery",
                    WorkAccomplished = "Cleared installation area and received material delivery",
                    WorkPlanned = "Begin electrical work",
                    PersonnelOnSite = 4,
                    TotalWorkHours = 32,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.DailyReports.AddRangeAsync(dailyReports);
            await _context.SaveChangesAsync();

            var report1 = dailyReports[0];
            var report2 = dailyReports[1];

            // Create work progress items for daily reports
            var workProgressItems = new List<WorkProgressItem>
            {
                new WorkProgressItem
                {
                    WorkProgressItemId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    TaskId = tasks[1].TaskId,
                    Activity = "Electrical Panel Installation",
                    Description = "Installed new 200A electrical panel for solar integration",
                    HoursWorked = 8.0,
                    WorkersAssigned = 2,
                    PercentageComplete = 75,
                    Notes = "Panel installation complete, connecting circuits tomorrow",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WorkProgressItem
                {
                    WorkProgressItemId = Guid.NewGuid(),
                    DailyReportId = report2.DailyReportId,
                    TaskId = tasks[0].TaskId,
                    Activity = "Site Preparation",
                    Description = "Cleared installation area and prepared mounting points",
                    HoursWorked = 6.0,
                    WorkersAssigned = 3,
                    PercentageComplete = 100,
                    Notes = "Site is ready for equipment installation",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.WorkProgressItems.AddRangeAsync(workProgressItems);

            // Create personnel logs
            var personnelLogs = new List<PersonnelLog>
            {
                new PersonnelLog
                {
                    PersonnelLogId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    UserId = tech1.UserId,
                    HoursWorked = 8.0,
                    Position = "Lead Technician",
                    Notes = "Supervised electrical panel installation",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new PersonnelLog
                {
                    PersonnelLogId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    UserId = tech2.UserId,
                    HoursWorked = 8.0,
                    Position = "Electrician",
                    Notes = "Assisted with panel wiring",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.PersonnelLogs.AddRangeAsync(personnelLogs);

            // Create material usage records
            var materialUsages = new List<MaterialUsage>
            {
                new MaterialUsage
                {
                    MaterialUsageId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    MaterialName = "200A Electrical Panel",
                    QuantityUsed = 1,
                    Unit = "pcs",
                    Cost = 450.00m,
                    Supplier = "Electric Supply Co",
                    Notes = "Main distribution panel for solar system",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new MaterialUsage
                {
                    MaterialUsageId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    MaterialName = "Electrical Conduit",
                    QuantityUsed = 50,
                    Unit = "ft",
                    Cost = 125.00m,
                    Supplier = "Electric Supply Co",
                    Notes = "For running DC and AC wiring",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.MaterialUsages.AddRangeAsync(materialUsages);

            // Create equipment logs
            var equipmentLogs = new List<EquipmentLog>
            {
                new EquipmentLog
                {
                    EquipmentLogId = Guid.NewGuid(),
                    DailyReportId = report1.DailyReportId,
                    EquipmentName = "Power Drill Set",
                    HoursUsed = 4.0,
                    OperatorName = "Mike Rodriguez",
                    Purpose = "Drilling mounting holes",
                    MaintenanceRequired = false,
                    Notes = "Equipment in good condition",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new EquipmentLog
                {
                    EquipmentLogId = Guid.NewGuid(),
                    DailyReportId = report2.DailyReportId,
                    EquipmentName = "Extension Ladder",
                    HoursUsed = 6.0,
                    OperatorName = "Lisa Chen",
                    Purpose = "Access to installation area",
                    MaintenanceRequired = true,
                    MaintenanceNotes = "Needs safety inspection",
                    Notes = "Ladder showing wear on rungs",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.EquipmentLogs.AddRangeAsync(equipmentLogs);

            // Create sample work requests
            var workRequests = new List<WorkRequest>
            {
                new WorkRequest
                {
                    WorkRequestId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    Title = "Additional Electrical Outlet Installation",
                    Description = "Customer requested additional 240V outlet for electric vehicle charging near garage",
                    Type = WorkRequestType.Other,
                    Priority = WorkRequestPriority.Medium,
                    Status = WorkRequestStatus.Open,
                    RequestedById = adminUser.UserId,
                    AssignedToId = tech2.UserId,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    EstimatedHours = 4.0,
                    EstimatedCost = 350.00m,
                    Location = "Garage area",
                    Notes = "Coordinate with homeowner for scheduling",
                    RequestedDate = DateTime.UtcNow.AddDays(-3),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new WorkRequest
                {
                    WorkRequestId = Guid.NewGuid(),
                    ProjectId = project1.ProjectId,
                    Title = "Panel Layout Revision",
                    Description = "Revise solar panel layout due to newly discovered roof obstruction",
                    Type = WorkRequestType.Maintenance,
                    Priority = WorkRequestPriority.High,
                    Status = WorkRequestStatus.InProgress,
                    RequestedById = tech1.UserId,
                    AssignedToId = manager.UserId,
                    DueDate = DateTime.UtcNow.AddDays(2),
                    EstimatedHours = 6.0,
                    EstimatedCost = 0.00m,
                    Location = "Rooftop installation area",
                    Notes = "Need to redesign layout to avoid HVAC unit",
                    RequestedDate = DateTime.UtcNow.AddDays(-1),
                    StartedAt = DateTime.UtcNow.AddHours(-4),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WorkRequest
                {
                    WorkRequestId = Guid.NewGuid(),
                    ProjectId = project2.ProjectId,
                    Title = "Site Security Assessment",
                    Description = "Assess security requirements for commercial installation equipment storage",
                    Type = WorkRequestType.Inspection,
                    Priority = WorkRequestPriority.Low,
                    Status = WorkRequestStatus.Completed,
                    RequestedById = adminUser.UserId,
                    AssignedToId = tech1.UserId,
                    DueDate = DateTime.UtcNow.AddDays(-2),
                    EstimatedHours = 2.0,
                    EstimatedCost = 0.00m,
                    ActualHours = 1.5,
                    ActualCost = 0.00m,
                    Location = "Commercial building perimeter",
                    Notes = "Standard security measures sufficient",
                    Resolution = "No additional security measures required. Existing building security adequate.",
                    RequestedDate = DateTime.UtcNow.AddDays(-5),
                    StartedAt = DateTime.UtcNow.AddDays(-3),
                    CompletedDate = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            await _context.WorkRequests.AddRangeAsync(workRequests);
            await _context.SaveChangesAsync();

            // Create work request tasks
            var workRequestTasks = new List<WorkRequestTask>
            {
                new WorkRequestTask
                {
                    WorkRequestTaskId = Guid.NewGuid(),
                    WorkRequestId = workRequests[0].WorkRequestId,
                    Title = "Site Assessment for Outlet",
                    Description = "Assess electrical requirements and optimal location for 240V outlet",
                    Status = WorkRequestStatus.Open,
                    AssignedToId = tech2.UserId,
                    EstimatedHours = 1.0,
                    DueDate = DateTime.UtcNow.AddDays(5),
                    Notes = "Check existing electrical capacity",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new WorkRequestTask
                {
                    WorkRequestTaskId = Guid.NewGuid(),
                    WorkRequestId = workRequests[1].WorkRequestId,
                    Title = "Redesign Panel Layout",
                    Description = "Create new layout avoiding roof obstruction",
                    Status = WorkRequestStatus.InProgress,
                    AssignedToId = manager.UserId,
                    EstimatedHours = 4.0,
                    ActualHours = 2.5,
                    DueDate = DateTime.UtcNow.AddDays(1),
                    Notes = "Using CAD software for new design",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.WorkRequestTasks.AddRangeAsync(workRequestTasks);

            // Create work request comments
            var workRequestComments = new List<WorkRequestComment>
            {
                new WorkRequestComment
                {
                    WorkRequestCommentId = Guid.NewGuid(),
                    WorkRequestId = workRequests[0].WorkRequestId,
                    AuthorId = adminUser.UserId,
                    Comment = "Customer confirmed they want Tesla charging capability. Please ensure outlet supports 240V/50A.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new WorkRequestComment
                {
                    WorkRequestCommentId = Guid.NewGuid(),
                    WorkRequestId = workRequests[1].WorkRequestId,
                    AuthorId = tech1.UserId,
                    Comment = "Discovered HVAC unit was installed after initial survey. Need to revise panel layout to maintain required clearances.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new WorkRequestComment
                {
                    WorkRequestCommentId = Guid.NewGuid(),
                    WorkRequestId = workRequests[1].WorkRequestId,
                    AuthorId = manager.UserId,
                    Comment = "Working on new layout. Should have revised design ready by end of day.",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            await _context.WorkRequestComments.AddRangeAsync(workRequestComments);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sample data seeding completed successfully!");
            _logger.LogInformation($"Created {users.Count} users, {projects.Count} projects, {tasks.Count} tasks, {dailyReports.Count} daily reports, and {workRequests.Count} work requests");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding sample data");
            throw;
        }
    }
}
