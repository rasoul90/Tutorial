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
        var total = await _orderRepository.CountOrdersByMerchantAsync(merchantId, cancellationToken);
        var open = await _orderRepository.CountOpenProblemsByMerchantAsync(merchantId, cancellationToken);
        return new MerchantDashboardDto(total, open);
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

    public async Task<List<PickupTaskDto>> GetPickupTasksAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        var list = await _orderRepository.GetPickupTaskListAsync(take, cancellationToken);
        return list.Select(x => new PickupTaskDto(x.Id, x.OrderNumber, x.CustomerName, x.CustomerPhone, x.Address, x.State)).ToList();
    }
}
