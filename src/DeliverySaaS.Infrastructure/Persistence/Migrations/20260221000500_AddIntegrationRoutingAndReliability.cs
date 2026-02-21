using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class AddIntegrationRoutingAndReliability : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
ALTER TABLE [RoutingRules] ADD [GovernorateId] uniqueidentifier NOT NULL CONSTRAINT DF_RoutingRules_GovernorateId DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE [RoutingRules] ADD [PartnerConnectionId] uniqueidentifier NOT NULL CONSTRAINT DF_RoutingRules_PartnerConnectionId DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX [IX_RoutingRules_Tenant_Branch_Gov_Active] ON [RoutingRules]([TenantId],[BranchId],[GovernorateId],[IsActive]);
CREATE INDEX [IX_OutboxMessages_Tenant_Processed_CreatedAt] ON [OutboxMessages]([TenantId],[ProcessedAt],[CreatedAt]);
CREATE INDEX [IX_InboxMessages_Tenant_Type_Processed_ReceivedAt] ON [InboxMessages]([TenantId],[Type],[IsProcessed],[ReceivedAt]);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP INDEX IF EXISTS [IX_InboxMessages_Tenant_Type_Processed_ReceivedAt] ON [InboxMessages];
DROP INDEX IF EXISTS [IX_OutboxMessages_Tenant_Processed_CreatedAt] ON [OutboxMessages];
DROP INDEX IF EXISTS [IX_RoutingRules_Tenant_Branch_Gov_Active] ON [RoutingRules];
ALTER TABLE [RoutingRules] DROP CONSTRAINT DF_RoutingRules_PartnerConnectionId;
ALTER TABLE [RoutingRules] DROP CONSTRAINT DF_RoutingRules_GovernorateId;
ALTER TABLE [RoutingRules] DROP COLUMN [PartnerConnectionId];
ALTER TABLE [RoutingRules] DROP COLUMN [GovernorateId];
");
    }
}
