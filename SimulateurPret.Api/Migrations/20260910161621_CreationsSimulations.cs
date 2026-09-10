using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SimulateurPret.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreationsSimulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Simulations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Montant = table.Column<decimal>(type: "numeric", nullable: false),
                    TauxAnnuel = table.Column<decimal>(type: "numeric", nullable: false),
                    DureeMois = table.Column<int>(type: "integer", nullable: false),
                    Mensualite = table.Column<decimal>(type: "numeric", nullable: false),
                    CoutTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    InteretsTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Simulations");
        }
    }
}
