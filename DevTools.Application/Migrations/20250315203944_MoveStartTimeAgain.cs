using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class MoveStartTimeAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionStartTime",
                table: "Joker");

            migrationBuilder.AddColumn<DateTime>(
                name: "QuestionStartTime",
                table: "QuizShows",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionStartTime",
                table: "QuizShows");

            migrationBuilder.AddColumn<DateTime>(
                name: "QuestionStartTime",
                table: "Joker",
                type: "datetime2",
                nullable: true);
        }
    }
}
