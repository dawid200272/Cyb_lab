using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cyb_lab.Migrations
{
    /// <inheritdoc />
    public partial class OnetimePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnetimePasswordEnabled",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnetimePasswordEnabled",
                table: "AspNetUsers");
        }
    }
}
