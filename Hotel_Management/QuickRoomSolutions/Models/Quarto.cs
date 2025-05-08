using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuickRoomSolutions.Models;

public enum QuartoEstados
{
    Disponivel = 1,
    Manutencao = 2,
    Ocupado = 3,
    Indisponivel = 4,
    Limpeza = 5,
}


public partial class Quarto
{
    public string QuartoId { get; set; } = null!;

    public string Bloco { get; set; } = null!;

    public int Piso { get; set; }

    public int Porta { get; set; }

    public QuartoEstados QuartoEstado { get; set; }

    public int TipologiaTipologiaId { get; set; }
    [JsonIgnore]
    public virtual ICollection<TicketLimpeza> TicketsLimpeza { get; set; } = new List<TicketLimpeza>();
    [JsonIgnore]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    [JsonIgnore]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    [JsonIgnore]
    public virtual Tipologia? TipologiaTipologia { get; set; } = null!;





    public bool AlterarEstado(QuartoEstados estadoQuarto)
    {
        switch (estadoQuarto)
        {
            case QuartoEstados.Disponivel:
                QuartoEstado = QuartoEstados.Disponivel;
                break;
            case QuartoEstados.Manutencao:
                QuartoEstado = QuartoEstados.Manutencao;
                break;
            case QuartoEstados.Ocupado:
                QuartoEstado = QuartoEstados.Ocupado;
                break;
            case QuartoEstados.Indisponivel:
                QuartoEstado = QuartoEstados.Indisponivel;
                break;
            case QuartoEstados.Limpeza:
                QuartoEstado = QuartoEstados.Limpeza;
                break;
            default:
                return false;
                
        }

        return true;
    }
}
