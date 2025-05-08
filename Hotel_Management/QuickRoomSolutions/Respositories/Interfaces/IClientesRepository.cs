using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IClientesRepository<Cliente> 
    {

        Task<List<Cliente>> GetAll();
        Task<Cliente> GetbyId(int id);
        Task<Cliente> Insert(Cliente entity);
        Task<Cliente> Update(Cliente entity, int id);
        Task<bool> DeleteById(int id);
        Task<Cliente> AssociarPessoaAoRegisto(Cliente entity);


    }
}
