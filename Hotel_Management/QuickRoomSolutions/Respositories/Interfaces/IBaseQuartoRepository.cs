using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IBaseQuartoRepository<Quarto>
    {
        Task<List<Quarto>> GetAll();
        Task<Quarto> GetbyId(string id);
        Task<Quarto> Insert(Quarto quarto);
        Task<Quarto> Update(Quarto quarto, string id);
        Task<bool> DeleteById(string id);
        Task<bool> ExisteQuartoId(string id);
        Task<string> GerarQuartoId(string bloco, int piso, int numero);
        Task<Quarto> AtualizarEstadoQuarto(string quartoId, int estado);
        Task UpdateQuartoAsync(Quarto quarto);
        string GetQuartoIdByTipologiaId(int id);
        int GetTipologiaIdByQuartoId(string id);
        bool DisponibilidadeQuarto(string id, DateTime inicio, DateTime fim);
        Task<List<Quarto>> QuartosLivresNoPeriodoReserva(int reservaId);
        Task<bool> CanBook(int id, DateTime inicio, DateTime fim);
    }
}
