using DeliverySaaS.Application.Archiving;
using DeliverySaaS.Application.Auditing;
using DeliverySaaS.Application.Notifications;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Application.Payments;
using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.Repositories;
using DeliverySaaS.Tests.Common;
using DeliverySaaS.Domain.Accounting.Enums;
using DeliverySaaS.Domain.Operations.Entities;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.Tests.Unit;

public class Phase19ObservabilityAndArchiveTests
{
    [Fact]
    public async Task AuditLog_Created_OnPaymentCreation()
    {
        var (db, ctx) = CreateDb(nameof(AuditLog_Created_OnPaymentCreation));
        var merchantId = Guid.NewGuid();
        db.Orders.Add(new Order { TenantId = ctx.TenantId!.Value, BranchId = ctx.BranchId!.Value, MerchantId = merchantId, OrderNumber = "A", CustomerName = "C", CustomerPhone = "1", Address = "A", DeliveredAt = DateTime.UtcNow.AddDays(-1), MerchantDueAmount = 100, MerchantRemainingAmount = 100, IsDeliveryAgentSettled = true, MerchantSettlementStatus = MerchantSettlementStatus.Ready });
        await db.SaveChangesAsync();

        var audit = new AuditLogService(new AuditLogRepository(db, ctx), ctx);
        var notifications = new NotificationService(new NotificationRepository(db, ctx), ctx, new NoOpNotificationSender());
        var service = new MerchantPaymentsService(new MerchantPaymentsRepository(db, ctx), ctx, audit, notifications);

        await service.CreatePaymentAsync(new CreateMerchantPaymentRequest { MerchantId = merchantId, PaymentDate = DateTime.UtcNow, Amount = 50, Method = PaymentMethod.Courier, CreatedByUserId = Guid.NewGuid() });

        Assert.Contains(db.AuditLogs, x => x.Action == "PAYMENT_CREATED");
    }

    [Fact]
    public async Task Notification_Created_OnProblemReport()
    {
        var (db, ctx) = CreateDb(nameof(Notification_Created_OnProblemReport));
        var order = new Order { TenantId = ctx.TenantId!.Value, BranchId = ctx.BranchId!.Value, MerchantId = Guid.NewGuid(), OrderNumber = "B", CustomerName = "C", CustomerPhone = "1", Address = "A" };
        var catalogId = Guid.NewGuid();
        db.Orders.Add(order);
        db.ProblemCatalogs.Add(new ProblemCatalog { Id = catalogId, TenantId = ctx.TenantId!.Value, BranchId = ctx.BranchId!.Value, Name = "Issue", DynamicResolutionType = DynamicResolutionType.NOTE_ONLY });
        await db.SaveChangesAsync();

        var service = new OrderProblemService(new OrderRepository(db, ctx), ctx, new Infrastructure.Caching.ReferenceDataCacheService(db, ctx, new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions())), new NotificationService(new NotificationRepository(db, ctx), ctx, new NoOpNotificationSender()), new AuditLogService(new AuditLogRepository(db, ctx), ctx));

        await service.CreateProblemAsync(order.Id, catalogId, "note");

        Assert.Contains(db.Notifications, x => x.Type == Domain.Notifications.Enums.NotificationType.Problem);
    }

    [Fact]
    public async Task ArchiveJob_Skips_UnsettledOrders()
    {
        var (db, ctx) = CreateDb(nameof(ArchiveJob_Skips_UnsettledOrders));
        db.Orders.Add(new Order { TenantId = ctx.TenantId!.Value, BranchId = ctx.BranchId!.Value, MerchantId = Guid.NewGuid(), OrderNumber = "OLD", CustomerName = "C", CustomerPhone = "1", Address = "A", DeliveredAt = DateTime.UtcNow.AddDays(-120), IsMerchantSettled = false, HasReturn = false });
        await db.SaveChangesAsync();

        var svc = new ArchiveService(new ArchiveRepository(db, ctx), new AuditLogService(new AuditLogRepository(db, ctx), ctx));
        var archived = await svc.RunArchiveAsync();

        Assert.Equal(0, archived);
        Assert.Empty(db.OrdersArchive);
    }

    private static (ApplicationDbContext Db, TestRequestContext Ctx) CreateDb(string name)
    {
        var ctx = new TestRequestContext { TenantId = Guid.NewGuid(), BranchId = Guid.NewGuid() };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options;
        return (new ApplicationDbContext(options, ctx), ctx);
    }
}
