using DeliverySaaS.Domain.Auditing.Entities;
using DeliverySaaS.Domain.Notifications.Entities;
using DeliverySaaS.Domain.Operations.ArchiveEntities;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TitleAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.BodyAr).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.RelatedEntityType).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.IsRead, x.CreatedAt });
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SummaryAr).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DiffJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.CreatedAt });
    }
}

public class OrderArchiveConfiguration : IEntityTypeConfiguration<OrderArchive>
{
    public void Configure(EntityTypeBuilder<OrderArchive> builder)
    {
        builder.ToTable("OrdersArchive");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MerchantDueAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.DeliveredAt });
    }
}

public class OrderEventArchiveConfiguration : IEntityTypeConfiguration<OrderEventArchive>
{
    public void Configure(EntityTypeBuilder<OrderEventArchive> builder)
    {
        builder.ToTable("OrderEventsArchive");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.CreatedAt });
    }
}

public class OrderProblemArchiveConfiguration : IEntityTypeConfiguration<OrderProblemArchive>
{
    public void Configure(EntityTypeBuilder<OrderProblemArchive> builder)
    {
        builder.ToTable("OrderProblemsArchive");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId, x.Status });
    }
}
