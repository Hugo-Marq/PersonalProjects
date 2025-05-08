using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface ITicketLimpezaRepository<TicketLimpeza>
    {
        Task<List<TicketLimpeza>> GetAll();
        Task<TicketLimpeza> GetById(int id);
        Task<TicketLimpeza> Insert(TicketLimpeza entity, LimpezaPrioridade limpezaPrioridade);
        Task<TicketLimpeza> Update(TicketLimpeza entity, int id);
        Task<bool> DeleteById(int id);
        Task<List<TicketLimpeza>> GetByPriorityOrder();
        Task<TicketLimpeza> InicializarLimpeza(int id, int auxLimpezaId);
        Task<TicketLimpeza> FinalizarLimpeza(int id, LimpezaEstados estadoFinalizacao);

        Task<List<TicketLimpeza>> GetTicketsActivosPorAuxLimpeza(int auxLimpezaid);

        Task<TicketLimpeza> CriarTicketLimpezaParaCheckout(string quartoID);

    }
}
