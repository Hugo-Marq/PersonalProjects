using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface ITipologiasRepository<Tipologia>
    {
       Task<Tipologia> GetbyQuartoID(string quartoId);
        Task<List<Tipologia>> GetAll();
        Task<Tipologia> GetById(int id);
        Task<Tipologia> Insert(Tipologia entity);
        Task<Tipologia> Update(Tipologia entity, int id);
        Task<bool> DeleteById(int id);
    }
}
