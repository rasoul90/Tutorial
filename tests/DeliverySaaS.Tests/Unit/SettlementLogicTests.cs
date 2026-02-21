using DeliverySaaS.Application.Accounting;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Tests.Common;

namespace DeliverySaaS.Tests.Unit;

public class SettlementLogicTests
{
    [Fact]
    public async Task ApproveSettlementAndGenerateInvoice_ApprovesSettlementAndCreatesInvoiceAndAudit()
    {
        var repo = new FakeAccountingRepository();
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var service = new AccountingService(repo, ctx);

        var settlement = new MerchantSettlementRequest
        {
            Id = Guid.NewGuid(),
            MerchantId = Guid.NewGuid(),
            Amount = 150m,
            Status = "Pending",
            BranchId = ctx.BranchId!.Value
        };
        repo.Settlements[settlement.Id] = settlement;

        var invoiceId = await service.ApproveSettlementAndGenerateInvoiceAsync(settlement.Id);

        Assert.Equal("Approved", settlement.Status);
        Assert.Contains(repo.Invoices, x => x.Id == invoiceId && x.TotalAmount == settlement.Amount);
        Assert.True(repo.Audits.Count >= 2);
    }
}
