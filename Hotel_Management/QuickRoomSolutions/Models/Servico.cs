using System;
using System.Collections.Generic;

namespace QuickRoomSolutions.Models;

public partial class Servico
{
    public int ServicoId { get; set; }

    public string ServicoTipo { get; set; } = null!;

    public virtual ICollection<Fornecedor> FornecedorFornecedors { get; set; } = new List<Fornecedor>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
