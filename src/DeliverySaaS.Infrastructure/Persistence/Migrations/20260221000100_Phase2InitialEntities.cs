using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class Phase2InitialEntities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE [SaasTenants]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Name] nvarchar(200) NOT NULL,[Code] nvarchar(50) NOT NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [TenantBranches]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Name] nvarchar(200) NOT NULL,[Code] nvarchar(50) NOT NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Users]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[UserName] nvarchar(100) NOT NULL,[FullName] nvarchar(200) NOT NULL,[PasswordHash] nvarchar(500) NOT NULL,[PhoneNumber] nvarchar(30) NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Roles]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Name] nvarchar(100) NOT NULL,[Description] nvarchar(500) NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Permissions]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Name] nvarchar(120) NOT NULL,[Key] nvarchar(120) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [UserRoles]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[UserId] uniqueidentifier NOT NULL,[RoleId] uniqueidentifier NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [RolePermissions]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[RoleId] uniqueidentifier NOT NULL,[PermissionId] uniqueidentifier NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Governorates]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Name] nvarchar(100) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Areas]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[GovernorateId] uniqueidentifier NOT NULL,[Name] nvarchar(120) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);

CREATE TABLE [Merchants]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[Name] nvarchar(200) NOT NULL,[ContactPerson] nvarchar(200) NULL,[PhoneNumber] nvarchar(30) NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [PickupAgents]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[Name] nvarchar(200) NOT NULL,[PhoneNumber] nvarchar(30) NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [DeliveryAgents]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[Name] nvarchar(200) NOT NULL,[PhoneNumber] nvarchar(30) NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Orders]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[OrderNumber] nvarchar(50) NOT NULL,[MerchantId] uniqueidentifier NOT NULL,[CustomerName] nvarchar(200) NOT NULL,[CustomerPhone] nvarchar(30) NOT NULL,[Address] nvarchar(500) NOT NULL,[AmountToCollect] decimal(18,2) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [OrderEvents]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[OrderId] uniqueidentifier NOT NULL,[EventType] nvarchar(100) NOT NULL,[Notes] nvarchar(500) NULL,[EventAt] datetime2 NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [OrderProblems]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[OrderId] uniqueidentifier NOT NULL,[ProblemType] nvarchar(100) NOT NULL,[Notes] nvarchar(500) NULL,[IsResolved] bit NOT NULL,[ResolvedAt] datetime2 NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);

CREATE TABLE [PricingCategories]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[Name] nvarchar(120) NOT NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [PricingRates]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[PricingCategoryId] uniqueidentifier NOT NULL,[AreaId] uniqueidentifier NOT NULL,[Size1Rate] decimal(18,2) NOT NULL,[Size2Rate] decimal(18,2) NOT NULL,[Size3Rate] decimal(18,2) NOT NULL,[Size4Rate] decimal(18,2) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);

CREATE TABLE [MerchantSettlementRequests]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[MerchantId] uniqueidentifier NOT NULL,[Amount] decimal(18,2) NOT NULL,[Status] nvarchar(40) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [MerchantInvoices]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[MerchantId] uniqueidentifier NOT NULL,[InvoiceNumber] nvarchar(80) NOT NULL,[TotalAmount] decimal(18,2) NOT NULL,[InvoiceDate] datetime2 NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [DeliveryReconciliations]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[DeliveryAgentId] uniqueidentifier NOT NULL,[CollectedAmount] decimal(18,2) NOT NULL,[DeliveredAmount] decimal(18,2) NOT NULL,[ReconciledAt] datetime2 NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Payroll]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[UserId] uniqueidentifier NOT NULL,[Amount] decimal(18,2) NOT NULL,[PayrollDate] datetime2 NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [Expenses]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[Category] nvarchar(100) NOT NULL,[Amount] decimal(18,2) NOT NULL,[Notes] nvarchar(500) NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);

CREATE TABLE [BranchPrintSettings]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[PrinterName] nvarchar(200) NOT NULL,[PaperSize] nvarchar(20) NOT NULL,[AutoPrint] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [PrintJobs]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[JobType] nvarchar(60) NOT NULL,[Status] nvarchar(40) NOT NULL,[RequestedAt] datetime2 NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [PrintJobItems]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[PrintJobId] uniqueidentifier NOT NULL,[ReferenceId] uniqueidentifier NOT NULL,[ReferenceType] nvarchar(60) NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);

CREATE TABLE [PartnerConnections]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[PartnerName] nvarchar(150) NOT NULL,[BaseUrl] nvarchar(500) NOT NULL,[ApiKey] nvarchar(500) NOT NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [RoutingRules]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[RuleName] nvarchar(150) NOT NULL,[ConditionJson] nvarchar(max) NOT NULL,[ActionJson] nvarchar(max) NOT NULL,[IsActive] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [OrderHandoffs]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[BranchId] uniqueidentifier NOT NULL,[OrderId] uniqueidentifier NOT NULL,[PartnerConnectionId] uniqueidentifier NOT NULL,[Status] nvarchar(40) NOT NULL,[SentAt] datetime2 NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [OutboxMessages]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Type] nvarchar(120) NOT NULL,[Payload] nvarchar(max) NOT NULL,[ProcessedAt] datetime2 NULL,[Error] nvarchar(max) NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
CREATE TABLE [InboxMessages]([Id] uniqueidentifier NOT NULL PRIMARY KEY,[TenantId] uniqueidentifier NOT NULL,[Type] nvarchar(120) NOT NULL,[Payload] nvarchar(max) NOT NULL,[ReceivedAt] datetime2 NOT NULL,[IsProcessed] bit NOT NULL,[CreatedAt] datetime2 NOT NULL,[UpdatedAt] datetime2 NULL);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP TABLE IF EXISTS [InboxMessages];
DROP TABLE IF EXISTS [OutboxMessages];
DROP TABLE IF EXISTS [OrderHandoffs];
DROP TABLE IF EXISTS [RoutingRules];
DROP TABLE IF EXISTS [PartnerConnections];
DROP TABLE IF EXISTS [PrintJobItems];
DROP TABLE IF EXISTS [PrintJobs];
DROP TABLE IF EXISTS [BranchPrintSettings];
DROP TABLE IF EXISTS [Expenses];
DROP TABLE IF EXISTS [Payroll];
DROP TABLE IF EXISTS [DeliveryReconciliations];
DROP TABLE IF EXISTS [MerchantInvoices];
DROP TABLE IF EXISTS [MerchantSettlementRequests];
DROP TABLE IF EXISTS [PricingRates];
DROP TABLE IF EXISTS [PricingCategories];
DROP TABLE IF EXISTS [OrderProblems];
DROP TABLE IF EXISTS [OrderEvents];
DROP TABLE IF EXISTS [Orders];
DROP TABLE IF EXISTS [DeliveryAgents];
DROP TABLE IF EXISTS [PickupAgents];
DROP TABLE IF EXISTS [Merchants];
DROP TABLE IF EXISTS [Areas];
DROP TABLE IF EXISTS [Governorates];
DROP TABLE IF EXISTS [RolePermissions];
DROP TABLE IF EXISTS [UserRoles];
DROP TABLE IF EXISTS [Permissions];
DROP TABLE IF EXISTS [Roles];
DROP TABLE IF EXISTS [Users];
DROP TABLE IF EXISTS [TenantBranches];
DROP TABLE IF EXISTS [SaasTenants];
");
    }
}
