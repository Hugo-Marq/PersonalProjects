
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;


namespace QuickRoomSolutions.Respositories
{
    public class PessoasRepository : IPessoasRepository<Pessoa>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;


        public PessoasRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext, IFuncionarioRepository<Funcionario> funcionarioRepository) 
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }

 
        public async Task<List<Pessoa>> GetAll()
        {
            return await _dbContext.Pessoas.ToListAsync();
        }

        public async Task<Pessoa> GetbyId(int id)
        {
            return await _dbContext.Pessoas.FirstOrDefaultAsync(pessoa => pessoa.Nif == id);
            
        }

        public async Task<Pessoa> GetbyEmail(string email)
        {
            return await _dbContext.Pessoas.FirstOrDefaultAsync(pessoa => pessoa.Email == email);
        }

        public async Task<Pessoa> Insert(Pessoa entity)
        {
            
            await _dbContext.Pessoas.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Pessoa> Update(Pessoa entity, int id)
        {
            Pessoa pessoa = await GetbyId(id);

            if (pessoa == null)
            {
                return null;
            }

            pessoa.Nome = entity.Nome;
            pessoa.DataNasc = entity.DataNasc;
            pessoa.Morada = entity.Morada;
            pessoa.Cp = entity.Cp;
            pessoa.ContactoTelefonico = entity.ContactoTelefonico;
            pessoa.Email=entity.Email;

            _dbContext.Pessoas.Update(pessoa);
           await  _dbContext.SaveChangesAsync();

            return pessoa;


        }



        public async Task<bool> DeleteById(int id)
        {
            Pessoa pessoa = await GetbyId(id);

            if (pessoa == null)
            {
                return false;
            }

            _dbContext.Pessoas.Remove(pessoa);
           await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
