using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Repositories
{
    public class CargoRepository : ICargoRepository<Cargo>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public CargoRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext) 
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }

        public async Task<List<Cargo>> GetAll()
        {
            return await _dbContext.Cargos.ToListAsync();
        }

        public async Task<Cargo> GetById(int id)
        {
            return await _dbContext.Cargos.FirstOrDefaultAsync(entity => entity.CargoId == id);
        }

        public async Task<Cargo> Insert(Cargo entity)
        {
            await _dbContext.Cargos.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Cargo> Update(Cargo entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            // Update entity properties here

            _dbContext.Cargos.Update(existingEntity);
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

            _dbContext.Cargos.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
