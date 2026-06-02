using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class paymenttablefields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BankAmount",
                table: "OvmcPayments",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCardName",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankFailureMessage",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankPaymentMode",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankRefNo",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankStatusCode",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankStatusMessage",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAmount",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankCardName",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankFailureMessage",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankPaymentMode",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankRefNo",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankStatusCode",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "BankStatusMessage",
                table: "OvmcPayments");
        }
    }
}
