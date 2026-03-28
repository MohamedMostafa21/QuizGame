using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizGame.Migrations
{
    /// <inheritdoc />
    public partial class AddinguniqueconstraintonGameIdandUserIdonGamePlayermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_UserId",
                table: "GamePlayers",
                columns: new[] { "GameId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_UserId",
                table: "GamePlayers");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers",
                column: "GameId");
        }
    }
}
