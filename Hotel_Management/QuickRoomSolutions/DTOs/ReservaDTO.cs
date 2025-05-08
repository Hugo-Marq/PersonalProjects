using QuickRoomSolutions.Models;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace QuickRoomSolutions.DTOs
{
    public class ReservaDTO
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

        [Required(ErrorMessage = "TipologiaId is required.")]
        [Range(1, 3, ErrorMessage = "TipologiaId must be greater than 0.")]
        public int TipologiaId { get; set; }
        public float PrecoTotal { get; set; }
        public string? QuartoId { get; set; }


    }
}
