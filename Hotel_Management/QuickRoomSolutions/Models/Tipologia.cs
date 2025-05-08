using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public partial class Tipologia
{
    public int TipologiaId { get; set; }

    public string TipologiaDescricao { get; set; } = null!;

    public float Preco { get; set; }

    public byte[]? TipologiaImage { get; set; }
    [JsonIgnore]
    public virtual ICollection<Quarto>? Quartos { get; set; } = new List<Quarto>();

    [JsonIgnore]
    public virtual ICollection<Reserva>? Reservas { get; set; } = new List<Reserva>();


}
