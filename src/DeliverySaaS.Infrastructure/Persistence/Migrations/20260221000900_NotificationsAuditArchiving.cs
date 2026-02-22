using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class NotificationsAuditArchiving : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TargetRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                BodyAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                RelatedEntityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsRead = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Notifications", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Action = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                EntityId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                SummaryAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                DiffJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Ip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.Id));

        migrationBuilder.CreateTable(name: "OrdersArchive", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OriginalOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false), MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true), MerchantDueAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true), IsMerchantSettled = table.Column<bool>(type: "bit", nullable: false), HasReturn = table.Column<bool>(type: "bit", nullable: false), ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false), CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false), UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true), IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false), DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true) }, constraints: table => table.PrimaryKey("PK_OrdersArchive", x => x.Id));
        migrationBuilder.CreateTable(name: "OrderEventsArchive", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OriginalOrderEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false), Notes = table.Column<string>(type: "nvarchar(max)", nullable: true), EventAt = table.Column<DateTime>(type: "datetime2", nullable: false), ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false), CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false), UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true), IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false), DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true) }, constraints: table => table.PrimaryKey("PK_OrderEventsArchive", x => x.Id));
        migrationBuilder.CreateTable(name: "OrderProblemsArchive", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OriginalOrderProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false), ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true), ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false), CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false), UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true), IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false), DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true) }, constraints: table => table.PrimaryKey("PK_OrderProblemsArchive", x => x.Id));

        migrationBuilder.AddColumn<DateTime>(name: "ReturnedToMerchantAt", table: "Orders", type: "datetime2", nullable: true);

        migrationBuilder.CreateIndex(name: "IX_Notifications_TenantId_BranchId_IsRead_CreatedAt", table: "Notifications", columns: new[] { "TenantId", "BranchId", "IsRead", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_TenantId_BranchId_CreatedAt", table: "AuditLogs", columns: new[] { "TenantId", "BranchId", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_Orders_TenantId_BranchId_MerchantId_DeliveredAt", table: "Orders", columns: new[] { "TenantId", "BranchId", "MerchantId", "DeliveredAt" });
        migrationBuilder.CreateIndex(name: "IX_OrderEvents_TenantId_BranchId_OrderId_CreatedAt", table: "OrderEvents", columns: new[] { "TenantId", "BranchId", "OrderId", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Notifications");
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "OrdersArchive");
        migrationBuilder.DropTable(name: "OrderEventsArchive");
        migrationBuilder.DropTable(name: "OrderProblemsArchive");
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_MerchantId_DeliveredAt", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_OrderEvents_TenantId_BranchId_OrderId_CreatedAt", table: "OrderEvents");
        migrationBuilder.DropColumn(name: "ReturnedToMerchantAt", table: "Orders");
    }
}
