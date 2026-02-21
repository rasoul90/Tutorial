using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class HardeningSecurityAndStability : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "SaasTenants",
            type: "bit",
            nullable: false,
            defaultValue: false);
        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "SaasTenants",
            type: "datetime2",
            nullable: true);

        var tables = new[]
        {
            "TenantBranches", "Users", "Roles", "Permissions", "UserRoles", "RolePermissions",
            "Merchants", "PickupAgents", "DeliveryAgents", "Orders", "OrderEvents", "OrderProblems", "ProblemCatalogs",
            "Governorates", "Areas", "PricingCategories", "PricingRates", "MerchantSettlementRequests", "MerchantInvoices",
            "DeliveryReconciliations", "Payroll", "Expenses", "AuditEntries", "BranchPrintSettings", "PrintJobs", "PrintJobItems",
            "PartnerConnections", "RoutingRules", "OrderHandoffs", "OutboxMessages", "InboxMessages"
        };

        foreach (var table in tables)
        {
            migrationBuilder.AddColumn<bool>(name: "IsDeleted", table: table, type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<DateTime>(name: "DeletedAt", table: table, type: "datetime2", nullable: true);
        }

        migrationBuilder.AddColumn<Guid>(
            name: "GovernorateId",
            table: "PricingRates",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_TenantId_BranchId_State",
            table: "Orders",
            columns: new[] { "TenantId", "BranchId", "State" });

        migrationBuilder.CreateIndex(
            name: "IX_OrderEvents_OrderId",
            table: "OrderEvents",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_PricingRates_TenantId_GovernorateId",
            table: "PricingRates",
            columns: new[] { "TenantId", "GovernorateId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Orders_TenantId_BranchId_State", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_OrderEvents_OrderId", table: "OrderEvents");
        migrationBuilder.DropIndex(name: "IX_PricingRates_TenantId_GovernorateId", table: "PricingRates");

        migrationBuilder.DropColumn(name: "GovernorateId", table: "PricingRates");

        migrationBuilder.DropColumn(name: "IsDeleted", table: "SaasTenants");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "SaasTenants");

        var tables = new[]
        {
            "TenantBranches", "Users", "Roles", "Permissions", "UserRoles", "RolePermissions",
            "Merchants", "PickupAgents", "DeliveryAgents", "Orders", "OrderEvents", "OrderProblems", "ProblemCatalogs",
            "Governorates", "Areas", "PricingCategories", "PricingRates", "MerchantSettlementRequests", "MerchantInvoices",
            "DeliveryReconciliations", "Payroll", "Expenses", "AuditEntries", "BranchPrintSettings", "PrintJobs", "PrintJobItems",
            "PartnerConnections", "RoutingRules", "OrderHandoffs", "OutboxMessages", "InboxMessages"
        };

        foreach (var table in tables)
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: table);
            migrationBuilder.DropColumn(name: "DeletedAt", table: table);
        }
    }
}
