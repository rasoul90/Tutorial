using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class AddOrderProblemsWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE [ProblemCatalogs](
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [TenantId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [NameAr] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL
);

ALTER TABLE [Orders] ADD [HasProblem] bit NOT NULL CONSTRAINT DF_Orders_HasProblem DEFAULT 0;
ALTER TABLE [Orders] ADD [ProblemStatus] nvarchar(50) NOT NULL CONSTRAINT DF_Orders_ProblemStatus DEFAULT 'None';
ALTER TABLE [Orders] ADD [InternalNote] nvarchar(1000) NULL;

ALTER TABLE [OrderProblems] ADD [ProblemCatalogId] uniqueidentifier NOT NULL CONSTRAINT DF_OrderProblems_ProblemCatalogId DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE [OrderProblems] ADD [Status] nvarchar(50) NOT NULL CONSTRAINT DF_OrderProblems_Status DEFAULT 'Open';
ALTER TABLE [OrderProblems] ADD [ResolutionType] nvarchar(50) NULL;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
ALTER TABLE [OrderProblems] DROP CONSTRAINT DF_OrderProblems_Status;
ALTER TABLE [OrderProblems] DROP CONSTRAINT DF_OrderProblems_ProblemCatalogId;
ALTER TABLE [OrderProblems] DROP COLUMN [Status];
ALTER TABLE [OrderProblems] DROP COLUMN [ResolutionType];
ALTER TABLE [OrderProblems] DROP COLUMN [ProblemCatalogId];

ALTER TABLE [Orders] DROP CONSTRAINT DF_Orders_HasProblem;
ALTER TABLE [Orders] DROP CONSTRAINT DF_Orders_ProblemStatus;
ALTER TABLE [Orders] DROP COLUMN [HasProblem];
ALTER TABLE [Orders] DROP COLUMN [ProblemStatus];
ALTER TABLE [Orders] DROP COLUMN [InternalNote];

DROP TABLE IF EXISTS [ProblemCatalogs];
");
    }
}
