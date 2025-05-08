using System;
using System.Collections.Generic;

namespace QuickRoomSolutions.Models;

public partial class Fornecedor
{
    public int FornecedorId { get; set; }

    public string FornecedorNome { get; set; } = null!;

    public string FornecedorEmail { get; set; } = null!;

    public int FornecedorNipc { get; set; }

    public string FornecedorMorada { get; set; } = null!;

    public bool FornecedorAtivo { get; set; }

    public virtual ICollection<FuncionarioFornecedor> FuncionarioFornecedors { get; set; } = new List<FuncionarioFornecedor>();

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();

    public virtual ICollection<Servico> ServicosServicos { get; set; } = new List<Servico>();
}
