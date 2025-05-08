using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public partial class Funcionario
{
    public int FuncionarioId { get; set; }

    public int Nif { get; set; }

    public int CargoCargoId { get; set; }

    public string FuncionarioPassword { get; set; } = null!;

    public bool IsActive { get; set; }

    [JsonIgnore]
    public virtual Cargo? CargoCargo { get; set; } = null!;
    [JsonIgnore]
    public virtual Pessoa? PessoaPessoa { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<TicketLimpeza>? TicketsLimpezaRececionista { get; set; } = new List<TicketLimpeza>();
    [JsonIgnore]
    public virtual ICollection<TicketLimpeza>? TicketsLimpezaAuxLimpeza { get; set; } = new List<TicketLimpeza>();
    [JsonIgnore]
    public virtual ICollection<Ticket>? Tickets { get; set; } = new List<Ticket>();
}
