using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class urnnorequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OvmcUrnNumber",
                table: "OvmcRequests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvmcUrnNumber",
                table: "OvmcRequests");
        }
    }
}
