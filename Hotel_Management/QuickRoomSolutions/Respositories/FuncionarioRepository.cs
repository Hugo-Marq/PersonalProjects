using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;


namespace QuickRoomSolutions.Repositories
{
    public class FuncionarioRepository : IFuncionarioRepository<Funcionario>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public FuncionarioRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext) 
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }

        public async Task<List<Funcionario>> GetAll()
        {
            return await _dbContext.Funcionarios.ToListAsync();
        }

        public async Task<Funcionario> GetById(int id)
        {
            return await _dbContext.Funcionarios.FirstOrDefaultAsync(entity => entity.FuncionarioId == id);
        }

        public async Task<Funcionario> Insert(Funcionario entity)
        {
            await _dbContext.Funcionarios.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Funcionario> Update(Funcionario entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            // Update entity properties here

            _dbContext.Funcionarios.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteById(int id)
        {
            var entity = await GetById(id);
            if (entity == null)
            {
                return false;
            }

            _dbContext.Funcionarios.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Funcionario> AssociarPessoaAoRegisto(Funcionario entity)
        {
            entity.PessoaPessoa = await _dbContext.Pessoas.FirstOrDefaultAsync(pessoa => pessoa.Nif == entity.Nif);

            return entity;
        }
    }
}
