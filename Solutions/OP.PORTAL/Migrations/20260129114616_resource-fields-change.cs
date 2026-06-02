using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class resourcefieldschange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResourceValue",
                table: "AppResources",
                newName: "ResourceValueEnglish");

            migrationBuilder.RenameColumn(
                name: "Culture",
                table: "AppResources",
                newName: "ResourceValueArabic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResourceValueEnglish",
                table: "AppResources",
                newName: "ResourceValue");

            migrationBuilder.RenameColumn(
                name: "ResourceValueArabic",
                table: "AppResources",
                newName: "Culture");
        }
    }
}
