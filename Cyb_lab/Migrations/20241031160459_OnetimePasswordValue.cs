using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cyb_lab.Migrations
{
    /// <inheritdoc />
    public partial class OnetimePasswordValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OnetimePasswordValue",
                table: "AspNetUsers",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnetimePasswordValue",
                table: "AspNetUsers");
        }
    }
}
