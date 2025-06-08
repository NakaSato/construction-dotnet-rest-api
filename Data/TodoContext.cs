using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<ImageMetadata> ImageMetadata { get; set; }
    
    // Legacy support
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
        });

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.ProjectName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ClientInfo).HasMaxLength(1000);

            entity.HasOne(e => e.ProjectManager)
                  .WithMany(u => u.ManagedProjects)
                  .HasForeignKey(e => e.ProjectManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TaskItem entity
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedTechnician)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(e => e.AssignedTechnicianId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ImageMetadata entity
        modelBuilder.Entity<ImageMetadata>(entity =>
        {
            entity.HasKey(e => e.ImageId);
            entity.HasIndex(e => e.CloudStorageKey).IsUnique();
            entity.Property(e => e.CloudStorageKey).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeviceModel).HasMaxLength(255);
            
            // For PostgreSQL, use column type for JSONB
            entity.Property(e => e.EXIFData)
                  .HasColumnType("jsonb");

            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Images)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Task)
                  .WithMany(t => t.Images)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UploadedByUser)
                  .WithMany(u => u.UploadedImages)
                  .HasForeignKey(e => e.UploadedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Administrator" },
            new Role { RoleId = 2, RoleName = "ProjectManager" },
            new Role { RoleId = 3, RoleName = "FieldTechnician" }
        );

        // Seed default admin user
        var adminUserId = Guid.NewGuid();
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = adminUserId,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default password
                FullName = "System Administrator",
                RoleId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}

// Keep the old TodoContext for backwards compatibility
public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }
}