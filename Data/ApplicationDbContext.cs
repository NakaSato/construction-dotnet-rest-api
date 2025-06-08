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
    }
}
