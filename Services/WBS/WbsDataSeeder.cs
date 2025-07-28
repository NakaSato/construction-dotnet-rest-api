using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services.WBS;

/// <summary>
/// Service for seeding sample WBS (Work Breakdown Structure) data for solar PV installation projects
/// </summary>
public class WbsDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WbsDataSeeder> _logger;

    public WbsDataSeeder(ApplicationDbContext context, ILogger<WbsDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds sample WBS data for a solar PV installation project
    /// </summary>
    /// <param name="projectId">The project ID to associate WBS tasks with</param>
    public async System.Threading.Tasks.Task SeedWbsDataAsync(Guid projectId)
    {
        try
        {
            // Check if WBS data already exists for this project
            if (await _context.WbsTasks.AnyAsync(w => w.ProjectId == projectId))
            {
                _logger.LogInformation("WBS data already exists for project {ProjectId}", projectId);
                return;
            }

            // Get a sample user for assignment (if available)
            var sampleUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Role.RoleName == "User");

            var wbsTasks = new List<WbsTask>
            {
                // Phase 1: Project Initiation
                new WbsTask
                {
                    WbsId = "1.0",
                    TaskNameEN = "Project Initiation & Permitting",
                    TaskNameTH = "การเริ่มโครงการและการขออนุญาต",
                    Description = "Initial project setup, feasibility study, and regulatory approvals",
                    Status = WbsTaskStatus.Completed,
                    WeightPercent = 15.0,
                    InstallationArea = "General",
                    AcceptanceCriteria = "All permits obtained and project charter approved",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-90),
                    ActualStartDate = DateTime.UtcNow.AddDays(-90),
                    PlannedEndDate = DateTime.UtcNow.AddDays(-60),
                    ActualEndDate = DateTime.UtcNow.AddDays(-58),
                    CreatedAt = DateTime.UtcNow.AddDays(-90),
                    UpdatedAt = DateTime.UtcNow.AddDays(-58)
                },

                // Phase 4: Installation - Inverter Room
                new WbsTask
                {
                    WbsId = "4.1",
                    TaskNameEN = "Inverter Room Installation",
                    TaskNameTH = "การติดตั้งห้องอินเวอร์เตอร์",
                    Description = "Complete installation of inverter room including foundations, equipment, and electrical connections",
                    Status = WbsTaskStatus.InProgress,
                    WeightPercent = 20.0,
                    InstallationArea = "Inverter Room",
                    AcceptanceCriteria = "Inverter room operational and ready for commissioning",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-30),
                    ActualStartDate = DateTime.UtcNow.AddDays(-28),
                    PlannedEndDate = DateTime.UtcNow.AddDays(10),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },

                new WbsTask
                {
                    WbsId = "4.1.1",
                    ParentWbsId = "4.1",
                    TaskNameEN = "Civil and Structural Works",
                    TaskNameTH = "งานโครงสร้างและงานสิ่งก่อสร้าง",
                    Description = "Foundation work, excavation, rebar tying, formwork, and concrete pouring for inverter pads",
                    Status = WbsTaskStatus.Completed,
                    WeightPercent = 8.0,
                    InstallationArea = "Inverter Room",
                    AcceptanceCriteria = "All foundations meet structural specifications",
                    ProjectId = projectId,
                    AssignedUserId = sampleUser?.UserId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-30),
                    ActualStartDate = DateTime.UtcNow.AddDays(-28),
                    PlannedEndDate = DateTime.UtcNow.AddDays(-20),
                    ActualEndDate = DateTime.UtcNow.AddDays(-18),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-18)
                },

                new WbsTask
                {
                    WbsId = "4.1.2",
                    ParentWbsId = "4.1",
                    TaskNameEN = "Main Equipment Installation",
                    TaskNameTH = "การติดตั้งอุปกรณ์หลัก",
                    Description = "Installing inverters, MDB Solar cabinet, and DC Combiner Boxes",
                    Status = WbsTaskStatus.InProgress,
                    WeightPercent = 7.0,
                    InstallationArea = "Inverter Room",
                    AcceptanceCriteria = "All equipment securely installed with proper clearances",
                    ProjectId = projectId,
                    AssignedUserId = sampleUser?.UserId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-15),
                    ActualStartDate = DateTime.UtcNow.AddDays(-12),
                    PlannedEndDate = DateTime.UtcNow.AddDays(5),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },

                new WbsTask
                {
                    WbsId = "4.1.3",
                    ParentWbsId = "4.1",
                    TaskNameEN = "Electrical System Connection",
                    TaskNameTH = "การเชื่อมต่อระบบไฟฟ้า",
                    Description = "Installing cable trays, running DC and AC cables, and grounding system",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 5.0,
                    InstallationArea = "Inverter Room",
                    AcceptanceCriteria = "All electrical connections completed and tested",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(5),
                    PlannedEndDate = DateTime.UtcNow.AddDays(10),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                // Phase 4: Installation - Carport
                new WbsTask
                {
                    WbsId = "4.2",
                    TaskNameEN = "Carport Installation",
                    TaskNameTH = "การติดตั้งที่จอดรถ",
                    Description = "Complete carport structure and solar panel installation",
                    Status = WbsTaskStatus.InProgress,
                    WeightPercent = 25.0,
                    InstallationArea = "Carport",
                    AcceptanceCriteria = "100 solar panels installed and electrically connected",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-20),
                    ActualStartDate = DateTime.UtcNow.AddDays(-18),
                    PlannedEndDate = DateTime.UtcNow.AddDays(15),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },

                new WbsTask
                {
                    WbsId = "4.2.1",
                    ParentWbsId = "4.2",
                    TaskNameEN = "Foundation and Structural Works",
                    TaskNameTH = "งานฐานรากและโครงสร้าง",
                    Description = "Ground leveling, concrete footings, and carport structure installation",
                    Status = WbsTaskStatus.Completed,
                    WeightPercent = 10.0,
                    InstallationArea = "Carport",
                    AcceptanceCriteria = "Carport structure completed and certified",
                    ProjectId = projectId,
                    AssignedUserId = sampleUser?.UserId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-20),
                    ActualStartDate = DateTime.UtcNow.AddDays(-18),
                    PlannedEndDate = DateTime.UtcNow.AddDays(-10),
                    ActualEndDate = DateTime.UtcNow.AddDays(-8),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                },

                new WbsTask
                {
                    WbsId = "4.2.2",
                    ParentWbsId = "4.2",
                    TaskNameEN = "Solar Panel Installation",
                    TaskNameTH = "การติดตั้งแผงโซลาร์เซลล์",
                    Description = "Installing 100 solar panels on carport roof structure",
                    Status = WbsTaskStatus.InProgress,
                    WeightPercent = 10.0,
                    InstallationArea = "Carport",
                    AcceptanceCriteria = "All 100 panels installed and secured",
                    ProjectId = projectId,
                    AssignedUserId = sampleUser?.UserId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(-5),
                    ActualStartDate = DateTime.UtcNow.AddDays(-3),
                    PlannedEndDate = DateTime.UtcNow.AddDays(10),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                },

                new WbsTask
                {
                    WbsId = "4.2.3",
                    ParentWbsId = "4.2",
                    TaskNameEN = "DC Electrical Works",
                    TaskNameTH = "งานไฟฟ้ากระแสตรง",
                    Description = "Installing cable trays and running DC cables from carport to inverter room",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 5.0,
                    InstallationArea = "Carport",
                    AcceptanceCriteria = "All DC cables connected and tested",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(10),
                    PlannedEndDate = DateTime.UtcNow.AddDays(15),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                // Phase 4: Installation - Water Tank Roof
                new WbsTask
                {
                    WbsId = "4.3",
                    TaskNameEN = "Water Tank Roof Installation",
                    TaskNameTH = "การติดตั้งหลังคาถังน้ำ",
                    Description = "High-precision installation on water tank roof with 300 solar panels",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 30.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "300 solar panels safely installed with structural integrity verified",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(5),
                    PlannedEndDate = DateTime.UtcNow.AddDays(25),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new WbsTask
                {
                    WbsId = "4.3.2",
                    ParentWbsId = "4.3",
                    TaskNameEN = "Anchor Bolt Installation",
                    TaskNameTH = "ยึดพุก ติดแผ่นเพลท",
                    Description = "Critical engineering process for drilling and installing chemical anchors",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 15.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "All anchor points installed and pass pull-out tests",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(5),
                    PlannedEndDate = DateTime.UtcNow.AddDays(8),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new WbsTask
                {
                    WbsId = "4.3.2.1",
                    ParentWbsId = "4.3.2",
                    TaskNameEN = "Drilling & Cleaning",
                    TaskNameTH = "การเจาะและทำความสะอาดรู",
                    Description = "Drilling concrete holes and cleaning per chemical anchor manufacturer specs",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 5.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "Correct hole size, depth, and cleanliness achieved",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(5),
                    PlannedEndDate = DateTime.UtcNow.AddDays(6),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new WbsTask
                {
                    WbsId = "4.3.2.2",
                    ParentWbsId = "4.3.2",
                    TaskNameEN = "Chemical & Bolt Insertion",
                    TaskNameTH = "การฉีดเคมีและติดตั้งพุก",
                    Description = "Injecting chemical adhesive and installing anchor bolts",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 5.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "All anchor points installed with proper curing time observed",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(6),
                    PlannedEndDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new WbsTask
                {
                    WbsId = "4.3.2.3",
                    ParentWbsId = "4.3.2",
                    TaskNameEN = "Pull-out Test & Verification",
                    TaskNameTH = "การทดสอบแรงดึงและตรวจสอบ",
                    Description = "Performing pull-out tests on anchors according to ASTM E488 standard",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 5.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "Pass pull-out test at 1.5x the recommended working load",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(7),
                    PlannedEndDate = DateTime.UtcNow.AddDays(8),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new WbsTask
                {
                    WbsId = "4.3.3",
                    ParentWbsId = "4.3",
                    TaskNameEN = "Panel Mounting",
                    TaskNameTH = "งานติดตั้งแผง",
                    Description = "Lifting and installing 300 solar panels with safety systems",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 15.0,
                    InstallationArea = "Water Tank Roof",
                    AcceptanceCriteria = "All 300 panels installed and torqued to specification",
                    ProjectId = projectId,
                    AssignedUserId = sampleUser?.UserId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(8),
                    PlannedEndDate = DateTime.UtcNow.AddDays(20),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },

                // Phase 4: LV/HV System
                new WbsTask
                {
                    WbsId = "4.4.3.3",
                    TaskNameEN = "Joint Commissioning Test",
                    TaskNameTH = "ทดสอบการทำงานร่วมกับการไฟฟ้า",
                    Description = "Commissioning test witnessed by utility personnel",
                    Status = WbsTaskStatus.NotStarted,
                    WeightPercent = 10.0,
                    InstallationArea = "LV/HV System",
                    AcceptanceCriteria = "Zero Export function operates correctly as certified by the utility",
                    ProjectId = projectId,
                    PlannedStartDate = DateTime.UtcNow.AddDays(30),
                    PlannedEndDate = DateTime.UtcNow.AddDays(35),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

            // Add all WBS tasks
            _context.WbsTasks.AddRange(wbsTasks);
            await _context.SaveChangesAsync();

            // Create dependencies
            var dependencies = new List<WbsTaskDependency>
            {
                // 4.1.2 depends on 4.1.1 (Equipment installation depends on foundation)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.1.2",
                    PrerequisiteTaskId = "4.1.1",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.1.3 depends on 4.1.2 (Electrical work depends on equipment installation)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.1.3",
                    PrerequisiteTaskId = "4.1.2",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.2.2 depends on 4.2.1 (Panel installation depends on structure)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.2.2",
                    PrerequisiteTaskId = "4.2.1",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.2.3 depends on 4.2.2 (DC wiring depends on panel installation)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.2.3",
                    PrerequisiteTaskId = "4.2.2",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.3.2.2 depends on 4.3.2.1 (Chemical injection depends on drilling)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.3.2.2",
                    PrerequisiteTaskId = "4.3.2.1",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.3.2.3 depends on 4.3.2.2 (Pull-out test depends on bolt installation)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.3.2.3",
                    PrerequisiteTaskId = "4.3.2.2",
                    DependencyType = DependencyType.FinishToStart
                },

                // 4.3.3 depends on 4.3.2.3 (Panel mounting depends on verified anchors)
                new WbsTaskDependency
                {
                    DependentTaskId = "4.3.3",
                    PrerequisiteTaskId = "4.3.2.3",
                    DependencyType = DependencyType.FinishToStart
                }
            };

            _context.WbsTaskDependencies.AddRange(dependencies);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded WBS data for project {ProjectId} with {TaskCount} tasks and {DependencyCount} dependencies",
                projectId, wbsTasks.Count, dependencies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding WBS data for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Clears all WBS data for a specific project
    /// </summary>
    /// <param name="projectId">The project ID to clear WBS data for</param>
    public async System.Threading.Tasks.Task ClearWbsDataAsync(Guid projectId)
    {
        try
        {
            var tasks = await _context.WbsTasks
                .Where(w => w.ProjectId == projectId)
                .ToListAsync();

            if (tasks.Any())
            {
                // Remove all dependencies first
                var dependencies = await _context.WbsTaskDependencies
                    .Where(d => tasks.Any(t => t.WbsId == d.DependentTaskId || t.WbsId == d.PrerequisiteTaskId))
                    .ToListAsync();

                _context.WbsTaskDependencies.RemoveRange(dependencies);

                // Remove all evidence
                var evidence = await _context.WbsTaskEvidence
                    .Where(e => tasks.Any(t => t.WbsId == e.WbsTaskId))
                    .ToListAsync();

                _context.WbsTaskEvidence.RemoveRange(evidence);

                // Remove all tasks
                _context.WbsTasks.RemoveRange(tasks);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully cleared WBS data for project {ProjectId}", projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing WBS data for project {ProjectId}", projectId);
            throw;
        }
    }
}
