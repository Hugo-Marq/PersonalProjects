
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
    public class TipologiasRepository : ITipologiasRepository<Tipologia>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;


        public TipologiasRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext) 
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }


        public async Task<Tipologia> GetbyQuartoID(string quartoID)
        {
            
            Quarto quarto = await _dbContext.Quartos
                .Include(q => q.TipologiaTipologia)
                .FirstOrDefaultAsync(q => q.QuartoId == quartoID);

            
            return quarto?.TipologiaTipologia;
        }

        public async Task<List<Tipologia>> GetAll()
        {
            return await _dbContext.Tipologia.ToListAsync();
        }

        public async Task<Tipologia> GetById(int id)
        {
            return await _dbContext.Tipologia.FirstOrDefaultAsync(entity => entity.TipologiaId == id);
        }

        public async Task<Tipologia> Insert(Tipologia entity)
        {
            await _dbContext.Tipologia.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Tipologia> Update(Tipologia entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            // Update entity properties here

            _dbContext.Tipologia.Update(existingEntity);
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

            _dbContext.Tipologia.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }











    }
}
