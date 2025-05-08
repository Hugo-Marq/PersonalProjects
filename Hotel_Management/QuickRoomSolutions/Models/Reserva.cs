using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;
public enum ReservaEstado
{
    Ativa = 1,
    Iniciada = 2,
    Cancelada = 3,
    Finalizada = 4,
}
public partial class Reserva
{
    public int ReservaId { get; set; }

    public int ClienteClienteId { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public long NumeroCartao { get; set; }

    public ReservaEstado EstadoReserva { get; set; }

    public bool CheckIn { get; set; }

    public DateTime? DataCheckIn { get; set; }

    public bool CheckOut { get; set; }

    public DateTime? DataCheckOut { get; set; }

    public string? QuartoQuartoId { get; set; }

    public int TipologiaId { get; set; }

    [JsonIgnore]
    public virtual Cliente ClienteCliente { get; set; } = null!;
    [JsonIgnore]
    public virtual Quarto QuartoQuarto { get; set; } = null!;

    [JsonIgnore]
    public virtual Tipologia TipologiaTipologia { get; set; } = null!;
}
