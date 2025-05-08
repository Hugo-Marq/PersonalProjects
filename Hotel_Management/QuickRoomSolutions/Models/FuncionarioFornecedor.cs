using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public partial class FuncionarioFornecedor
{
    public int FuncFornecedorId { get; set; }

    public int PessoaNif { get; set; }

    public int FornecedorFornecedorId { get; set; }

    public string FuncFornecedorPassword { get; set; } = null!;

    public bool IsActive { get; set; }

    [JsonIgnore]
    public virtual Fornecedor? FornecedorFornecedor { get; set; } = null!;
    [JsonIgnore]
    public virtual Pessoa? PessoaPessoa { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Ticket>? Tickets { get; set; } = new List<Ticket>();
}
