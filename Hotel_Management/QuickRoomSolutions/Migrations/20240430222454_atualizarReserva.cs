using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRoomSolutions.Migrations
{
    /// <inheritdoc />
    public partial class atualizarReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "Reserva",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_TipologiaId",
                table: "Reserva",
                column: "TipologiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reserva_Tipologia_TipologiaId",
                table: "Reserva",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "TipologiaId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reserva_Tipologia_TipologiaId",
                table: "Reserva");

            migrationBuilder.DropIndex(
                name: "IX_Reserva_TipologiaId",
                table: "Reserva");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "Reserva");
        }
    }
}
