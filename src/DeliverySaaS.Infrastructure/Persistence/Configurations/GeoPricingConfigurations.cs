using DeliverySaaS.Domain.Geo.Entities;
using DeliverySaaS.Domain.Pricing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.GovernorateId, x.Name }).IsUnique();
    }
}

public class PricingCategoryConfiguration : IEntityTypeConfiguration<PricingCategory>
{
    public void Configure(EntityTypeBuilder<PricingCategory> builder)
    {
        builder.ToTable("PricingCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Name }).IsUnique();
    }
}

public class PricingRateConfiguration : IEntityTypeConfiguration<PricingRate>
{
    public void Configure(EntityTypeBuilder<PricingRate> builder)
    {
        builder.ToTable("PricingRates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Size1Rate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Size2Rate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Size3Rate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Size4Rate).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.PricingCategoryId, x.AreaId }).IsUnique();
    }
}
