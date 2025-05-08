using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public enum LimpezaEstados
{
    Pendente = 1,
    Iniciada = 2,
    Finalizada = 3,
    FinalizadaProblemas = 4,
}

public enum LimpezaPrioridade
{
    ReqCliente = 1,
    ChekInHoje = 2,
    ChekOut = 3,
}



public partial class TicketLimpeza
{
    public int LimpezaId { get; set; }

    public int? FuncionarioFuncionarioId { get; set; } = null!;

    public int? FuncionarioLimpezaId { get; set; }

    public string QuartoQuartoId { get; set; } = null!;

    public DateTime LimpezaDataCriacao { get; set; }

    public DateTime? LimpezaDataAtualizacao { get; set; }

    public LimpezaEstados LimpezaEstado { get; set; }

    public LimpezaPrioridade LimpezaPrioridade { get; set; }

    [JsonIgnore]
    public virtual Funcionario? FuncionarioLimpeza { get; set; } = null!;
    [JsonIgnore]
    public virtual Funcionario? FuncionarioFuncionario { get; set; } = null!;
    [JsonIgnore]
    public virtual Quarto? QuartoQuarto { get; set; } = null!;
}
