using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDS_PROJECT.Data.Migrations
{
    public partial class add_link : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "Products");
        }
    }
}
