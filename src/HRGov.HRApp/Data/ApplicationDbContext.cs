using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobTitle> JobTitles => Set<JobTitle>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<EmployeeTraining> EmployeeTrainings => Set<EmployeeTraining>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<GovernmentDocument> GovernmentDocuments => Set<GovernmentDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>()
            .HasIndex(e => e.NationalId)
            .IsUnique();

        builder.Entity<Department>()
            .HasMany(d => d.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JobTitle>()
            .HasMany(j => j.Employees)
            .WithOne(e => e.JobTitle)
            .HasForeignKey(e => e.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeaveRequest>()
            .HasOne(l => l.ApprovedBy)
            .WithMany()
            .HasForeignKey(l => l.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EmployeeTraining>()
            .HasKey(et => new { et.EmployeeId, et.TrainingSessionId });

        builder.Entity<EmployeeTraining>()
            .HasOne(et => et.Employee)
            .WithMany(e => e.TrainingHistory)
            .HasForeignKey(et => et.EmployeeId);

        builder.Entity<EmployeeTraining>()
            .HasOne(et => et.TrainingSession)
            .WithMany(ts => ts.Participants)
            .HasForeignKey(et => et.TrainingSessionId);
    }
}
