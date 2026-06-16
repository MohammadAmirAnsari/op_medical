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

            migrationBuilder.Sql("ALTER TABLE `portal_sponsors` MODIFY COLUMN `EmailTokenExpiry` DATETIME(6) AFTER `IsVerified`;");
            migrationBuilder.Sql("ALTER TABLE `portal_sponsors` MODIFY COLUMN `EmailVerificationToken` LONGTEXT AFTER `EmailTokenExpiry`;");
            migrationBuilder.Sql("ALTER TABLE `portal_sponsors` MODIFY COLUMN `IsEmailVerified` BIT(1) AFTER `EmailVerificationToken`;");
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
