using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sponsors",
                table: "Sponsors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SmsRequests",
                table: "SmsRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OvmcRequests",
                table: "OvmcRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OvmcPayments",
                table: "OvmcPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OvmcAuditLogs",
                table: "OvmcAuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneralMasters",
                table: "GeneralMasters");

            migrationBuilder.RenameTable(
                name: "Sponsors",
                newName: "portal_sponsors");

            migrationBuilder.RenameTable(
                name: "SmsRequests",
                newName: "portal_sms_logs");

            migrationBuilder.RenameTable(
                name: "OvmcRequests",
                newName: "portal_requests");

            migrationBuilder.RenameTable(
                name: "OvmcPayments",
                newName: "portal_payments");

            migrationBuilder.RenameTable(
                name: "OvmcAuditLogs",
                newName: "portal_audit_logs");

            migrationBuilder.RenameTable(
                name: "GeneralMasters",
                newName: "portal_general_master");

            migrationBuilder.RenameIndex(
                name: "IX_OvmcPayments_OrderNo",
                table: "portal_payments",
                newName: "IX_portal_payments_OrderNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_sponsors",
                table: "portal_sponsors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_sms_logs",
                table: "portal_sms_logs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_requests",
                table: "portal_requests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_payments",
                table: "portal_payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_audit_logs",
                table: "portal_audit_logs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_portal_general_master",
                table: "portal_general_master",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_sponsors",
                table: "portal_sponsors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_sms_logs",
                table: "portal_sms_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_requests",
                table: "portal_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_payments",
                table: "portal_payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_general_master",
                table: "portal_general_master");

            migrationBuilder.DropPrimaryKey(
                name: "PK_portal_audit_logs",
                table: "portal_audit_logs");

            migrationBuilder.RenameTable(
                name: "portal_sponsors",
                newName: "Sponsors");

            migrationBuilder.RenameTable(
                name: "portal_sms_logs",
                newName: "SmsRequests");

            migrationBuilder.RenameTable(
                name: "portal_requests",
                newName: "OvmcRequests");

            migrationBuilder.RenameTable(
                name: "portal_payments",
                newName: "OvmcPayments");

            migrationBuilder.RenameTable(
                name: "portal_general_master",
                newName: "GeneralMasters");

            migrationBuilder.RenameTable(
                name: "portal_audit_logs",
                newName: "OvmcAuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_portal_payments_OrderNo",
                table: "OvmcPayments",
                newName: "IX_OvmcPayments_OrderNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sponsors",
                table: "Sponsors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SmsRequests",
                table: "SmsRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OvmcRequests",
                table: "OvmcRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OvmcPayments",
                table: "OvmcPayments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneralMasters",
                table: "GeneralMasters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OvmcAuditLogs",
                table: "OvmcAuditLogs",
                column: "Id");
        }
    }
}
