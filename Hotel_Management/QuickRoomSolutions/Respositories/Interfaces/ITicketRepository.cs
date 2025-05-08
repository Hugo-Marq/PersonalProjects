using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface ITicketRepository<Ticket>
    {
        Task<List<Ticket>> GetAll();
        Task<Ticket> GetById(int id);
        Task<Ticket> Insert(Ticket entity);
        Task<Ticket> Update(Ticket entity, int id);
        Task<bool> DeleteById(int id);
        Task<Ticket> InicializarManutencao(int id, int funcFornecedorId);
        Task<Ticket> FinalizarManutencao(int ticketId);
        Task<Ticket> AprovarTicket(int id);
        Task<Ticket> RejeitarTicket(int id);
        Task<List<Ticket>> AllFiltrados(TicketEstados estado);
        Task<List<Ticket>> GetAllFiltradosFuncFornecedorEstadoIniciado(int funcFornecedorId);
        Task EnviarEmailFornecedoresComServico(Ticket ticket);
        Task<Ticket> AprovarManutencaoPrestada(int ticketId);
        Task<Ticket> PedirRevisaoManutencaoPrestada(int ticketId);
        Task EnviarEmailConclusaoServico(Ticket ticket);
        Task EnviarEmailRevisaoServico(Ticket ticket);

    }
}
