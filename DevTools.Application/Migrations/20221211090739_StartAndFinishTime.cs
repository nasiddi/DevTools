using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DevTools.Application.Migrations
{
    public partial class StartAndFinishTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turns_PlayerId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_PlayerResults_PlayerId",
                table: "PlayerResults");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishTime",
                table: "Games",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Games",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Turns_PlayerId",
                table: "Turns",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerResults_PlayerId",
                table: "PlayerResults",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turns_PlayerId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_PlayerResults_PlayerId",
                table: "PlayerResults");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_PlayerId",
                table: "Turns",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerResults_PlayerId",
                table: "PlayerResults",
                column: "PlayerId",
                unique: true);
        }
    }
}
