namespace DeliverySaaS.Application.Reports;

public interface IMerchantLedgerQueryService
{
    Task<MerchantLedgerResultDto> GetLedgerAsync(Guid merchantId, DateTime from, DateTime to, int page, int size, CancellationToken cancellationToken = default);
}
