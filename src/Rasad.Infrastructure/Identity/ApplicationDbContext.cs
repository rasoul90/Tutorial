using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rasad.Domain.Entities;

namespace Rasad.Infrastructure.Identity;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ministry> Ministries => Set<Ministry>();
    public DbSet<MinistryDataWindow> MinistryDataWindows => Set<MinistryDataWindow>();
    public DbSet<Directorate> Directorates => Set<Directorate>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<UserScope> UserScopes => Set<UserScope>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<ApiNonce> ApiNonces => Set<ApiNonce>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ministry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50);
            entity.HasMany(x => x.Directorates).WithOne(x => x.Ministry).HasForeignKey(x => x.MinistryId);
            entity.HasMany(x => x.DataWindows).WithOne(x => x.Ministry).HasForeignKey(x => x.MinistryId);
        });

        builder.Entity<MinistryDataWindow>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AfterCloseMessageAr).HasMaxLength(500);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450);
        });

        builder.Entity<Directorate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.HasMany(x => x.Departments).WithOne(x => x.Directorate).HasForeignKey(x => x.DirectorateId);
        });

        builder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.HasMany(x => x.Sections).WithOne(x => x.Department).HasForeignKey(x => x.DepartmentId);
        });

        builder.Entity<Section>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.HasMany(x => x.Units).WithOne(x => x.Section).HasForeignKey(x => x.SectionId);
        });

        builder.Entity<Unit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.NormalizedName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.JobNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.NationalIdEncrypted).IsRequired();
            entity.Property(x => x.NationalIdHash).IsRequired();
            entity.Property(x => x.VerifiedSource).HasMaxLength(50);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450);
            entity.Property(x => x.UpdatedByUserId).HasMaxLength(450);
            entity.Property(x => x.DeletedByUserId).HasMaxLength(450);
            entity.HasIndex(x => x.NationalIdHash);
            entity.HasIndex(x => new { x.MinistryId, x.NationalIdHash });
            entity.HasIndex(x => new { x.NationalIdHash, x.NormalizedName, x.MinistryId });
            entity.HasIndex(x => new { x.MinistryId, x.JobNumber }).IsUnique();
            entity.HasIndex(x => new { x.MinistryId, x.DirectorateId, x.DepartmentId, x.SectionId, x.UnitId });
            entity.HasIndex(x => new { x.MinistryId, x.NationalIdHash })
                .HasFilter("[IsDeleted] = 0")
                .IsUnique();
        });

        builder.Entity<UserScope>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.HasOne(x => x.Ministry).WithMany().HasForeignKey(x => x.MinistryId);
            entity.HasOne(x => x.Directorate).WithMany().HasForeignKey(x => x.DirectorateId);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);
            entity.HasOne(x => x.Section).WithMany().HasForeignKey(x => x.SectionId);
            entity.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.MinistryId);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IP).HasMaxLength(50);
            entity.HasIndex(x => x.Timestamp);
        });

        builder.Entity<ImportBatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UploadedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        builder.Entity<ApiClient>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.AllowedIps).HasMaxLength(500);
            entity.HasIndex(x => x.ClientId).IsUnique();
        });

        builder.Entity<ApiNonce>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nonce).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.ApiClientId, x.Nonce }).IsUnique();
        });
    }
}
