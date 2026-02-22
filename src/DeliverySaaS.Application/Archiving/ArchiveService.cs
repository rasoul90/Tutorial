using DeliverySaaS.Application.Auditing;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Operations.ArchiveEntities;

namespace DeliverySaaS.Application.Archiving;

public class ArchiveService : IArchiveService
{
    private readonly IArchiveRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public ArchiveService(IArchiveRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<int> RunArchiveAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-90);
        var orders = await _repository.GetOrdersEligibleForArchiveAsync(cutoff, cancellationToken);
        if (orders.Count == 0)
        {
            await _auditLogService.WriteAsync("ARCHIVE_RUN", "Orders", "0", "تشغيل الأرشفة - لا يوجد عناصر", "{}", cancellationToken: cancellationToken);
            return 0;
        }

        var ids = orders.Select(x => x.Id).ToList();
        var events = await _repository.GetOrderEventsAsync(ids, cancellationToken);
        var problems = await _repository.GetOrderProblemsAsync(ids, cancellationToken);

        await _repository.AddOrderArchivesAsync(orders.Select(x => new OrderArchive
        {
            OriginalOrderId = x.Id,
            OrderNumber = x.OrderNumber,
            MerchantId = x.MerchantId,
            DeliveredAt = x.DeliveredAt,
            MerchantDueAmount = x.MerchantDueAmount,
            IsMerchantSettled = x.IsMerchantSettled,
            HasReturn = x.HasReturn,
            ArchivedAt = DateTime.UtcNow,
            BranchId = x.BranchId
        }), cancellationToken);

        await _repository.AddOrderEventArchivesAsync(events.Select(x => new OrderEventArchive
        {
            OriginalOrderEventId = x.Id,
            OrderId = x.OrderId,
            EventType = x.EventType,
            Notes = x.Notes,
            EventAt = x.EventAt,
            ArchivedAt = DateTime.UtcNow,
            BranchId = x.BranchId
        }), cancellationToken);

        await _repository.AddOrderProblemArchivesAsync(problems.Select(x => new OrderProblemArchive
        {
            OriginalOrderProblemId = x.Id,
            OrderId = x.OrderId,
            Status = x.Status,
            ResolvedAt = x.ResolvedAt,
            ArchivedAt = DateTime.UtcNow,
            BranchId = x.BranchId
        }), cancellationToken);

        await _repository.RemoveOrderEventsAsync(events, cancellationToken);
        await _repository.RemoveOrderProblemsAsync(problems, cancellationToken);
        await _repository.RemoveOrdersAsync(orders, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync("ARCHIVE_RUN", "Orders", orders.Count.ToString(), "تشغيل الأرشفة", $"{{\"archived\":{orders.Count}}}", cancellationToken: cancellationToken);
        return orders.Count;
    }
}
