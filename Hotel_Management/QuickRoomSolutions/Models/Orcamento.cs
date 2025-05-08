using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public enum OrcamentoEstados
{
    Pendente = 1,
    Aceite = 2,
    Rejeitado = 3,
}


public partial class Orcamento
{
    public int OrcamentoId { get; set; }

    public int TicketTicketId { get; set; }

    public int FornecedorFornecedorId { get; set; }

    public float ValorOrcamento { get; set; }

    public string DescricacaoOrcamento { get; set; } = null!;

    [JsonIgnore]
    public byte[]? OrcamentoFile { get; set; } = null!;

    public OrcamentoEstados OrcamentoEstado { get; set; }

    [JsonIgnore]
    public virtual Fornecedor? FornecedorFornecedor { get; set; } = null!;
    [JsonIgnore]
    public virtual Ticket? TicketTicket { get; set; } = null!;
}
