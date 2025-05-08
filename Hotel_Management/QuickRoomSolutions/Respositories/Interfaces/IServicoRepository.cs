using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IServicoRepository<Servico>
    {
        Task<List<Servico>> GetAll();
        Task<Servico> GetById(int id);
        Task<Servico> Insert(Servico entity);
        Task<Servico> Update(Servico entity, int id);
        Task<bool> DeleteById(int id);
    }
}
