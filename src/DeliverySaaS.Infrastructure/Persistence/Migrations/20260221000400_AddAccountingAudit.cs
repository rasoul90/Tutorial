using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class AddAccountingAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE [AuditEntries](
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [BranchId] uniqueidentifier NOT NULL,
    [EntityName] nvarchar(120) NOT NULL,
    [EntityId] uniqueidentifier NOT NULL,
    [Operation] nvarchar(80) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL
);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [AuditEntries];");
    }
}
