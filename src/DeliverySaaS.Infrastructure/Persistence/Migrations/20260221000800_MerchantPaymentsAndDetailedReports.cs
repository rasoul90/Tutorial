using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class MerchantPaymentsAndDetailedReports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(name: "MerchantPaidAmount", table: "Orders", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>(name: "MerchantRemainingAmount", table: "Orders", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<string>(name: "MerchantSettlementStatus", table: "Orders", type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NotReady");

        migrationBuilder.CreateTable(
            name: "MerchantPayments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Method = table.Column<int>(type: "int", nullable: false),
                ReferenceNo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_MerchantPayments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "MerchantPaymentAllocations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MerchantPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_MerchantPaymentAllocations", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_MerchantPayments_TenantId_BranchId_MerchantId_PaymentDate", table: "MerchantPayments", columns: new[] { "TenantId", "BranchId", "MerchantId", "PaymentDate" });
        migrationBuilder.CreateIndex(name: "IX_MerchantPaymentAllocations_TenantId_BranchId_OrderId", table: "MerchantPaymentAllocations", columns: new[] { "TenantId", "BranchId", "OrderId" });
        migrationBuilder.CreateIndex(name: "IX_Orders_TenantId_BranchId_MerchantId_IsDeliveryAgentSettled_IsMerchantSettled", table: "Orders", columns: new[] { "TenantId", "BranchId", "MerchantId", "IsDeliveryAgentSettled", "IsMerchantSettled" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MerchantPaymentAllocations");
        migrationBuilder.DropTable(name: "MerchantPayments");
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_MerchantId_IsDeliveryAgentSettled_IsMerchantSettled", table: "Orders");
        migrationBuilder.DropColumn(name: "MerchantPaidAmount", table: "Orders");
        migrationBuilder.DropColumn(name: "MerchantRemainingAmount", table: "Orders");
        migrationBuilder.DropColumn(name: "MerchantSettlementStatus", table: "Orders");
    }
}
