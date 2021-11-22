using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class MakeFlagNameUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Flags_Name",
                table: "Flags",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flags_Name",
                table: "Flags");
        }
    }
}
