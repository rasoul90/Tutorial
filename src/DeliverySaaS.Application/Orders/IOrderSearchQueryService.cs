using DeliverySaaS.Application.Common.Models;

namespace DeliverySaaS.Application.Orders;

public interface IOrderSearchQueryService
{
    Task<PagedResult<OrderSearchRowDto>> SearchAsync(bool includeArchive, int page, int size, CancellationToken cancellationToken = default);
}

public record OrderSearchRowDto(Guid Id, string OrderNumber, DateTime? DeliveredAt, bool IsArchived);
