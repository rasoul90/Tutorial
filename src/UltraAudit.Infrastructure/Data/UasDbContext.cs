using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UltraAudit.Domain.Entities;
using UltraAudit.Infrastructure.Identity;

namespace UltraAudit.Infrastructure.Data;

/// <summary>
/// سياق قاعدة البيانات لتطبيق Ultra Audit Services.
/// </summary>
public sealed class UasDbContext : IdentityDbContext<ApplicationUser>
{
    public UasDbContext(DbContextOptions<UasDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectRisk> ProjectRisks => Set<ProjectRisk>();
    public DbSet<ProjectProcedure> ProjectProcedures => Set<ProjectProcedure>();
    public DbSet<Dataset> Datasets => Set<Dataset>();
    public DbSet<DatasetField> DatasetFields => Set<DatasetField>();
    public DbSet<Analysis> Analyses => Set<Analysis>();
    public DbSet<AnalysisCommand> AnalysisCommands => Set<AnalysisCommand>();
    public DbSet<AnalysisRun> AnalysisRuns => Set<AnalysisRun>();
    public DbSet<ExceptionCase> ExceptionCases => Set<ExceptionCase>();
    public DbSet<ExceptionComment> ExceptionComments => Set<ExceptionComment>();
    public DbSet<WorkingPaper> WorkingPapers => Set<WorkingPaper>();
    public DbSet<CommandCatalogItem> CommandCatalog => Set<CommandCatalogItem>();
    public DbSet<ResourceGrant> ResourceGrants => Set<ResourceGrant>();
    public DbSet<AuditTrailEntry> AuditTrail => Set<AuditTrailEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Project>().HasIndex(project => project.Code).IsUnique();
        builder.Entity<Dataset>().HasIndex(dataset => dataset.StorageTable).IsUnique();
        builder.Entity<AnalysisRun>().HasIndex(run => run.IntegrityHash);
        builder.Entity<AuditTrailEntry>().HasIndex(entry => entry.IntegrityHash);
    }
}
