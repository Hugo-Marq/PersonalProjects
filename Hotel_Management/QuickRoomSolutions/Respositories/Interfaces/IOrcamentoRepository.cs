using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IOrcamentoRepository<Orcamento>
    {
        Task<List<Orcamento>> GetAll();
        Task<Orcamento> GetById(int id);
        Task<Orcamento> Insert(Orcamento entity);
        Task<Orcamento> Update(Orcamento entity, int id);
        Task<bool> DeleteById(int id);
        Task<Orcamento> GetOrcamentoAceiteByTicketId(int TicketId);
        Task<Orcamento> AceitarOrcamento(int orcamentoId);
        Task<Orcamento> RejeitarOrcamento(int orcamentoId);
        Task<List<Orcamento>> GetAllFiltrados();
        Task NotificarFornecedorAceitacaoOrcamento(Orcamento entity);
        Task<bool> ExisteTicket(int TicketId);
    }
}
