using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Orders;

public class OrderMobileService : IOrderMobileService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRequestContext _requestContext;

    public OrderMobileService(IOrderRepository orderRepository, IRequestContext requestContext)
    {
        _orderRepository = orderRepository;
        _requestContext = requestContext;
    }

    public async Task<MerchantDashboardDto> GetMerchantDashboardAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetMerchantDashboardAsync(merchantId, cancellationToken);
    }

    public async Task<Guid> CreateOrderByReservedQrAsync(CreateOrderByReservedQrRequest request, CancellationToken cancellationToken = default)
    {
        var branchId = _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");

        var order = new Order
        {
            OrderNumber = $"QR-{request.ReservedQr}",
            MerchantId = request.MerchantId,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Address = request.Address,
            AmountToCollect = request.AmountToCollect,
            BranchId = branchId
        };

        await _orderRepository.AddOrderAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        return order.Id;
    }

    public Task<IReadOnlyList<PickupTaskDto>> GetPickupTasksAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        => _orderRepository.GetPickupTaskListAsync(pageNumber, pageSize, cancellationToken);
}
