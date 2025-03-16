using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class TeamJoker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Joker");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Question",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamJoker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JokerType = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    QuestionIndex = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamJoker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamJoker_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Team_Name_QuizShowId",
                table: "Team",
                columns: new[] { "Name", "QuizShowId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_TeamId_QuizShowId",
                table: "Team",
                columns: new[] { "TeamId", "QuizShowId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoker_TeamId",
                table: "TeamJoker",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamJoker");

            migrationBuilder.DropIndex(
                name: "IX_Team_Name_QuizShowId",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_TeamId_QuizShowId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Question");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Joker",
                type: "datetime2",
                nullable: true);
        }
    }
}
