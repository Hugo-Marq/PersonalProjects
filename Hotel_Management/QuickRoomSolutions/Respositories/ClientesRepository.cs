using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;



namespace QuickRoomSolutions.Respositories
{
    public class ClientesRepository : IClientesRepository<Cliente>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;


        public ClientesRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext)
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }


        public async Task<List<Cliente>> GetAll()
        {
            return await _dbContext.Clientes.ToListAsync();
        }

        public async Task<Cliente> GetbyId(int id)
        {
            return await _dbContext.Clientes.FirstOrDefaultAsync(cliente => cliente.ClienteId == id);

        }

        public async Task<Cliente> Insert(Cliente entity)
        {

            await _dbContext.Clientes.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Cliente> Update(Cliente entity, int id)
        {
            Cliente cliente = await GetbyId(id);

            if (cliente == null)
            {
                return null;
            }


            cliente.ClientPassword = entity.ClientPassword;
            cliente.PessoaNif = entity.PessoaNif;
            cliente.IsActive = entity.IsActive;
            cliente.PessoaPessoa = entity.PessoaPessoa;

            _dbContext.Clientes.Update(cliente);
            await _dbContext.SaveChangesAsync();

            return cliente;


        }



        public async Task<bool> DeleteById(int id)
        {
            Cliente cliente = await GetbyId(id);

            if (cliente == null)
            {
                return false;
            }

            _dbContext.Clientes.Remove(cliente);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Cliente> AssociarPessoaAoRegisto(Cliente entity)
        {
            entity.PessoaPessoa = await _dbContext.Pessoas.FirstOrDefaultAsync(pessoa => pessoa.Nif == entity.PessoaNif);

            return entity;
        }
    }
}
