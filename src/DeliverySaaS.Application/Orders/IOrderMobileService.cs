using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Orders;

public interface IOrderMobileService
{
    Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<Guid> CreateOrderByReservedQrAsync(CreateOrderByReservedQrRequest request, CancellationToken cancellationToken = default);
    Task<List<PickupTaskDto>> GetPickupTasksAsync(int take = 50, CancellationToken cancellationToken = default);
}

public record MerchantDashboardDto(int TotalOrders, int OpenProblems);

public record CreateOrderByReservedQrRequest(
    string ReservedQr,
    Guid MerchantId,
    string CustomerName,
    string CustomerPhone,
    string Address,
    decimal AmountToCollect);

public record PickupTaskDto(Guid OrderId, string OrderNumber, string CustomerName, string CustomerPhone, string Address, OperationalState State);
