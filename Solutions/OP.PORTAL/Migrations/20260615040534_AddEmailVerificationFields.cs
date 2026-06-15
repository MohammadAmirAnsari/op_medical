using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OP.PORTAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailTokenExpiry",
                table: "portal_sponsors",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "portal_sponsors",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "portal_sponsors",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTokenExpiry",
                table: "portal_sponsors");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "portal_sponsors");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "portal_sponsors");
        }
    }
}
