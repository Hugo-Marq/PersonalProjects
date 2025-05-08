using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IPessoasRepository<Pessoa> 
    {
        Task<List<Pessoa>> GetAll();
        Task<Pessoa> GetbyId(int id);
        Task<Pessoa> GetbyEmail(string email);
        Task<Pessoa> Insert(Pessoa entity);
        Task<Pessoa> Update(Pessoa entity, int id);
        Task<bool> DeleteById(int id);
    }
}
