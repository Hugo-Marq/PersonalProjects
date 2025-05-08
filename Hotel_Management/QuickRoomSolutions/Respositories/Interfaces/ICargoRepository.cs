using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface ICargoRepository<Cargo>
    {
        Task<List<Cargo>> GetAll();
        Task<Cargo> GetById(int id);
        Task<Cargo> Insert(Cargo entity);
        Task<Cargo> Update(Cargo entity, int id);
        Task<bool> DeleteById(int id);
    }
}
