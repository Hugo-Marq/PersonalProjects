using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IReservasRepository<Reserva> 
    {

        Task<List<Reserva>> GetAll();

        Task<Reserva> GetbyId(int id);

        Task<Reserva> Insert(Reserva entity);

        Task<Reserva> Update(Reserva entity, int id);

        Task<bool> DeleteById(int id);

        Task<List<Reserva>> GetReservasQuartoId(string id);

        Task<Reserva> DoCheckout(int reservaId);

        Task<Reserva> CancelarReserva(int reservaId);

        Task<List<Quarto>> GetQuartosByTipologia(int id);

        Task<int> NumQuartosIndisponiveis(int id, DateTime inicio, DateTime fim);

        Task<bool> PodeReservar(ReservaDTO entityDTO);

        Task<Reserva> DoCheckIn(int reservaId);

        Reserva MapReserva(ReservaDTO reservaDTO);

        Task<float> CalcularPrecoReserva(Reserva reserva);

        Task<string> AtribuirQuartoAUmaReserva(ReservaDTO reservaDTO);
        
        Task<(Reserva,bool)> AtualizarQuartoDaReserva(Reserva reserva, Quarto quarto);

        Task<List<Reserva>> GetReservasAtuaisByTipologia(int id);

        Task<List<Reserva>> GetReservasWaitingCheckIn();

        Task<List<Reserva>> GetReservasWaitingCheckOut();
    }
}
