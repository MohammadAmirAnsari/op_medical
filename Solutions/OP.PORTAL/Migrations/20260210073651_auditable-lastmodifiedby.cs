using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class auditablelastmodifiedby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "Sponsors",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "OvmcRequests",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "OvmcPayments",
                newName: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "Sponsors",
                newName: "ChangedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "OvmcRequests",
                newName: "ChangedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "OvmcPayments",
                newName: "ChangedBy");
        }
    }
}
