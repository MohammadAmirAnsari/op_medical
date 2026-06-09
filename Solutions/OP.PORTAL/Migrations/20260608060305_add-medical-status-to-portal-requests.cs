using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class addmedicalstatustoportalrequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalStatus",
                table: "portal_requests",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql("ALTER TABLE `portal_requests` MODIFY COLUMN `MedicalStatus` VARCHAR(50) AFTER `PaymentStatus`;");

            migrationBuilder.AlterColumn<decimal>(
                name: "BankAmount",
                table: "portal_payments",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "portal_payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

                
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalStatus",
                table: "portal_requests");

            migrationBuilder.AlterColumn<decimal>(
                name: "BankAmount",
                table: "portal_payments",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "portal_payments",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
