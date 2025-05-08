using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IFuncionarioFornecedorRepository<FuncionarioFornecedor>
    {
        Task<List<FuncionarioFornecedor>> GetAll();
        Task<FuncionarioFornecedor> GetById(int id);
        Task<FuncionarioFornecedor> Insert(FuncionarioFornecedor entity);
        Task<FuncionarioFornecedor> Update(FuncionarioFornecedor entity, int id);
        Task<bool> DeleteById(int id);
    }
}
