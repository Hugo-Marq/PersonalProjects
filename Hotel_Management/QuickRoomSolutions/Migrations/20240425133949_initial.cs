using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRoomSolutions.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cargo",
                columns: table => new
                {
                    CargoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescricaoCargo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cargo__B4E665CDEED8E657", x => x.CargoId);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedor",
                columns: table => new
                {
                    FornecedorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FornecedorNome = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FornecedorEmail = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FornecedorNIPC = table.Column<int>(type: "int", nullable: false),
                    FornecedorMorada = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Forneced__494B8C10371A8538", x => x.FornecedorId);
                });

            migrationBuilder.CreateTable(
                name: "Pessoa",
                columns: table => new
                {
                    NIF = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    DataNasc = table.Column<DateTime>(type: "datetime", nullable: false),
                    Morada = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CP = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ContactoTelefonico = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pessoa__C7DEC3317E9C9F41", x => x.NIF);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    ServicoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicoTipo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Servicos__C59767B6FC5CF0A1", x => x.ServicoId);
                });

            migrationBuilder.CreateTable(
                name: "Tipologia",
                columns: table => new
                {
                    TipologiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipologiaDescricao = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Preco = table.Column<float>(type: "real", nullable: false),
                    TipologiaImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tipologi__62F8D35E25BE44C9", x => x.TipologiaId);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PessoaNIF = table.Column<int>(type: "int", nullable: false),
                    ClientPassword = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Cliente__71ABD087D82EA67A", x => x.ClienteId);
                    table.ForeignKey(
                        name: "FKCliente785426",
                        column: x => x.PessoaNIF,
                        principalTable: "Pessoa",
                        principalColumn: "NIF");
                });

            migrationBuilder.CreateTable(
                name: "Funcionario",
                columns: table => new
                {
                    FuncionarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NIF = table.Column<int>(type: "int", nullable: false),
                    CargoCargoId = table.Column<int>(type: "int", nullable: false),
                    FuncionarioPassword = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Funciona__297ECCAA3C870B15", x => x.FuncionarioId);
                    table.ForeignKey(
                        name: "FKFuncionari583978",
                        column: x => x.NIF,
                        principalTable: "Pessoa",
                        principalColumn: "NIF");
                    table.ForeignKey(
                        name: "FKFuncionari942921",
                        column: x => x.CargoCargoId,
                        principalTable: "Cargo",
                        principalColumn: "CargoId");
                });

            migrationBuilder.CreateTable(
                name: "FuncionarioFornecedor",
                columns: table => new
                {
                    FuncFornecedorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PessoaNIF = table.Column<int>(type: "int", nullable: false),
                    FornecedorFornecedorId = table.Column<int>(type: "int", nullable: false),
                    FuncFornecedorPassword = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Funciona__DE2495F54369F4D4", x => x.FuncFornecedorId);
                    table.ForeignKey(
                        name: "FKFuncionari252582",
                        column: x => x.PessoaNIF,
                        principalTable: "Pessoa",
                        principalColumn: "NIF");
                    table.ForeignKey(
                        name: "FKFuncionari357614",
                        column: x => x.FornecedorFornecedorId,
                        principalTable: "Fornecedor",
                        principalColumn: "FornecedorId");
                });

            migrationBuilder.CreateTable(
                name: "ServicosFornecedor",
                columns: table => new
                {
                    FornecedorFornecedorId = table.Column<int>(type: "int", nullable: false),
                    ServicosServicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Servicos__87388612C1D67A14", x => new { x.FornecedorFornecedorId, x.ServicosServicoId });
                    table.ForeignKey(
                        name: "FKServicosFo3255",
                        column: x => x.ServicosServicoId,
                        principalTable: "Servicos",
                        principalColumn: "ServicoId");
                    table.ForeignKey(
                        name: "FKServicosFo466425",
                        column: x => x.FornecedorFornecedorId,
                        principalTable: "Fornecedor",
                        principalColumn: "FornecedorId");
                });

            migrationBuilder.CreateTable(
                name: "Quarto",
                columns: table => new
                {
                    QuartoId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Bloco = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Piso = table.Column<int>(type: "int", nullable: false),
                    Porta = table.Column<int>(type: "int", nullable: false),
                    QuartoEstado = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    TipologiaTipologiaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Quarto__903445333972E660", x => x.QuartoId);
                    table.ForeignKey(
                        name: "FKQuarto389558",
                        column: x => x.TipologiaTipologiaId,
                        principalTable: "Tipologia",
                        principalColumn: "TipologiaId");
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteClienteId = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime", nullable: false),
                    NumeroCartao = table.Column<long>(type: "bigint", nullable: false),
                    EstadoReserva = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CheckIn = table.Column<bool>(type: "bit", nullable: false),
                    DataCheckIn = table.Column<DateTime>(type: "datetime", nullable: true),
                    CheckOut = table.Column<bool>(type: "bit", nullable: false),
                    DataCheckOut = table.Column<DateTime>(type: "datetime", nullable: true),
                    QuartoQuartoId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reserva__C39937634EF16361", x => x.ReservaId);
                    table.ForeignKey(
                        name: "FKReserva817646",
                        column: x => x.ClienteClienteId,
                        principalTable: "Cliente",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FKReserva851797",
                        column: x => x.QuartoQuartoId,
                        principalTable: "Quarto",
                        principalColumn: "QuartoId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FuncionarioFuncionarioId = table.Column<int>(type: "int", nullable: false),
                    FuncionarioFornecedorFuncFornecedorId = table.Column<int>(type: "int", nullable: true),
                    QuartoQuartoId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    TicketDescricao = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    TicketDataAbertura = table.Column<DateTime>(type: "datetime", nullable: false),
                    TickectDataAtualizacao = table.Column<DateTime>(type: "datetime", nullable: true),
                    TicketEstado = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ticket__712CC6076586CA3E", x => x.TicketId);
                    table.ForeignKey(
                        name: "FKTicket380775",
                        column: x => x.FuncionarioFuncionarioId,
                        principalTable: "Funcionario",
                        principalColumn: "FuncionarioId");
                    table.ForeignKey(
                        name: "FKTicket996136",
                        column: x => x.QuartoQuartoId,
                        principalTable: "Quarto",
                        principalColumn: "QuartoId");
                    table.ForeignKey(
                        name: "FK_Ticket_FuncionarioFornecedor_FuncionarioFornecedorFuncFornecedorId",
                        column: x => x.FuncionarioFornecedorFuncFornecedorId,
                        principalTable: "FuncionarioFornecedor",
                        principalColumn: "FuncFornecedorId");
                });

            migrationBuilder.CreateTable(
                name: "TicketLimpeza",
                columns: table => new
                {
                    LimpezaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FuncionarioFuncionarioId = table.Column<int>(type: "int", nullable: false),
                    FuncionarioLimpezaId = table.Column<int>(type: "int", nullable: true),
                    QuartoQuartoId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    LimpezaDataCriacao = table.Column<DateTime>(type: "datetime", nullable: false),
                    LimpezaDataAtualizacao = table.Column<DateTime>(type: "datetime", nullable: true),
                    LimpezaEstado = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    LimpezaPrioridade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RegistoL__15039A162BB27612", x => x.LimpezaId);
                    table.ForeignKey(
                        name: "FKRegistoLim395939",
                        column: x => x.FuncionarioFuncionarioId,
                        principalTable: "Funcionario",
                        principalColumn: "FuncionarioId");
                    table.ForeignKey(
                        name: "FKRegistoLim780577",
                        column: x => x.QuartoQuartoId,
                        principalTable: "Quarto",
                        principalColumn: "QuartoId");
                    table.ForeignKey(
                        name: "FK_TicketLimpeza_Funcionario_FuncionarioLimpezaId",
                        column: x => x.FuncionarioLimpezaId,
                        principalTable: "Funcionario",
                        principalColumn: "FuncionarioId");
                });

            migrationBuilder.CreateTable(
                name: "Orcamento",
                columns: table => new
                {
                    OrcamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketTicketId = table.Column<int>(type: "int", nullable: false),
                    FornecedorFornecedorId = table.Column<int>(type: "int", nullable: false),
                    ValorOrcamento = table.Column<float>(type: "real", nullable: false),
                    DescricacaoOrcamento = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    OrcamentoFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    OrcamentoEstado = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orcament__4E96F7795B2C316B", x => x.OrcamentoId);
                    table.ForeignKey(
                        name: "FKOrcamento101725",
                        column: x => x.TicketTicketId,
                        principalTable: "Ticket",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "FKOrcamento670984",
                        column: x => x.FornecedorFornecedorId,
                        principalTable: "Fornecedor",
                        principalColumn: "FornecedorId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_PessoaNIF",
                table: "Cliente",
                column: "PessoaNIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionario_CargoCargoId",
                table: "Funcionario",
                column: "CargoCargoId");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionario_NIF",
                table: "Funcionario",
                column: "NIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuncionarioFornecedor_FornecedorFornecedorId",
                table: "FuncionarioFornecedor",
                column: "FornecedorFornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_FuncionarioFornecedor_PessoaNIF",
                table: "FuncionarioFornecedor",
                column: "PessoaNIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orcamento_FornecedorFornecedorId",
                table: "Orcamento",
                column: "FornecedorFornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamento_TicketTicketId",
                table: "Orcamento",
                column: "TicketTicketId");

            migrationBuilder.CreateIndex(
                name: "UQ__Pessoa__C7DEC33027126182",
                table: "Pessoa",
                column: "NIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quarto_TipologiaTipologiaId",
                table: "Quarto",
                column: "TipologiaTipologiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_ClienteClienteId",
                table: "Reserva",
                column: "ClienteClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_QuartoQuartoId",
                table: "Reserva",
                column: "QuartoQuartoId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosFornecedor_ServicosServicoId",
                table: "ServicosFornecedor",
                column: "ServicosServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FuncionarioFornecedorFuncFornecedorId",
                table: "Ticket",
                column: "FuncionarioFornecedorFuncFornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FuncionarioFuncionarioId",
                table: "Ticket",
                column: "FuncionarioFuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_QuartoQuartoId",
                table: "Ticket",
                column: "QuartoQuartoId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLimpeza_FuncionarioFuncionarioId",
                table: "TicketLimpeza",
                column: "FuncionarioFuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLimpeza_FuncionarioLimpezaId",
                table: "TicketLimpeza",
                column: "FuncionarioLimpezaId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLimpeza_QuartoQuartoId",
                table: "TicketLimpeza",
                column: "QuartoQuartoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orcamento");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "ServicosFornecedor");

            migrationBuilder.DropTable(
                name: "TicketLimpeza");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Funcionario");

            migrationBuilder.DropTable(
                name: "Quarto");

            migrationBuilder.DropTable(
                name: "FuncionarioFornecedor");

            migrationBuilder.DropTable(
                name: "Cargo");

            migrationBuilder.DropTable(
                name: "Tipologia");

            migrationBuilder.DropTable(
                name: "Pessoa");

            migrationBuilder.DropTable(
                name: "Fornecedor");
        }
    }
}
