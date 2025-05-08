using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRoomSolutions.Migrations
{
    /// <inheritdoc />
    public partial class estadoFornecedor05052024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FornecedorAtivo",
                table: "Fornecedor",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FornecedorAtivo",
                table: "Fornecedor");
        }
    }
}
