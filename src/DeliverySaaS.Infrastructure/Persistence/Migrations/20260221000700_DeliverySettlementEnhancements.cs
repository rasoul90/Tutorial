using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class DeliverySettlementEnhancements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "DeliveryAgentId", table: "Orders", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "GovernorateId", table: "Orders", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "PricingCategoryId", table: "Orders", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<string>(name: "OrderSize", table: "Orders", type: "nvarchar(20)", maxLength: 20, nullable: true);

        migrationBuilder.AddColumn<DateTime>(name: "DeliveredAt", table: "Orders", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "DeliveredPriceWithDelivery", table: "Orders", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "DeliveryFeeApplied", table: "Orders", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "DeliveryAgentFeeApplied", table: "Orders", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "CompanyNetDeliveryProfit", table: "Orders", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "MerchantDueAmount", table: "Orders", type: "decimal(18,2)", nullable: true);

        migrationBuilder.AddColumn<bool>(name: "IsDeliveryAgentSettled", table: "Orders", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTime>(name: "DeliveryAgentSettledAt", table: "Orders", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "IsMerchantSettled", table: "Orders", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTime>(name: "MerchantSettledAt", table: "Orders", type: "datetime2", nullable: true);

        migrationBuilder.AddColumn<bool>(name: "HasReturn", table: "Orders", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTime>(name: "ReturnInitiatedAt", table: "Orders", type: "datetime2", nullable: true);

        migrationBuilder.AddColumn<decimal>(name: "DeliveryFeePerOrder", table: "DeliveryAgents", type: "decimal(18,2)", nullable: false, defaultValue: 0m);

        migrationBuilder.CreateIndex(name: "IX_Orders_TenantId_BranchId_DeliveredAt", table: "Orders", columns: new[] { "TenantId", "BranchId", "DeliveredAt" });
        migrationBuilder.CreateIndex(name: "IX_Orders_TenantId_BranchId_IsDeliveryAgentSettled_IsMerchantSettled", table: "Orders", columns: new[] { "TenantId", "BranchId", "IsDeliveryAgentSettled", "IsMerchantSettled" });
        migrationBuilder.CreateIndex(name: "IX_Orders_TenantId_BranchId_HasReturn_State", table: "Orders", columns: new[] { "TenantId", "BranchId", "HasReturn", "State" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_DeliveredAt", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_IsDeliveryAgentSettled_IsMerchantSettled", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_HasReturn_State", table: "Orders");

        migrationBuilder.DropColumn(name: "DeliveryAgentId", table: "Orders");
        migrationBuilder.DropColumn(name: "GovernorateId", table: "Orders");
        migrationBuilder.DropColumn(name: "PricingCategoryId", table: "Orders");
        migrationBuilder.DropColumn(name: "OrderSize", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveredAt", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveredPriceWithDelivery", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryFeeApplied", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryAgentFeeApplied", table: "Orders");
        migrationBuilder.DropColumn(name: "CompanyNetDeliveryProfit", table: "Orders");
        migrationBuilder.DropColumn(name: "MerchantDueAmount", table: "Orders");
        migrationBuilder.DropColumn(name: "IsDeliveryAgentSettled", table: "Orders");
        migrationBuilder.DropColumn(name: "DeliveryAgentSettledAt", table: "Orders");
        migrationBuilder.DropColumn(name: "IsMerchantSettled", table: "Orders");
        migrationBuilder.DropColumn(name: "MerchantSettledAt", table: "Orders");
        migrationBuilder.DropColumn(name: "HasReturn", table: "Orders");
        migrationBuilder.DropColumn(name: "ReturnInitiatedAt", table: "Orders");

        migrationBuilder.DropColumn(name: "DeliveryFeePerOrder", table: "DeliveryAgents");
    }
}
