using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliverySaaS.Infrastructure.Persistence.Migrations;

public partial class AuthRbacPhaseA : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Roles_TenantId_Name", table: "Roles");
        migrationBuilder.DropIndex(name: "IX_Permissions_TenantId_Key", table: "Permissions");
        migrationBuilder.DropIndex(name: "IX_UserRoles_TenantId_UserId_RoleId", table: "UserRoles");
        migrationBuilder.DropIndex(name: "IX_RolePermissions_TenantId_RoleId_PermissionId", table: "RolePermissions");

        migrationBuilder.AddColumn<Guid>(name: "BranchId", table: "Users", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "BranchId", table: "Roles", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "BranchId", table: "Permissions", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "BranchId", table: "UserRoles", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "BranchId", table: "RolePermissions", type: "uniqueidentifier", nullable: true);

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_RefreshTokens", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Roles_TenantId_BranchId_Name", table: "Roles", columns: new[] { "TenantId", "BranchId", "Name" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Permissions_TenantId_BranchId_Key", table: "Permissions", columns: new[] { "TenantId", "BranchId", "Key" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_UserRoles_TenantId_BranchId_UserId_RoleId", table: "UserRoles", columns: new[] { "TenantId", "BranchId", "UserId", "RoleId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_RolePermissions_TenantId_BranchId_RoleId_PermissionId", table: "RolePermissions", columns: new[] { "TenantId", "BranchId", "RoleId", "PermissionId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_TenantId_TokenHash", table: "RefreshTokens", columns: new[] { "TenantId", "TokenHash" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RefreshTokens");

        migrationBuilder.DropIndex(name: "IX_Roles_TenantId_BranchId_Name", table: "Roles");
        migrationBuilder.DropIndex(name: "IX_Permissions_TenantId_BranchId_Key", table: "Permissions");
        migrationBuilder.DropIndex(name: "IX_UserRoles_TenantId_BranchId_UserId_RoleId", table: "UserRoles");
        migrationBuilder.DropIndex(name: "IX_RolePermissions_TenantId_BranchId_RoleId_PermissionId", table: "RolePermissions");

        migrationBuilder.DropColumn(name: "BranchId", table: "Users");
        migrationBuilder.DropColumn(name: "BranchId", table: "Roles");
        migrationBuilder.DropColumn(name: "BranchId", table: "Permissions");
        migrationBuilder.DropColumn(name: "BranchId", table: "UserRoles");
        migrationBuilder.DropColumn(name: "BranchId", table: "RolePermissions");

        migrationBuilder.CreateIndex(name: "IX_Roles_TenantId_Name", table: "Roles", columns: new[] { "TenantId", "Name" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Permissions_TenantId_Key", table: "Permissions", columns: new[] { "TenantId", "Key" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_UserRoles_TenantId_UserId_RoleId", table: "UserRoles", columns: new[] { "TenantId", "UserId", "RoleId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_RolePermissions_TenantId_RoleId_PermissionId", table: "RolePermissions", columns: new[] { "TenantId", "RoleId", "PermissionId" }, unique: true);
    }
}
