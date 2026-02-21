using DeliverySaaS.Domain.Printing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliverySaaS.Infrastructure.Persistence.Configurations;

public class BranchPrintSettingConfiguration : IEntityTypeConfiguration<BranchPrintSetting>
{
    public void Configure(EntityTypeBuilder<BranchPrintSetting> builder)
    {
        builder.ToTable("BranchPrintSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrinterName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PaperSize).HasMaxLength(20).IsRequired();
    }
}

public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.ToTable("PrintJobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.JobType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
    }
}

public class PrintJobItemConfiguration : IEntityTypeConfiguration<PrintJobItem>
{
    public void Configure(EntityTypeBuilder<PrintJobItem> builder)
    {
        builder.ToTable("PrintJobItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReferenceType).HasMaxLength(60).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.PrintJobId });
    }
}
