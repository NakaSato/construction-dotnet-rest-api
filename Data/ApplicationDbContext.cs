using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Data;

/// <summary>
/// Main database context for the Solar Projects API application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Entity sets for all models
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ImageMetadata> ImageMetadata { get; set; }
    
    // Daily Reports entities
    public DbSet<DailyReport> DailyReports { get; set; }
    public DbSet<WorkProgressItem> WorkProgressItems { get; set; }
    public DbSet<PersonnelLog> PersonnelLogs { get; set; }
    public DbSet<MaterialUsage> MaterialUsages { get; set; }
    public DbSet<EquipmentLog> EquipmentLogs { get; set; }
    
    // Work Requests entities
    public DbSet<WorkRequest> WorkRequests { get; set; }
    public DbSet<WorkRequestTask> WorkRequestTasks { get; set; }
    public DbSet<WorkRequestComment> WorkRequestComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(50);
            
            // Create unique index on RoleName
            entity.HasIndex(r => r.RoleName)
                .IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            
            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);
            
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Create unique indexes
            entity.HasIndex(u => u.Username)
                .IsUnique();
            
            entity.HasIndex(u => u.Email)
                .IsUnique();

            // Configure relationship with Role
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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
            entity.HasKey(dr => dr.ReportId);
            
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

            entity.HasOne(dr => dr.CreatedByUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(dr => dr.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.SubmittedByUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(dr => dr.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WorkProgressItem entity
        modelBuilder.Entity<WorkProgressItem>(entity =>
        {
            entity.HasKey(wpi => wpi.WorkProgressId);
            
            entity.Property(wpi => wpi.Activity)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(wpi => wpi.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(wpi => wpi.DailyReport)
                .WithMany(dr => dr.WorkProgressItems)
                .HasForeignKey(wpi => wpi.ReportId)
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
                .HasForeignKey(pl => pl.ReportId)
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
                .HasForeignKey(mu => mu.ReportId)
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
                .HasForeignKey(el => el.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkRequest entity
        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.HasKey(wr => wr.RequestId);
            
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

            entity.HasOne(wr => wr.RequestedByUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(wr => wr.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(wr => wr.AssignedToUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(wr => wr.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure WorkRequestTask entity
        modelBuilder.Entity<WorkRequestTask>(entity =>
        {
            entity.HasKey(wrt => wrt.TaskId);
            
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
                .HasForeignKey(wrt => wrt.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wrt => wrt.AssignedToUser)
                .WithMany() // No back navigation from User
                .HasForeignKey(wrt => wrt.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure WorkRequestComment entity
        modelBuilder.Entity<WorkRequestComment>(entity =>
        {
            entity.HasKey(wrc => wrc.CommentId);
            
            entity.Property(wrc => wrc.Comment)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(wrc => wrc.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(wrc => wrc.WorkRequest)
                .WithMany(wr => wr.Comments)
                .HasForeignKey(wrc => wrc.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wrc => wrc.User)
                .WithMany() // No back navigation from User
                .HasForeignKey(wrc => wrc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
