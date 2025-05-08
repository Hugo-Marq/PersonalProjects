using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IFuncionarioRepository<Funcionario>
    {
        Task<List<Funcionario>> GetAll();
        Task<Funcionario> GetById(int id);
        Task<Funcionario> Insert(Funcionario entity);
        Task<Funcionario> Update(Funcionario entity, int id);
        Task<bool> DeleteById(int id);
        Task<Funcionario> AssociarPessoaAoRegisto(Funcionario entity);
    }
}
