using DeliverySaaS.Domain.Accounting.Enums;

namespace DeliverySaaS.Application.Payments;

public record ManualAllocationRequestDto(Guid OrderId, decimal Amount);

public class CreateMerchantPaymentRequest
{
    public Guid MerchantId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool AutoAllocateFifo { get; set; } = true;
    public List<ManualAllocationRequestDto> ManualAllocations { get; set; } = [];
}

public record MerchantPaymentResultDto(Guid PaymentId, decimal AllocatedAmount, decimal UnallocatedAmount);
