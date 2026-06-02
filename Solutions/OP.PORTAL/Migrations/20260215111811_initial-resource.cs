using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class initialresource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppResources",
                table: "AppResources");

            migrationBuilder.RenameTable(
                name: "AppResources",
                newName: "portal_app_resources");

            migrationBuilder.RenameIndex(
                name: "IX_AppResources_ResourceKey",
                table: "portal_app_resources",
                newName: "IX_portal_app_resources_ResourceKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_app_resources",
                table: "portal_app_resources",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_app_resources",
                table: "portal_app_resources");

            migrationBuilder.RenameTable(
                name: "portal_app_resources",
                newName: "AppResources");

            migrationBuilder.RenameIndex(
                name: "IX_portal_app_resources_ResourceKey",
                table: "AppResources",
                newName: "IX_AppResources_ResourceKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppResources",
                table: "AppResources",
                column: "Id");
        }
    }
}
