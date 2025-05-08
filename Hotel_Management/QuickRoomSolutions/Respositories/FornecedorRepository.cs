using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace QuickRoomSolutions.Repositories
{
    public class FornecedorRepository : IFornecedorRepository<Fornecedor>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public FornecedorRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext) 
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }

        public async Task<List<Fornecedor>> GetAll()
        {
            return await _dbContext.Fornecedores.ToListAsync();
        }

        public async Task<Fornecedor> GetById(int id)
        {
            return await _dbContext.Fornecedores.FirstOrDefaultAsync(entity => entity.FornecedorId == id);
        }

        public async Task<Fornecedor> Insert(Fornecedor entity)
        {
            await _dbContext.Fornecedores.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Fornecedor> Update(Fornecedor entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            // Update scalar properties
            existingEntity.FornecedorNome = entity.FornecedorNome;
            existingEntity.FornecedorEmail = entity.FornecedorEmail;
            existingEntity.FornecedorNipc = entity.FornecedorNipc;
            existingEntity.FornecedorMorada = entity.FornecedorMorada;
            existingEntity.FornecedorAtivo = entity.FornecedorAtivo;

            // Update FuncionarioFornecedors
            existingEntity.FuncionarioFornecedors.Clear();
            foreach (var funcionarioFornecedor in entity.FuncionarioFornecedors)
            {
                // Assuming FuncionarioFornecedor is a new entity or an existing one
                if (funcionarioFornecedor.FuncFornecedorId == 0)
                {
                    existingEntity.FuncionarioFornecedors.Add(funcionarioFornecedor);
                    _dbContext.Entry(funcionarioFornecedor).State = EntityState.Added;
                }
                else
                {
                    existingEntity.FuncionarioFornecedors.Add(funcionarioFornecedor);
                    _dbContext.Entry(funcionarioFornecedor).State = EntityState.Modified;
                }
            }

            // Update Orcamentos
            existingEntity.Orcamentos.Clear();
            foreach (var orcamento in entity.Orcamentos)
            {
                if (orcamento.OrcamentoId == 0)
                {
                    existingEntity.Orcamentos.Add(orcamento);
                    _dbContext.Entry(orcamento).State = EntityState.Added;
                }
                else
                {
                    existingEntity.Orcamentos.Add(orcamento);
                    _dbContext.Entry(orcamento).State = EntityState.Modified;
                }
            }

            // Update ServicosServicos (Many-to-Many Relationship Handling)
            existingEntity.ServicosServicos.Clear();
            foreach (var servico in entity.ServicosServicos)
            {
                // Attach the existing or new Servico entity
                var existingServico = await _dbContext.Servicos.FindAsync(servico.ServicoId);
                if (existingServico == null)
                {
                    existingEntity.ServicosServicos.Add(servico);
                    _dbContext.Entry(servico).State = EntityState.Added;
                }
                else
                {
                    existingEntity.ServicosServicos.Add(existingServico);
                }
            }

            // Update the main entity
            _dbContext.Fornecedores.Update(existingEntity);
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

            _dbContext.Fornecedores.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Adiciona servi�os a um fornecedor na base de dadis.
        /// </summary>
        /// <param name="servicosFornecedorDTO">DTO contendo o ID do fornecedor e os IDs dos servi�os a serem adicionados.</param>
        /// <returns>Um booleano indicando se os servi�os foram adicionados com sucesso.</returns>
        /// <exception cref="InvalidOperationException">Exce��o lan�ada quando o ID do fornecedor n�o � v�lido ou os IDs dos servi�os n�o s�o v�lidos.</exception>
        public async Task<bool> AddServicosToFornecedor(ServicosFornecedorDTO servicosFornecedorDTO)
        {
            // Procura de fornecedor na base de dados incluindo os servi�os associados
            Fornecedor fornecedor = await _dbContext.Fornecedores
                .Include(f => f.ServicosServicos)
                .FirstOrDefaultAsync(f => f.FornecedorId == servicosFornecedorDTO.FornecedorId);

            // Lan�a uma exce��o se o fornecedor n�o for encontrado
            if (fornecedor == null)
            {
                throw new InvalidOperationException($"O ID do fornecedor n�o � v�lido: {servicosFornecedorDTO.FornecedorId}");
            }

            // Procura os servi�os com base nos IDs fornecidos
            List<Servico> servicos = await _dbContext.Servicos
                .Where(servico => servicosFornecedorDTO.ServicoIDs.Contains(servico.ServicoId))
                .ToListAsync();

            // Lan�a uma exce��o se nenhum servi�o v�lido for encontrado
            if (servicos.Count == 0)
            {
                throw new InvalidOperationException($"Nenhum servi�o v�lido encontrado com os IDs fornecidos.");
            }

            // Adiciona cada servi�o ao fornecedor, se ainda n�o estiver associado
            foreach (var servicoId in servicosFornecedorDTO.ServicoIDs)
            {
                var servico = _dbContext.Servicos.Find(servicoId);

                // Lan�a uma exce��o se o servi�o n�o for encontrado
                if (servico == null)
                {
                    throw new InvalidOperationException($"Servi�o com o ID {servicoId} n�o encontrado.");
                }

                // Verifica se o servi�o j� est� associado ao fornecedor
                if (!fornecedor.ServicosServicos.Any(s => s.ServicoId == servicoId))
                {
                    fornecedor.ServicosServicos.Add(servico);
                }
            }
            _dbContext.Fornecedores.Update(fornecedor);

            // Salva as altera��es na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna true indicando que os servi�os foram adicionados com sucesso
            return true;
        }


        /// <summary>
        /// Atualiza os servi�os associados a um fornecedor na base de dados.
        /// </summary>
        /// <param name="servicosFornecedorDTO">DTO contendo o ID do fornecedor e os IDs dos servi�os a serem atualizados.</param>
        /// <returns>Um booleano indicando se os servi�os foram atualizados com sucesso.</returns>
        /// <exception cref="InvalidOperationException">Exce��o lan�ada quando o ID do fornecedor n�o � v�lido ou os IDs dos servi�os n�o s�o v�lidos.</exception>
        public async Task<bool> UpdateServicosFornecedor(ServicosFornecedorDTO servicosFornecedorDTO)
        {
            // Procura o fornecedor no banco de dados incluindo os servi�os associados
            Fornecedor fornecedor = await _dbContext.Fornecedores
                .Include(f => f.ServicosServicos)
                .FirstOrDefaultAsync(f => f.FornecedorId == servicosFornecedorDTO.FornecedorId);

            // Lan�a uma exce��o se o fornecedor n�o for encontrado
            if (fornecedor == null)
            {
                throw new InvalidOperationException($"O ID do fornecedor n�o � v�lido: {servicosFornecedorDTO.FornecedorId}");
            }

            // Remove todos os servi�os associados ao fornecedor
            fornecedor.ServicosServicos.Clear();

            // Verifica se h� servi�os para adicionar
            if (servicosFornecedorDTO.ServicoIDs.Count != 0)
            {
                // Procura os servi�os com base nos IDs fornecidos
                List<Servico> servicos = await _dbContext.Servicos
                    .Where(servico => servicosFornecedorDTO.ServicoIDs.Contains(servico.ServicoId))
                    .ToListAsync();

                // Lan�a uma exce��o se nenhum servi�o v�lido for encontrado
                if (servicos.Count == 0)
                {
                    throw new InvalidOperationException($"Nenhum servi�o v�lido encontrado com os IDs fornecidos.");
                }

                // Adiciona os servi�os ao fornecedor
                foreach (var servico in servicos)
                {
                    fornecedor.ServicosServicos.Add(servico);
                }
            }

            // Salva as altera��es na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna true indicando que os servi�os foram atualizados com sucesso
            return true;
        }

        public async Task<List<Fornecedor>> GetFornecedoresAtivosPorServico(int servicoId)
        {
            return await _dbContext.Fornecedores
                .Where(f => f.FornecedorAtivo)
                .Where(f => f.ServicosServicos.Any(ss => ss.ServicoId == servicoId))
                .ToListAsync();
        }

    }
}
