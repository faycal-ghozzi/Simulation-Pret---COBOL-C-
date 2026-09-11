using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimulateurPret.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjoutCleEtrangereUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Simulations_UserId",
                table: "Simulations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Simulations_AspNetUsers_UserId",
                table: "Simulations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Simulations_AspNetUsers_UserId",
                table: "Simulations");

            migrationBuilder.DropIndex(
                name: "IX_Simulations_UserId",
                table: "Simulations");
        }
    }
}
