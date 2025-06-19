using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Data;

/// <summary>
/// Application database context for PostgreSQL
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for main entities
    public DbSet<Project> Projects { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<DailyReport> DailyReports { get; set; }
    public DbSet<DailyReportAttachment> DailyReportAttachments { get; set; }
    public DbSet<MasterPlan> MasterPlans { get; set; }
    public DbSet<ProjectPhase> ProjectPhases { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<Models.Task> Tasks { get; set; }
    public DbSet<WorkRequest> WorkRequests { get; set; }
    
    // Missing DbSets
    public DbSet<ProgressReport> ProgressReports { get; set; }
    public DbSet<PhaseProgress> PhaseProgresses { get; set; }
    public DbSet<WorkProgressItem> WorkProgressItems { get; set; }
    public DbSet<PersonnelLog> PersonnelLogs { get; set; }
    public DbSet<MaterialUsage> MaterialUsages { get; set; }
    public DbSet<EquipmentLog> EquipmentLogs { get; set; }
    public DbSet<WorkRequestTask> WorkRequestTasks { get; set; }
    public DbSet<WorkRequestComment> WorkRequestComments { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.ProjectId);
            
            entity.Property(p => p.ProjectName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(p => p.Address)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(p => p.ClientInfo)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with User (ProjectManager)
            entity.HasOne(p => p.ProjectManager)
                .WithMany(u => u.ManagedProjects)
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProjectTask entity
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.HasKey(t => t.TaskId);
            
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with Project
            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with User (AssignedTechnician)
            entity.HasOne(t => t.AssignedTechnician)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ImageMetadata entity
        modelBuilder.Entity<ImageMetadata>(entity =>
        {
            entity.HasKey(i => i.ImageId);
            
            entity.Property(i => i.CloudStorageKey)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(i => i.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(i => i.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(i => i.FileSizeInBytes)
                .IsRequired();
            
            entity.Property(i => i.GPSLatitude)
                .HasPrecision(18, 12);
            
            entity.Property(i => i.GPSLongitude)
                .HasPrecision(18, 12);
            
            entity.Property(i => i.DeviceModel)
                .HasMaxLength(255);
            
            entity.Property(i => i.EXIFData)
                .HasColumnType("jsonb");
            
            entity.Property(i => i.UploadTimestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with Project
            entity.HasOne(i => i.Project)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with User (UploadedBy)
            entity.HasOne(i => i.UploadedByUser)
                .WithMany(u => u.UploadedImages)
                .HasForeignKey(i => i.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with ProjectTask
            entity.HasOne(i => i.Task)
                .WithMany(t => t.Images)
                .HasForeignKey(i => i.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure relationship with DailyReport
            entity.HasOne(i => i.DailyReport)
                .WithMany(dr => dr.Images)
                .HasForeignKey(i => i.DailyReportId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure relationship with WorkRequest
            entity.HasOne(i => i.WorkRequest)
                .WithMany(wr => wr.Images)
                .HasForeignKey(i => i.WorkRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Manager" },
            new Role { RoleId = 3, RoleName = "User" },
            new Role { RoleId = 4, RoleName = "Viewer" }
        );

        // Seed default admin user (password: Admin123!)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Email = "admin@solarprojects.com",
                PasswordHash = "$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu", // Admin123!
                FullName = "System Administrator",
                RoleId = 1,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Configure DailyReport entity
        modelBuilder.Entity<DailyReport>(entity =>
        {
            entity.HasKey(dr => dr.DailyReportId);
            
            entity.Property(dr => dr.ReportDate)
                .IsRequired();
            
            entity.Property(dr => dr.Status)
                .IsRequired()
                .HasConversion<string>();
                
            entity.Property(dr => dr.WeatherCondition)
                .HasConversion<string>();
            
            entity.Property(dr => dr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationships
            entity.HasOne(dr => dr.Project)
                .WithMany() // No back navigation from Project
                .HasForeignKey(dr => dr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dr => dr.Reporter)
                .WithMany() // No back navigation from User
                .HasForeignKey(dr => dr.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.SubmittedByUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(dr => dr.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WorkProgressItem entity
        modelBuilder.Entity<WorkProgressItem>(entity =>
        {
            entity.HasKey(wpi => wpi.WorkProgressItemId);
            
            entity.Property(wpi => wpi.Activity)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(wpi => wpi.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(wpi => wpi.DailyReport)
                .WithMany(dr => dr.WorkProgressItems)
                .HasForeignKey(wpi => wpi.DailyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wpi => wpi.Task)
                .WithMany() // No back navigation from ProjectTask
                .HasForeignKey(wpi => wpi.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure PersonnelLog entity
        modelBuilder.Entity<PersonnelLog>(entity =>
        {
            entity.HasKey(pl => pl.PersonnelLogId);
            
            entity.Property(pl => pl.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(pl => pl.DailyReport)
                .WithMany(dr => dr.PersonnelLogs)
                .HasForeignKey(pl => pl.DailyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pl => pl.User)
                .WithMany() // No back navigation from User
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MaterialUsage entity
        modelBuilder.Entity<MaterialUsage>(entity =>
        {
            entity.HasKey(mu => mu.MaterialUsageId);
            
            entity.Property(mu => mu.MaterialName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(mu => mu.Unit)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(mu => mu.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(mu => mu.DailyReport)
                .WithMany(dr => dr.MaterialUsages)
                .HasForeignKey(mu => mu.DailyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EquipmentLog entity
        modelBuilder.Entity<EquipmentLog>(entity =>
        {
            entity.HasKey(el => el.EquipmentLogId);
            
            entity.Property(el => el.EquipmentName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(el => el.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(el => el.DailyReport)
                .WithMany(dr => dr.EquipmentLogs)
                .HasForeignKey(el => el.DailyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkRequest entity
        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.HasKey(wr => wr.WorkRequestId);
            
            entity.Property(wr => wr.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(wr => wr.Description)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(wr => wr.Type)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(wr => wr.Priority)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(wr => wr.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(wr => wr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationships
            entity.HasOne(wr => wr.Project)
                .WithMany() // No back navigation from Project
                .HasForeignKey(wr => wr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wr => wr.RequestedBy)
                .WithMany() // No back navigation from User
                .HasForeignKey(wr => wr.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(wr => wr.AssignedTo)
                .WithMany() // No back navigation from User
                .HasForeignKey(wr => wr.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure WorkRequestTask entity
        modelBuilder.Entity<WorkRequestTask>(entity =>
        {
            entity.HasKey(wrt => wrt.WorkRequestTaskId);
            
            entity.Property(wrt => wrt.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(wrt => wrt.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(wrt => wrt.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(wrt => wrt.WorkRequest)
                .WithMany(wr => wr.Tasks)
                .HasForeignKey(wrt => wrt.WorkRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wrt => wrt.AssignedToUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(wrt => wrt.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure WorkRequestComment entity
        modelBuilder.Entity<WorkRequestComment>(entity =>
        {
            entity.HasKey(wrc => wrc.WorkRequestCommentId);
            
            entity.Property(wrc => wrc.Comment)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(wrc => wrc.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(wrc => wrc.WorkRequest)
                .WithMany(wr => wr.Comments)
                .HasForeignKey(wrc => wrc.WorkRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wrc => wrc.Author)
                .WithMany() // No back navigation from User
                .HasForeignKey(wrc => wrc.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure CalendarEvent entity
        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(ce => ce.EventId);
            
            entity.Property(ce => ce.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(ce => ce.Description)
                .HasMaxLength(2000);
            
            entity.Property(ce => ce.StartDateTime)
                .IsRequired();
            
            entity.Property(ce => ce.EndDateTime)
                .IsRequired();
            
            entity.Property(ce => ce.EventType)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(ce => ce.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(ce => ce.Priority)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(ce => ce.Location)
                .HasMaxLength(500);
            
            entity.Property(ce => ce.RecurrencePattern)
                .HasMaxLength(100);
            
            entity.Property(ce => ce.MeetingUrl)
                .HasMaxLength(500);
            
            entity.Property(ce => ce.Attendees)
                .HasMaxLength(1000);
            
            entity.Property(ce => ce.Notes)
                .HasMaxLength(1000);
            
            entity.Property(ce => ce.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(ce => ce.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationships
            entity.HasOne(ce => ce.Project)
                .WithMany() // No back navigation from Project
                .HasForeignKey(ce => ce.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ce => ce.Task)
                .WithMany() // No back navigation from ProjectTask
                .HasForeignKey(ce => ce.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ce => ce.CreatedBy)
                .WithMany() // No back navigation from User
                .HasForeignKey(ce => ce.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ce => ce.AssignedTo)
                .WithMany() // No back navigation from User
                .HasForeignKey(ce => ce.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for better performance
            entity.HasIndex(ce => ce.StartDateTime);
            entity.HasIndex(ce => ce.EndDateTime);
            entity.HasIndex(ce => ce.EventType);
            entity.HasIndex(ce => ce.Status);
            entity.HasIndex(ce => ce.ProjectId);
            entity.HasIndex(ce => ce.TaskId);
            entity.HasIndex(ce => ce.CreatedByUserId);
            entity.HasIndex(ce => ce.AssignedToUserId);
        });

        // Configure WorkRequestApproval entity
        modelBuilder.Entity<WorkRequestApproval>(entity =>
        {
            entity.HasKey(wra => wra.ApprovalId);
            
            entity.Property(wra => wra.Comments)
                .HasMaxLength(1000);
            
            entity.Property(wra => wra.RejectionReason)
                .HasMaxLength(500);
            
            entity.Property(wra => wra.EscalationReason)
                .HasMaxLength(500);
            
            entity.Property(wra => wra.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationships
            entity.HasOne(wra => wra.WorkRequest)
                .WithMany(wr => wr.ApprovalHistory)
                .HasForeignKey(wra => wra.WorkRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wra => wra.Approver)
                .WithMany()
                .HasForeignKey(wra => wra.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(wra => wra.EscalatedFrom)
                .WithMany(wra => wra.Escalations)
                .HasForeignKey(wra => wra.EscalatedFromId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(wra => wra.WorkRequestId);
            entity.HasIndex(wra => wra.ApproverId);
            entity.HasIndex(wra => wra.Action);
            entity.HasIndex(wra => wra.Level);
            entity.HasIndex(wra => wra.CreatedAt);
            entity.HasIndex(wra => wra.IsActive);
        });

        // Configure WorkRequestNotification entity
        modelBuilder.Entity<WorkRequestNotification>(entity =>
        {
            entity.HasKey(wrn => wrn.NotificationId);
            
            entity.Property(wrn => wrn.Subject)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(wrn => wrn.Message)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(wrn => wrn.EmailTo)
                .HasMaxLength(500);
            
            entity.Property(wrn => wrn.EmailCc)
                .HasMaxLength(500);
            
            entity.Property(wrn => wrn.ErrorMessage)
                .HasMaxLength(1000);
            
            entity.Property(wrn => wrn.Metadata)
                .HasMaxLength(2000);
            
            entity.Property(wrn => wrn.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationships
            entity.HasOne(wrn => wrn.WorkRequest)
                .WithMany(wr => wr.Notifications)
                .HasForeignKey(wrn => wrn.WorkRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wrn => wrn.Recipient)
                .WithMany()
                .HasForeignKey(wrn => wrn.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(wrn => wrn.Sender)
                .WithMany()
                .HasForeignKey(wrn => wrn.SenderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(wrn => wrn.WorkRequestId);
            entity.HasIndex(wrn => wrn.RecipientId);
            entity.HasIndex(wrn => wrn.Type);
            entity.HasIndex(wrn => wrn.Status);
            entity.HasIndex(wrn => wrn.CreatedAt);
            entity.HasIndex(wrn => wrn.SentAt);
            entity.HasIndex(wrn => wrn.ReadAt);
        });

        // Configure WeeklyWorkRequest entity
        modelBuilder.Entity<WeeklyWorkRequest>(entity =>
        {
            entity.HasKey(w => w.WeeklyRequestId);
            
            entity.Property(w => w.OverallGoals)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(w => w.KeyTasks)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(w => w.PersonnelForecast)
                .HasMaxLength(1000);

            entity.Property(w => w.MajorEquipment)
                .HasMaxLength(1000);

            entity.Property(w => w.CriticalMaterials)
                .HasMaxLength(1000);

            entity.Property(w => w.Priority)
                .HasMaxLength(50);

            entity.Property(w => w.Type)
                .HasMaxLength(100);

            // Foreign key relationships
            entity.HasOne(w => w.Project)
                .WithMany()
                .HasForeignKey(w => w.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.RequestedByUser)
                .WithMany()
                .HasForeignKey(w => w.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.ApprovedByUser)
                .WithMany()
                .HasForeignKey(w => w.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(w => w.ProjectId);
            entity.HasIndex(w => w.WeekStartDate);
            entity.HasIndex(w => w.Status);
            entity.HasIndex(w => w.RequestedById);
            entity.HasIndex(w => w.CreatedAt);
        });

        // Configure WeeklyReport entity
        modelBuilder.Entity<WeeklyReport>(entity =>
        {
            entity.HasKey(w => w.WeeklyReportId);
            
            entity.Property(w => w.SummaryOfProgress)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(w => w.MajorAccomplishments)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(w => w.MajorIssues)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            entity.Property(w => w.Lookahead)
                .HasMaxLength(2000);

            // Foreign key relationships
            entity.HasOne(w => w.Project)
                .WithMany()
                .HasForeignKey(w => w.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.SubmittedByUser)
                .WithMany()
                .HasForeignKey(w => w.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(w => w.ApprovedByUser)
                .WithMany()
                .HasForeignKey(w => w.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(w => w.ProjectId);
            entity.HasIndex(w => w.WeekStartDate);
            entity.HasIndex(w => w.Status);
            entity.HasIndex(w => w.SubmittedById);
            entity.HasIndex(w => w.CreatedAt);
        });

        // Configure MasterPlan entity
        modelBuilder.Entity<MasterPlan>(entity =>
        {
            entity.HasKey(mp => mp.MasterPlanId);
            
            entity.Property(mp => mp.PlanName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(mp => mp.Description)
                .HasMaxLength(1000);

            entity.Property(mp => mp.TotalEstimatedBudget)
                .HasColumnType("decimal(18,2)");

            entity.Property(mp => mp.ApprovalNotes)
                .HasMaxLength(2000);

            // Foreign key relationships
            entity.HasOne(mp => mp.Project)
                .WithOne(p => p.MasterPlan)
                .HasForeignKey<MasterPlan>(mp => mp.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mp => mp.CreatedBy)
                .WithMany()
                .HasForeignKey(mp => mp.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mp => mp.ApprovedBy)
                .WithMany()
                .HasForeignKey(mp => mp.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(mp => mp.ProjectId).IsUnique();
            entity.HasIndex(mp => mp.Status);
            entity.HasIndex(mp => mp.CreatedAt);
        });

        // Configure ProjectPhase entity
        modelBuilder.Entity<ProjectPhase>(entity =>
        {
            entity.HasKey(p => p.PhaseId);
            
            entity.Property(p => p.PhaseName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.EstimatedBudget)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.ActualCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.WeightPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(p => p.CompletionPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(p => p.Prerequisites)
                .HasMaxLength(500);

            entity.Property(p => p.Notes)
                .HasMaxLength(2000);

            // Foreign key relationships
            entity.HasOne(p => p.MasterPlan)
                .WithMany(mp => mp.Phases)
                .HasForeignKey(p => p.MasterPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(p => p.MasterPlanId);
            entity.HasIndex(p => p.PhaseOrder);
            entity.HasIndex(p => p.Status);
        });

        // Configure ProjectMilestone entity
        modelBuilder.Entity<ProjectMilestone>(entity =>
        {
            entity.HasKey(m => m.MilestoneId);
            
            entity.Property(m => m.MilestoneName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(m => m.Description)
                .HasMaxLength(1000);

            entity.Property(m => m.WeightPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(m => m.CompletionCriteria)
                .HasMaxLength(2000);

            entity.Property(m => m.CompletionEvidence)
                .HasMaxLength(1000);

            entity.Property(m => m.Notes)
                .HasMaxLength(2000);

            // Foreign key relationships
            entity.HasOne(m => m.MasterPlan)
                .WithMany(mp => mp.Milestones)
                .HasForeignKey(m => m.MasterPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Phase)
                .WithMany()
                .HasForeignKey(m => m.PhaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.VerifiedBy)
                .WithMany()
                .HasForeignKey(m => m.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(m => m.MasterPlanId);
            entity.HasIndex(m => m.PhaseId);
            entity.HasIndex(m => m.Status);
            entity.HasIndex(m => m.PlannedDate);
        });

        // Configure ProgressReport entity
        modelBuilder.Entity<ProgressReport>(entity =>
        {
            entity.HasKey(pr => pr.ProgressReportId);
            
            entity.Property(pr => pr.OverallCompletionPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(pr => pr.SchedulePerformanceIndex)
                .HasColumnType("decimal(5,4)");

            entity.Property(pr => pr.CostPerformanceIndex)
                .HasColumnType("decimal(5,4)");

            entity.Property(pr => pr.ActualCostToDate)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.EstimatedCostAtCompletion)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.BudgetVariance)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.KeyAccomplishments)
                .HasMaxLength(4000);

            entity.Property(pr => pr.CurrentChallenges)
                .HasMaxLength(4000);

            entity.Property(pr => pr.UpcomingActivities)
                .HasMaxLength(4000);

            entity.Property(pr => pr.RiskSummary)
                .HasMaxLength(2000);

            entity.Property(pr => pr.QualityNotes)
                .HasMaxLength(2000);

            entity.Property(pr => pr.WeatherImpact)
                .HasMaxLength(1000);

            entity.Property(pr => pr.ResourceNotes)
                .HasMaxLength(2000);

            entity.Property(pr => pr.ExecutiveSummary)
                .HasMaxLength(3000);

            // Foreign key relationships
            entity.HasOne(pr => pr.MasterPlan)
                .WithMany(mp => mp.ProgressReports)
                .HasForeignKey(pr => pr.MasterPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pr => pr.Project)
                .WithMany()
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pr => pr.CreatedBy)
                .WithMany()
                .HasForeignKey(pr => pr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(pr => pr.MasterPlanId);
            entity.HasIndex(pr => pr.ProjectId);
            entity.HasIndex(pr => pr.ReportDate);
            entity.HasIndex(pr => pr.HealthStatus);
        });

        // Configure PhaseProgress entity
        modelBuilder.Entity<PhaseProgress>(entity =>
        {
            entity.HasKey(pp => pp.PhaseProgressId);
            
            entity.Property(pp => pp.CompletionPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(pp => pp.PlannedCompletionPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(pp => pp.ProgressVariance)
                .HasColumnType("decimal(5,2)");

            entity.Property(pp => pp.Notes)
                .HasMaxLength(2000);

            entity.Property(pp => pp.ActivitiesCompleted)
                .HasMaxLength(2000);

            entity.Property(pp => pp.Issues)
                .HasMaxLength(2000);

            // Foreign key relationships
            entity.HasOne(pp => pp.ProgressReport)
                .WithMany(pr => pr.PhaseProgressDetails)
                .HasForeignKey(pp => pp.ProgressReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pp => pp.Phase)
                .WithMany()
                .HasForeignKey(pp => pp.PhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(pp => pp.ProgressReportId);
            entity.HasIndex(pp => pp.PhaseId);
        });

        // Configure PhaseResource entity
        modelBuilder.Entity<PhaseResource>(entity =>
        {
            entity.HasKey(pr => pr.PhaseResourceId);
            
            entity.Property(pr => pr.ResourceName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(pr => pr.Description)
                .HasMaxLength(500);

            entity.Property(pr => pr.Unit)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(pr => pr.UnitCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.TotalEstimatedCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.ActualCost)
                .HasColumnType("decimal(18,2)");

            entity.Property(pr => pr.Supplier)
                .HasMaxLength(255);

            entity.Property(pr => pr.Notes)
                .HasMaxLength(1000);

            // Foreign key relationships
            entity.HasOne(pr => pr.Phase)
                .WithMany(p => p.Resources)
                .HasForeignKey(pr => pr.PhaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(pr => pr.PhaseId);
            entity.HasIndex(pr => pr.ResourceType);
            entity.HasIndex(pr => pr.AllocationStatus);
        });

        // Update ProjectTask to include Phase relationship
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            // Add foreign key to Phase
            entity.HasOne<ProjectPhase>()
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.PhaseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Add indexes for new fields
            entity.HasIndex(t => t.PhaseId);
            entity.HasIndex(t => t.Priority);
            entity.HasIndex(t => t.CompletionPercentage);
        });

        // DailyReport configuration
        modelBuilder.Entity<DailyReport>(entity =>
        {
            entity.HasKey(e => e.DailyReportId);
            
            entity.Property(e => e.DailyReportId)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.ProjectId, e.ReportDate })
                .HasDatabaseName("IX_DailyReports_ProjectId_ReportDate");

            entity.HasIndex(e => new { e.ReporterId, e.ReportDate })
                .HasDatabaseName("IX_DailyReports_ReporterId_ReportDate");

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reporter)
                .WithMany()
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DailyReportAttachment configuration
        modelBuilder.Entity<DailyReportAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.DailyReport)
                .WithMany()
                .HasForeignKey(e => e.DailyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
