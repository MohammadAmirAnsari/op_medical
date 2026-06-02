using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class auditableclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "Sponsors",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "Sponsors",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedTime",
                table: "SmsRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "OvmcRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "OvmcPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SessionUser",
                table: "OvmcAuditLogs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "UsedTime",
                table: "SmsRequests");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "OvmcRequests");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "OvmcPayments");

            migrationBuilder.DropColumn(
                name: "SessionUser",
                table: "OvmcAuditLogs");
        }
    }
}
