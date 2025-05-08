using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRoomSolutions.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarServicoID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicoId",
                table: "Ticket",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TipologiaId",
                table: "Reserva",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ServicoId",
                table: "Ticket",
                column: "ServicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Servicos_ServicoId",
                table: "Ticket",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "ServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Servicos_ServicoId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_ServicoId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "ServicoId",
                table: "Ticket");

            migrationBuilder.AlterColumn<int>(
                name: "TipologiaId",
                table: "Reserva",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
