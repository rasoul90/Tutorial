namespace DeliverySaaS.Application.Payments;

public interface IMerchantPaymentsService
{
    Task<MerchantPaymentResultDto> CreatePaymentAsync(CreateMerchantPaymentRequest request, CancellationToken cancellationToken = default);
}
