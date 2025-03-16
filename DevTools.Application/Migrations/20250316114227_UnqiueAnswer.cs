using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class UnqiueAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamAnswer_TeamId",
                table: "TeamAnswer");

            migrationBuilder.AddColumn<int>(
                name: "AnswerTimeMilliseconds",
                table: "TeamAnswer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeamAnswer_TeamId_QuestionIndex_AnswerId",
                table: "TeamAnswer",
                columns: new[] { "TeamId", "QuestionIndex", "AnswerId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamAnswer_TeamId_QuestionIndex_AnswerId",
                table: "TeamAnswer");

            migrationBuilder.DropColumn(
                name: "AnswerTimeMilliseconds",
                table: "TeamAnswer");

            migrationBuilder.CreateIndex(
                name: "IX_TeamAnswer_TeamId",
                table: "TeamAnswer",
                column: "TeamId");
        }
    }
}
