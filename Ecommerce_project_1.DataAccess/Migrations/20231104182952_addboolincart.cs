using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_project_1.DataAccess.Migrations
{
    public partial class addboolincart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isChecked",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isChecked",
                table: "Carts");
        }
    }
}
