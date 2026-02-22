using System.Text.Json;
using DeliverySaaS.Application.Auditing;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Notifications;
using DeliverySaaS.Domain.Accounting.Entities;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Payments;

public class MerchantPaymentsService : IMerchantPaymentsService
{
    private readonly IMerchantPaymentsRepository _repository;
    private readonly IRequestContext _requestContext;
    private readonly IAuditLogService? _auditLogService;
    private readonly INotificationService? _notificationService;

    public MerchantPaymentsService(IMerchantPaymentsRepository repository, IRequestContext requestContext, IAuditLogService? auditLogService = null, INotificationService? notificationService = null)
    {
        _repository = repository;
        _requestContext = requestContext;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
    }

    public async Task<MerchantPaymentResultDto> CreatePaymentAsync(CreateMerchantPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0) throw new InvalidOperationException("Payment amount must be greater than zero.");

        var payment = new MerchantPayment
        {
            MerchantId = request.MerchantId,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            Method = request.Method,
            ReferenceNo = request.ReferenceNo,
            Notes = request.Notes,
            CreatedByUserId = request.CreatedByUserId
        };

        await _repository.AddPaymentAsync(payment, cancellationToken);

        decimal remaining = request.Amount;
        decimal allocated = 0;

        if (request.AutoAllocateFifo)
        {
            var readyOrders = await _repository.GetReadyOrdersForMerchantAsync(request.MerchantId, cancellationToken);
            foreach (var order in readyOrders)
            {
                if (remaining <= 0) break;
                var amountToAllocate = Math.Min(remaining, Math.Max(0, order.MerchantRemainingAmount));
                if (amountToAllocate <= 0) continue;
                await AllocateAsync(payment.Id, order, amountToAllocate, cancellationToken);
                remaining -= amountToAllocate;
                allocated += amountToAllocate;
            }
        }
        else
        {
            var manualIds = request.ManualAllocations.Select(x => x.OrderId).Distinct().ToList();
            var orders = await _repository.GetOrdersByIdsAsync(request.MerchantId, manualIds, cancellationToken);
            var map = orders.ToDictionary(x => x.Id);

            foreach (var item in request.ManualAllocations)
            {
                if (remaining <= 0) break;
                if (!map.TryGetValue(item.OrderId, out var order)) continue;
                var capped = Math.Min(item.Amount, remaining);
                var amountToAllocate = Math.Min(capped, Math.Max(0, order.MerchantRemainingAmount));
                if (amountToAllocate <= 0) continue;
                await AllocateAsync(payment.Id, order, amountToAllocate, cancellationToken);
                remaining -= amountToAllocate;
                allocated += amountToAllocate;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
        if (_auditLogService != null)
        {
            await _auditLogService.WriteAsync("PAYMENT_CREATED", "MerchantPayment", payment.Id.ToString(), "تم إنشاء دفعة تاجر", JsonSerializer.Serialize(new { amount = request.Amount, allocated }), cancellationToken: cancellationToken);
        }
        if (_notificationService != null)
        {
            await _notificationService.CreateAsync(null, "Finance", "دفعة تاجر جديدة", "تم تسجيل دفعة جديدة للتاجر", Domain.Notifications.Enums.NotificationType.Payment, "Payment", payment.Id, cancellationToken);
        }
        return new MerchantPaymentResultDto(payment.Id, allocated, remaining);
    }

    private async Task AllocateAsync(Guid paymentId, Order order, decimal allocatedAmount, CancellationToken cancellationToken)
    {
        await _repository.AddAllocationAsync(new MerchantPaymentAllocation
        {
            MerchantPaymentId = paymentId,
            OrderId = order.Id,
            AllocatedAmount = allocatedAmount,
            BranchId = RequiredBranchId()
        }, cancellationToken);

        order.MerchantPaidAmount += allocatedAmount;
        var due = order.MerchantDueAmount ?? 0m;
        order.MerchantRemainingAmount = Math.Max(0, due - order.MerchantPaidAmount);

        if (!order.IsDeliveryAgentSettled)
        {
            order.MerchantSettlementStatus = MerchantSettlementStatus.NotReady;
        }
        else if (order.MerchantRemainingAmount <= 0)
        {
            order.MerchantSettlementStatus = MerchantSettlementStatus.Paid;
            order.IsMerchantSettled = true;
            order.MerchantSettledAt ??= DateTime.UtcNow;
        }
        else if (order.MerchantPaidAmount > 0)
        {
            order.MerchantSettlementStatus = MerchantSettlementStatus.PartiallyPaid;
        }
        else
        {
            order.MerchantSettlementStatus = MerchantSettlementStatus.Ready;
        }

        await _repository.AddOrderEventAsync(new OrderEvent
        {
            OrderId = order.Id,
            EventType = "MerchantPaymentAllocated",
            Notes = JsonSerializer.Serialize(new { paymentId, allocatedAmount, remaining = order.MerchantRemainingAmount }),
            EventAt = DateTime.UtcNow,
            BranchId = RequiredBranchId()
        }, cancellationToken);
    }

    private Guid RequiredBranchId() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
}
