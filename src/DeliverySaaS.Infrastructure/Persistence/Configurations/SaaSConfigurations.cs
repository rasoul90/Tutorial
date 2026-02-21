using DeliverySaaS.Domain.SaaS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class SaasTenantConfiguration : IEntityTypeConfiguration<SaasTenant>
{
    public void Configure(EntityTypeBuilder<SaasTenant> builder)
    {
        builder.ToTable("SaasTenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class TenantBranchConfiguration : IEntityTypeConfiguration<TenantBranch>
{
    public void Configure(EntityTypeBuilder<TenantBranch> builder)
    {
        builder.ToTable("TenantBranches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
