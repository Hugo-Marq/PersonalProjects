using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.DTOs;

public partial class QuartoDTO
{
    public string QuartoId { get; set; } = null!;

    public string Bloco { get; set; } = null!;

    public int Piso { get; set; }

    public int Porta { get; set; }

    public int TipologiaTipologiaId { get; set; }

    public int QuartoEstadoQuartoEstadoId { get; set; }
}