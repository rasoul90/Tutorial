using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class ProblemCatalogConfiguration : IEntityTypeConfiguration<ProblemCatalog>
{
    public void Configure(EntityTypeBuilder<ProblemCatalog> builder)
    {
        builder.ToTable("ProblemCatalogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Name });
    }
}

public class PickupAgentConfiguration : IEntityTypeConfiguration<PickupAgent>
{
    public void Configure(EntityTypeBuilder<PickupAgent> builder)
    {
        builder.ToTable("PickupAgents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public class DeliveryAgentConfiguration : IEntityTypeConfiguration<DeliveryAgent>
{
    public void Configure(EntityTypeBuilder<DeliveryAgent> builder)
    {
        builder.ToTable("DeliveryAgents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DeliveryFeePerOrder).HasColumnType("decimal(18,2)");
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();
        builder.Property(x => x.InternalNote).HasMaxLength(1000);
        builder.Property(x => x.AmountToCollect).HasColumnType("decimal(18,2)");
        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.OrderSize)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(x => x.ProblemStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DeliveredPriceWithDelivery).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeliveryFeeApplied).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeliveryAgentFeeApplied).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CompanyNetDeliveryProfit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MerchantDueAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.MerchantPaidAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MerchantRemainingAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MerchantSettlementStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(MerchantSettlementStatus.NotReady)
            .IsRequired();

        builder.Property(x => x.IsDeliveryAgentSettled).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsMerchantSettled).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.HasReturn).HasDefaultValue(false).IsRequired();

        builder.Property(x => x.HasProblem).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.State });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.DeliveredAt });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.IsDeliveryAgentSettled, x.IsMerchantSettled });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.HasReturn, x.State });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.MerchantId, x.IsDeliveryAgentSettled, x.IsMerchantSettled });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderNumber }).IsUnique();
    }
}

public class OrderEventConfiguration : IEntityTypeConfiguration<OrderEvent>
{
    public void Configure(EntityTypeBuilder<OrderEvent> builder)
    {
        builder.ToTable("OrderEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.EventAt });
    }
}

public class OrderProblemConfiguration : IEntityTypeConfiguration<OrderProblem>
{
    public void Configure(EntityTypeBuilder<OrderProblem> builder)
    {
        builder.ToTable("OrderProblems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.ResolutionType)
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.Status });
    }
}
