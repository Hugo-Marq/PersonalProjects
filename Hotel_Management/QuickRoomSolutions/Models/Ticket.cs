using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public enum TicketEstados
{
    Pendente = 1,
    Atribuido = 2,
    ParaRevisao = 3,
    Iniciado = 4,
    Finalizado = 5,
    Cancelado = 6,
    Rejeitado = 7,
    Concluido = 8,
    Validado = 9
}


public partial class Ticket
{
    public int TicketId { get; set; }

    public int FuncionarioFuncionarioId { get; set; }

    public int? FuncionarioFornecedorFuncFornecedorId { get; set; }

    public string QuartoQuartoId { get; set; } = null!;

    public string? TicketDescricao { get; set; }

    public DateTime TicketDataAbertura { get; set; }

    public DateTime? TickectDataAtualizacao { get; set; }

    public TicketEstados TicketEstado { get; set; }

    public int ServicoId { get; set; }

    [JsonIgnore]
    public virtual Funcionario? FuncionarioFuncionario { get; set; } = null!;

    [JsonIgnore]
    public virtual FuncionarioFornecedor? FuncionarioFornecedor { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Orcamento>? Orcamentos { get; set; } = new List<Orcamento>();

    [JsonIgnore]
    public virtual Quarto? QuartoQuarto { get; set; } = null!;

    [JsonIgnore]
    public virtual Servico? ServicoServico { get; set; } = null!;

}
