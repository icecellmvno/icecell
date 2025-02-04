using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IceSMS.API.Migrations
{
    /// <inheritdoc />
    public partial class Tenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Tenants");
        }
    }
}
