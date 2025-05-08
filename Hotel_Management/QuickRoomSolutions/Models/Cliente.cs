using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public int PessoaNif { get; set; }

    public string ClientPassword { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Pessoa? PessoaPessoa{ get; set; } = null!;

    public virtual ICollection<Reserva>? Reservas { get; set; } = new List<Reserva>();
}
