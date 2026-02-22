using DeliverySaaS.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class MerchantSettlementRequestConfiguration : IEntityTypeConfiguration<MerchantSettlementRequest>
{
    public void Configure(EntityTypeBuilder<MerchantSettlementRequest> builder)
    {
        builder.ToTable("MerchantSettlementRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
    }
}

public class MerchantInvoiceConfiguration : IEntityTypeConfiguration<MerchantInvoice>
{
    public void Configure(EntityTypeBuilder<MerchantInvoice> builder)
    {
        builder.ToTable("MerchantInvoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.InvoiceNumber }).IsUnique();
    }
}

public class DeliveryReconciliationConfiguration : IEntityTypeConfiguration<DeliveryReconciliation>
{
    public void Configure(EntityTypeBuilder<DeliveryReconciliation> builder)
    {
        builder.ToTable("DeliveryReconciliations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CollectedAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DeliveredAmount).HasColumnType("decimal(18,2)");
    }
}

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.ToTable("Payroll");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}


public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.EntityName, x.EntityId });
    }
}


public class MerchantPaymentConfiguration : IEntityTypeConfiguration<MerchantPayment>
{
    public void Configure(EntityTypeBuilder<MerchantPayment> builder)
    {
        builder.ToTable("MerchantPayments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNo).HasMaxLength(120);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.Method).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.MerchantId, x.PaymentDate });
    }
}

public class MerchantPaymentAllocationConfiguration : IEntityTypeConfiguration<MerchantPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<MerchantPaymentAllocation> builder)
    {
        builder.ToTable("MerchantPaymentAllocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllocatedAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.OrderId });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.MerchantPaymentId });
    }
}
