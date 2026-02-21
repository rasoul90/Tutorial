using System.Linq.Expressions;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Common.Entities;
using DeliverySaaS.Domain.Geo.Entities;
using DeliverySaaS.Domain.Identity.Entities;
using DeliverySaaS.Domain.Integration.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Pricing.Entities;
using DeliverySaaS.Domain.Printing.Entities;
using DeliverySaaS.Domain.SaaS.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IRequestContext _requestContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IRequestContext requestContext) : base(options)
    {
        _requestContext = requestContext;
    }

    public DbSet<SaasTenant> SaasTenants => Set<SaasTenant>();
    public DbSet<TenantBranch> TenantBranches => Set<TenantBranch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<PickupAgent> PickupAgents => Set<PickupAgent>();
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();
    public DbSet<OrderProblem> OrderProblems => Set<OrderProblem>();
    public DbSet<ProblemCatalog> ProblemCatalogs => Set<ProblemCatalog>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<PricingCategory> PricingCategories => Set<PricingCategory>();
    public DbSet<PricingRate> PricingRates => Set<PricingRate>();
    public DbSet<MerchantSettlementRequest> MerchantSettlementRequests => Set<MerchantSettlementRequest>();
    public DbSet<MerchantInvoice> MerchantInvoices => Set<MerchantInvoice>();
    public DbSet<DeliveryReconciliation> DeliveryReconciliations => Set<DeliveryReconciliation>();
    public DbSet<Payroll> Payroll => Set<Payroll>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<BranchPrintSetting> BranchPrintSettings => Set<BranchPrintSetting>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<PrintJobItem> PrintJobItems => Set<PrintJobItem>();
    public DbSet<PartnerConnection> PartnerConnections => Set<PartnerConnection>();
    public DbSet<RoutingRule> RoutingRules => Set<RoutingRule>();
    public DbSet<OrderHandoff> OrderHandoffs => Set<OrderHandoff>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.TenantId)).IsRequired();
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.CreatedAt)).IsRequired();
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.IsDeleted)).HasDefaultValue(false).IsRequired();
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }

            if (typeof(BaseBranchEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseBranchEntity.BranchId)).IsRequired();
            }
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditAndTenantRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndTenantRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditAndTenantRules()
    {
        var tenantId = _requestContext.TenantId;
        if (!tenantId.HasValue)
        {
            throw new InvalidOperationException("TenantId is required in request context.");
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                entry.Entity.TenantId = tenantId.Value;
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = null;
                entry.Entity.IsDeleted = false;
                entry.Entity.DeletedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.TenantId = tenantId.Value;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.TenantId = tenantId.Value;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseBranchEntity>())
        {
            if ((entry.State == EntityState.Added || entry.State == EntityState.Modified) && !entry.Entity.BranchId.Equals(_requestContext.BranchId))
            {
                if (!_requestContext.BranchId.HasValue)
                {
                    throw new InvalidOperationException("BranchId is required for branch entities.");
                }

                entry.Entity.BranchId = _requestContext.BranchId.Value;
            }
        }
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var prop = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var body = Expression.Equal(prop, Expression.Constant(false));
        return Expression.Lambda(body, parameter);
    }
}
