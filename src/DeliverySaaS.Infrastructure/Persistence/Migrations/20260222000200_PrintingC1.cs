using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class PrintingC1 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "BarcodeType", table: "BranchPrintSettings", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Both");
        migrationBuilder.AddColumn<int>(name: "Copies", table: "BranchPrintSettings", type: "int", nullable: false, defaultValue: 1);

        migrationBuilder.AddColumn<string>(name: "FileName", table: "PrintJobs", type: "nvarchar(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PayloadJson", table: "PrintJobItems", type: "nvarchar(2000)", maxLength: 2000, nullable: true);

        migrationBuilder.CreateIndex(name: "IX_BranchPrintSettings_TenantId_BranchId", table: "BranchPrintSettings", columns: new[] { "TenantId", "BranchId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_BranchPrintSettings_TenantId_BranchId", table: "BranchPrintSettings");
        migrationBuilder.DropColumn(name: "BarcodeType", table: "BranchPrintSettings");
        migrationBuilder.DropColumn(name: "Copies", table: "BranchPrintSettings");
        migrationBuilder.DropColumn(name: "FileName", table: "PrintJobs");
        migrationBuilder.DropColumn(name: "PayloadJson", table: "PrintJobItems");
    }
}
