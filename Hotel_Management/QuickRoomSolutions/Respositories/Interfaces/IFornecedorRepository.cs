using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Respositories.Interfaces
{
    public interface IFornecedorRepository<Fornecedor>
    {
        Task<List<Fornecedor>> GetAll();
        Task<Fornecedor> GetById(int id);
        Task<Fornecedor> Insert(Fornecedor entity);
        Task<Fornecedor> Update(Fornecedor entity, int id);
        Task<bool> DeleteById(int id);

        Task<bool> AddServicosToFornecedor(ServicosFornecedorDTO servicosFornecedorDTO);

        Task<bool> UpdateServicosFornecedor(ServicosFornecedorDTO servicosFornecedorDTO);

        Task<List<Fornecedor>> GetFornecedoresAtivosPorServico(int servicoId);

    }
}
