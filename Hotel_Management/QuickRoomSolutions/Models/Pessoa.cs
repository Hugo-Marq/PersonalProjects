using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public partial class Pessoa
{
    public int Nif { get; set; }

    public string Nome { get; set; } = null!;

    public DateTime DataNasc { get; set; }

    public string Morada { get; set; } = null!;

    public string Cp { get; set; } = null!;

    public int ContactoTelefonico { get; set; }

    public string Email { get; set; } = null!;

    [JsonIgnore]
    public virtual Cliente?  ClienteCliente { get; set; } = null!;
    [JsonIgnore]
    public virtual FuncionarioFornecedor? FuncionarioFornecedorFuncionarioFornecedor { get; set; } = null!;
    [JsonIgnore]
    public virtual Funcionario? FuncionarioFuncionario { get; set; } = null!;
}
