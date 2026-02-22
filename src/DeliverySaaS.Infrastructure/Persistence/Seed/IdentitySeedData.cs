using DeliverySaaS.API.Security;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Identity.Entities;
using DeliverySaaS.Domain.SaaS.Entities;

namespace DeliverySaaS.Infrastructure.Persistence.Seed;

public static class IdentitySeedData
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, RequestContext requestContext, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (dbContext.Users.Any())
        {
            return;
        }

        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var branchId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        requestContext.TenantId = tenantId;
        requestContext.BranchId = branchId;

        dbContext.SaasTenants.Add(new SaasTenant { Id = tenantId, Name = "Default Tenant", Code = "DEFAULT" });
        dbContext.TenantBranches.Add(new TenantBranch { Id = branchId, TenantId = tenantId, Name = "Main Branch", Code = "MAIN" });

        var permissions = new[]
        {
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "Create Orders", Key = "ORDER.CREATE" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "View Orders", Key = "ORDER.VIEW" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "Transition Orders", Key = "ORDER.TRANSITION" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "Manage Problems", Key = "ORDER_PROBLEM.MANAGE" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "View Branch Reports", Key = "REPORT.BRANCH.VIEW" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "View Company Reports", Key = "REPORT.COMPANY.VIEW" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "Manage Payments", Key = "PAYMENT.MANAGE" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "Tenant Management", Key = "TENANT.MANAGE" },
            new Permission { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "SaaS Management", Key = "SAAS.MANAGE" }
        };
        dbContext.Permissions.AddRange(permissions);

        var branchManagerRole = new Role { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = branchId, Name = "BranchManager", Description = "Branch scoped admin" };
        var companyAdminRole = new Role { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "CompanyAdmin", Description = "Company scoped admin" };
        var saasAdminRole = new Role { Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null, Name = "SaaSAdmin", Description = "SaaS scoped admin" };
        dbContext.Roles.AddRange(branchManagerRole, companyAdminRole, saasAdminRole);

        var branchManager = new User
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = branchId,
            UserName = "branch.admin", FullName = "Branch Manager", PasswordHash = passwordHasher.Hash("P@ssw0rd!"), IsActive = true
        };
        var companyAdmin = new User
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null,
            UserName = "company.admin", FullName = "Company Admin", PasswordHash = passwordHasher.Hash("P@ssw0rd!"), IsActive = true
        };
        var saasAdmin = new User
        {
            Id = Guid.NewGuid(), TenantId = tenantId, BranchId = null,
            UserName = "saas.admin", FullName = "SaaS Admin", PasswordHash = passwordHasher.Hash("P@ssw0rd!"), IsActive = true
        };
        dbContext.Users.AddRange(branchManager, companyAdmin, saasAdmin);

        dbContext.UserRoles.AddRange(
            new UserRole { TenantId = tenantId, BranchId = branchId, UserId = branchManager.Id, RoleId = branchManagerRole.Id },
            new UserRole { TenantId = tenantId, BranchId = null, UserId = companyAdmin.Id, RoleId = companyAdminRole.Id },
            new UserRole { TenantId = tenantId, BranchId = null, UserId = saasAdmin.Id, RoleId = saasAdminRole.Id });

        var branchPerms = permissions.Where(p => p.Key is "ORDER.CREATE" or "ORDER.VIEW" or "ORDER.TRANSITION" or "ORDER_PROBLEM.MANAGE" or "REPORT.BRANCH.VIEW" or "PAYMENT.MANAGE").ToList();
        var companyPerms = permissions.Where(p => p.Key is not "SAAS.MANAGE").ToList();

        dbContext.RolePermissions.AddRange(
            branchPerms.Select(p => new RolePermission { TenantId = tenantId, BranchId = branchId, RoleId = branchManagerRole.Id, PermissionId = p.Id }));
        dbContext.RolePermissions.AddRange(
            companyPerms.Select(p => new RolePermission { TenantId = tenantId, BranchId = null, RoleId = companyAdminRole.Id, PermissionId = p.Id }));
        dbContext.RolePermissions.AddRange(
            permissions.Select(p => new RolePermission { TenantId = tenantId, BranchId = null, RoleId = saasAdminRole.Id, PermissionId = p.Id }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
