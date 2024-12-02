using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cyb_lab.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LicenseActivated",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.DropColumn(
                name: "LicenseActivated",
                table: "AspNetUsers");
        }
    }
}
