using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class JokerQuestionIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "Joker");

            migrationBuilder.AddColumn<int>(
                name: "QuestionIndex",
                table: "Joker",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionIndex",
                table: "Joker");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "Joker",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
