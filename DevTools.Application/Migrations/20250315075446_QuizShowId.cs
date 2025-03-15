using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class QuizShowId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizShowId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizShowId",
                table: "Questions",
                column: "QuizShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuizShows_QuizShowId",
                table: "Questions",
                column: "QuizShowId",
                principalTable: "QuizShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuizShows_QuizShowId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuizShowId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuizShowId",
                table: "Questions");
        }
    }
}
