using DeliverySaaS.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.UserName }).IsUnique();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Name }).IsUnique();
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Key }).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.UserId, x.RoleId }).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.RoleId, x.PermissionId }).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.TokenHash }).IsUnique();
    }
}
