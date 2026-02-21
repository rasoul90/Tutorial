using DeliverySaaS.Domain.Integration.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class PartnerConnectionConfiguration : IEntityTypeConfiguration<PartnerConnection>
{
    public void Configure(EntityTypeBuilder<PartnerConnection> builder)
    {
        builder.ToTable("PartnerConnections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartnerName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.BaseUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ApiKey).HasMaxLength(500).IsRequired();
    }
}

public class RoutingRuleConfiguration : IEntityTypeConfiguration<RoutingRule>
{
    public void Configure(EntityTypeBuilder<RoutingRule> builder)
    {
        builder.ToTable("RoutingRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RuleName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ConditionJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ActionJson).HasColumnType("nvarchar(max)").IsRequired();
    }
}

public class OrderHandoffConfiguration : IEntityTypeConfiguration<OrderHandoff>
{
    public void Configure(EntityTypeBuilder<OrderHandoff> builder)
    {
        builder.ToTable("OrderHandoffs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
    }
}

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
    }
}

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
    }
}
