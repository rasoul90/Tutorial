using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class AddOrderOperationalState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE [Orders] ADD [State] nvarchar(50) NOT NULL CONSTRAINT DF_Orders_State DEFAULT 'New';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE [Orders] DROP CONSTRAINT DF_Orders_State;");
        migrationBuilder.Sql("ALTER TABLE [Orders] DROP COLUMN [State];");
    }
}
