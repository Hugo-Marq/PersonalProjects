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
    public class OrcamentoRepository : IOrcamentoRepository<Orcamento>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public OrcamentoRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext)
        {
            _dbContext = quickRoomSolutionDatabaseContext;
        }

        public async Task<List<Orcamento>> GetAll()
        {
            return await _dbContext.Orcamentos.ToListAsync();
        }

        public async Task<List<Orcamento>> GetAllFiltrados()
        {
            return await _dbContext.Orcamentos.Where(entity => entity.OrcamentoEstado == OrcamentoEstados.Pendente).ToListAsync();
        }

        public async Task<Orcamento> GetById(int id)
        {
            return await _dbContext.Orcamentos.FirstOrDefaultAsync(entity => entity.OrcamentoId == id);
        }
            
        public async Task<Orcamento> Insert(Orcamento entity)
        {
            await _dbContext.Orcamentos.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Orcamento> Update(Orcamento entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            // Update entity properties here

            _dbContext.Orcamentos.Update(existingEntity);
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

            _dbContext.Orcamentos.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Obt�m o or�amento aceite associado a um determinado ID de ticket.
        /// </summary>
        /// <param name="ticketId">O ID do ticket.</param>
        /// <returns>
        /// O primeiro or�amento aceite encontrado correspondente ao ID do ticket especificado,
        /// ou null se nenhum or�amento aceite for encontrado.
        /// </returns>
        public async Task<Orcamento> AceitarOrcamento(int orcamentoId)
        {
            Orcamento orcamento = await GetById(orcamentoId);

            if (orcamento.OrcamentoEstado != OrcamentoEstados.Pendente)
            {
                throw new InvalidOperationException($"Or�amento com o id {orcamentoId} n�o est� pendente");
            }
            Ticket ticket = await _dbContext.Tickets.FirstOrDefaultAsync(entity => entity.TicketId == orcamento.TicketTicketId);

            if (ticket == null)
            { 
                throw new InvalidOperationException($"Ticket com o id {orcamento.TicketTicketId} n�o existe!"); 
            }
            
            if (ticket.TicketEstado == TicketEstados.Atribuido)
            {
                throw new InvalidOperationException($"Ticket com o id {orcamento.TicketTicketId} j� foi atribuido a um or�amento!");
            }
            
            if (ticket.TicketEstado != TicketEstados.Validado)
            {
                throw new InvalidOperationException($"Ticket n�o se encontra validado para manuten��o");
            }

            ticket.TicketEstado = TicketEstados.Atribuido;
            orcamento.OrcamentoEstado = OrcamentoEstados.Aceite;

            _dbContext.Orcamentos.Update(orcamento);
            _dbContext.Tickets.Update(ticket);

            await _dbContext.SaveChangesAsync();

            return orcamento;
        }

        public async Task<Orcamento> RejeitarOrcamento(int orcamentoId)
        {
            Orcamento orcamento = await GetById(orcamentoId);

            if (orcamento.OrcamentoEstado == OrcamentoEstados.Aceite)
            {
                throw new InvalidOperationException($"Or�amento com o id {orcamentoId} j� foi aceite, rever processo!");
            }
            if (orcamento.OrcamentoEstado == OrcamentoEstados.Rejeitado)
            {
                throw new InvalidOperationException($"Or�amento com o id {orcamentoId} j� foi rejeitado anteriomente!");
            }
            orcamento.OrcamentoEstado = OrcamentoEstados.Rejeitado;

            _dbContext.Orcamentos.Update(orcamento);

            await _dbContext.SaveChangesAsync();

            return orcamento;
        }

        public async Task NotificarFornecedorAceitacaoOrcamento(Orcamento entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Erro ao enviar email, sem informa��o!");
            }

            Fornecedor fornecedor = _dbContext.Fornecedores.FirstOrDefault(f => f.FornecedorId == entity.FornecedorFornecedorId);

            if (fornecedor == null)
            {
                throw new InvalidOperationException("Fornecedor n�o encontrado!");
            }
            Notificacoes.Notificacoes.EnviarLembreteConfirmacao("devtest_pikos@hotmail.com"
                    , $"Orcamento aceite!", $"O or�amento {entity.OrcamentoId} relativo ao ticket interno {entity.TicketTicketId} foi aceite!" +
                    $"Data de interven��o a combinar");
        }

        /// <summary>
        /// Obt�m o or�amento aceite associado a um determinado ID de ticket.
        /// </summary>
        /// <param name="ticketId">O ID do ticket.</param>
        /// <returns>
        /// O primeiro or�amento aceite encontrado correspondente ao ID do ticket especificado,
        /// ou null se nenhum or�amento aceite for encontrado.
        /// </returns>
        public async Task<Orcamento> GetOrcamentoAceiteByTicketId(int ticketId)
        {
            // Realiza uma consulta ass�ncrona para obter o primeiro or�amento aceite correspondente ao ID do ticket
            return await _dbContext.Orcamentos.FirstOrDefaultAsync(entity => entity.TicketTicketId == ticketId && entity.OrcamentoEstado == OrcamentoEstados.Aceite);
        }

        public async Task<bool> ExisteTicket(int TicketId)
        {
            return await _dbContext.Tickets.AnyAsync(entity => entity.TicketId == TicketId);
        }
    }
}
