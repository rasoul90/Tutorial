using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

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
        builder.Property(x => x.AmountToCollect).HasColumnType("decimal(18,2)");
        builder.Property(x => x.State)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
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
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.EventAt });
    }
}

public class OrderProblemConfiguration : IEntityTypeConfiguration<OrderProblem>
{
    public void Configure(EntityTypeBuilder<OrderProblem> builder)
    {
        builder.ToTable("OrderProblems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProblemType).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.IsResolved });
    }
}
