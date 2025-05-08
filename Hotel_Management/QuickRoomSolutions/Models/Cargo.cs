using System;
using System.Collections.Generic;

namespace QuickRoomSolutions.Models;

public partial class Cargo
{
    public int CargoId { get; set; }

    public string DescricaoCargo { get; set; } = null!;

    public virtual ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
}
