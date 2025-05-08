using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Repositories
{
    public class TicketLimpezaRepository : ITicketLimpezaRepository<TicketLimpeza>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;
        private readonly IFuncionarioRepository<Funcionario> _funcionarioRepository;
        private readonly IReservasRepository<Reserva> _reservaRepository;
        private readonly IConfiguration _configuration;

        public TicketLimpezaRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext, IFuncionarioRepository<Funcionario> funcionarioRepository, IReservasRepository<Reserva> reservaRepository, IConfiguration configuration)
        {
            _dbContext = quickRoomSolutionDatabaseContext;
            _funcionarioRepository = funcionarioRepository;
            _reservaRepository = reservaRepository;
            _configuration = configuration;
        }

        public async Task<List<TicketLimpeza>> GetAll()
        {
            return await _dbContext.TicketsLimpeza.ToListAsync();
        }

        public async Task<TicketLimpeza> GetById(int id)
        {
            return await _dbContext.TicketsLimpeza.FirstOrDefaultAsync(entity => entity.LimpezaId == id);
        }

        public async Task<TicketLimpeza> Insert(TicketLimpeza entity, LimpezaPrioridade limpezaPrioridade)
        {
            // Verifica se j� existe um ticket de limpeza para o quarto especifico
            var existingTicket = await TicketLimpezaExist(entity.QuartoQuartoId);

            // Se j� existir um ticket para o quarto, lan�a uma exce��o
            if (existingTicket != null) throw new InvalidOperationException($"O quarto {entity.QuartoQuartoId} já tem limpeza agendada");


            entity.LimpezaDataCriacao = DateTime.Now;
            entity.LimpezaEstado = LimpezaEstados.Pendente;
            entity.LimpezaDataAtualizacao = DateTime.Now;
            entity.LimpezaPrioridade = limpezaPrioridade;

            await _dbContext.TicketsLimpeza.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }


        public async Task<TicketLimpeza> Update(TicketLimpeza entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            existingEntity.FuncionarioFuncionarioId = entity.FuncionarioFuncionarioId;
            existingEntity.FuncionarioLimpezaId = entity.FuncionarioLimpezaId;
            existingEntity.QuartoQuartoId = entity.QuartoQuartoId;
            existingEntity.LimpezaDataCriacao = entity.LimpezaDataCriacao;
            existingEntity.LimpezaDataAtualizacao = entity.LimpezaDataAtualizacao;
            existingEntity.LimpezaEstado = entity.LimpezaEstado;
            existingEntity.LimpezaPrioridade = entity.LimpezaPrioridade;
            existingEntity.FuncionarioLimpeza = entity.FuncionarioLimpeza;
            existingEntity.FuncionarioFuncionario = entity.FuncionarioFuncionario;
            existingEntity.QuartoQuarto = entity.QuartoQuarto;

            _dbContext.TicketsLimpeza.Update(existingEntity);
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

            _dbContext.TicketsLimpeza.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        


        /// <summary>
        /// Inicializa o processo de limpeza para um ticket específico.
        /// </summary>
        /// <param name="id">O identificador único do ticket de limpeza.</param>
        /// <returns>O objeto TicketLimpeza após a inicialização da limpeza.</returns>
        /// <exception cref="InvalidOperationException">Exceção lançada quando o ticket de limpeza não está pendente de limpeza.</exception>
        public async Task<TicketLimpeza> InicializarLimpeza(int id, int AuxLimpezaID)
        {
            // Obtém o ticket de limpeza pelo seu ID
            TicketLimpeza ticketLimpeza = await GetById(id);
            // Verifica se o ticket não é nulo
            if (ticketLimpeza == null) return null;

            // Verifica se o estado da limpeza do ticket está pendente e Lança uma exceção se o estado não for pendente
            if (ticketLimpeza.LimpezaEstado != LimpezaEstados.Pendente) throw new InvalidOperationException($"Ticket de limpeza {id} não está pendente de limpeza");

            // Verifica se o ticket de limpeza tem um auxiliar de limpeza associado
            if(ticketLimpeza.FuncionarioLimpezaId != null) throw new InvalidOperationException($"Ticket de limpeza {id} já tem um auxiliar de limpeza associado");

            // Obtém o auxiliar de limpeza pelo seu ID
            Funcionario auxiliarLimpeza = await _funcionarioRepository.GetById(AuxLimpezaID);
            // Verifica se o auxiliar de limpeza não é nulo
            if (auxiliarLimpeza == null) throw new InvalidOperationException($"O seu ID de funcionário não é válido");
            
            // Atribui o auxiliar de limpeza ao ticket de limpeza
            ticketLimpeza.FuncionarioLimpezaId = auxiliarLimpeza.FuncionarioId;
          
            // Atualiza o estado da limpeza para "Iniciada"
            ticketLimpeza.LimpezaEstado = LimpezaEstados.Iniciada;

            // Atualiza a data de atualização do ticket de limpeza
            ticketLimpeza.LimpezaDataAtualizacao = DateTime.Now;

            // Atualiza o ticket de limpeza no contexto da base de dados
            _dbContext.TicketsLimpeza.Update(ticketLimpeza);
            // Guarda as alterações na base de dados
            await _dbContext.SaveChangesAsync();
            // Retorna o ticket de limpeza atualizado
            return ticketLimpeza;
        }


        /// <summary>
        /// Finaliza o processo de limpeza para um ticket específico, atribuindo um estado de finalização.
        /// </summary>
        /// <param name="id">O identificador único do ticket de limpeza.</param>
        /// <param name="estadoFinalizacao">O estado final para atribuir ao ticket de limpeza.</param>
        /// <returns>O objeto TicketLimpeza após a finalização da limpeza.</returns>
        /// <exception cref="InvalidOperationException">Exceção lançada quando o ticket de limpeza não está em andamento ou quando o estado de finalização é inválido.</exception>
        public async Task<TicketLimpeza> FinalizarLimpeza(int id, LimpezaEstados estadoFinalizacao)
        {
            // Obtém o ticket de limpeza pelo seu ID
            TicketLimpeza ticketLimpeza = await GetById(id);
            // Verifica se o ticket não é nulo
            if (ticketLimpeza == null) return null;

            // Verifica se o estado da limpeza do ticket está como "Iniciada" e Lança uma exceção se o estado não for "Iniciada"
            if (ticketLimpeza.LimpezaEstado != LimpezaEstados.Iniciada) throw new InvalidOperationException($"Ticket de limpeza {id} não está em andamento");

            // Verifica se o estado de finalização é válido e Lança uma exceção se o estado de finalização não for válido
            if (estadoFinalizacao != LimpezaEstados.Finalizada && estadoFinalizacao != LimpezaEstados.FinalizadaProblemas) throw new InvalidOperationException($"Estado de finalização inválido");
            // Atualiza o estado da limpeza para o estado de finalização especificado
            ticketLimpeza.LimpezaEstado = estadoFinalizacao;
            // Atualiza a data de atualização do ticket de limpeza
            ticketLimpeza.LimpezaDataAtualizacao = DateTime.Now;
            // Atualiza o ticket de limpeza no contexto da base de dados
            _dbContext.TicketsLimpeza.Update(ticketLimpeza);
            // Guarda as alterações na base de dados
            await _dbContext.SaveChangesAsync();


            //Envio de notificaçoes para a receção caso necessário
            if (estadoFinalizacao == LimpezaEstados.FinalizadaProblemas)
            {
                // Criar a mensagem de email
                string mensagem = MensagensNotificacoes.MensagensNotificacoes.CriarMensagemTerminoLimpeza(ticketLimpeza, ticketLimpeza.LimpezaEstado);
                // Enviar a mensagem de email
                Notificacoes.Notificacoes.EnviarLembreteConfirmacao(_configuration.GetSection("AppSettings:ReceptionEmail").Value!, "Término de Limpeza com Problemas", mensagem);
            }
            else
            {
                if (ticketLimpeza.FuncionarioFuncionarioId != null)
                {
                    // Criar a mensagem de email
                    string mensagem = MensagensNotificacoes.MensagensNotificacoes.CriarMensagemTerminoLimpeza(ticketLimpeza, ticketLimpeza.LimpezaEstado);
                    // Enviar a mensagem de email
                    Notificacoes.Notificacoes.EnviarLembreteConfirmacao(_configuration.GetSection("AppSettings:ReceptionEmail").Value!, "Término de Limpeza", mensagem);

                }
            }

            return ticketLimpeza;
        }


        /// <summary>
        /// Obtém uma lista de tickets de limpeza ativos associados a um auxiliar de limpeza específico.
        /// Este tambem retorna os tickets que não está associados a nenhum auxiliar de limpeza.
        /// </summary>
        /// <param name="auxLimpezaid">O ID do auxiliar de limpeza.</param>
        /// <returns>Uma lista de tickets de limpeza.</returns>
        public async Task<List<TicketLimpeza>> GetTicketsActivosPorAuxLimpeza(int auxLimpezaid)
        {
            // Filtra os tickets de limpeza com base no ID do auxiliar de limpeza ou se não estão associados a nenhum auxiliar de limpeza e que não estão finalizados
            return await _dbContext.TicketsLimpeza
                .Where(entity => (entity.FuncionarioLimpezaId == auxLimpezaid || entity.FuncionarioLimpeza == null) && (entity.LimpezaEstado != LimpezaEstados.Finalizada || entity.LimpezaEstado != LimpezaEstados.FinalizadaProblemas))
                .ToListAsync();
        }

        /// <summary>
        /// Cria um novo ticket de limpeza para um quarto que será limpo para o check-out.
        /// </summary>
        /// <param name="quartoID">O ID do quarto para o qual o ticket de limpeza está sendo criado.</param>
        /// <returns>O novo ticket de limpeza criado.</returns>
        public async Task<TicketLimpeza> CriarTicketLimpezaParaCheckout(string quartoID)
        {
            // Cria um novo ticket de limpeza com as informações necessárias
            TicketLimpeza newTicketLimpeza = new TicketLimpeza
            {
                QuartoQuartoId = quartoID,
                LimpezaDataCriacao = DateTime.Now,
                LimpezaEstado = LimpezaEstados.Pendente,
                LimpezaDataAtualizacao = DateTime.Now
            };

            // Obtém a lista de reservas para o quarto
            List<Reserva> reservas = await _reservaRepository.GetReservasQuartoId(quartoID);

            // Verifica se há reservas para o quarto e determina a prioridade do ticket de limpeza
            if (reservas == null)
            {
                // Se não houver reservas, define a prioridade como Check-out
                newTicketLimpeza = await Insert(newTicketLimpeza, LimpezaPrioridade.ChekOut);
            }
            else if (reservas.FirstOrDefault(reserva => reserva.DataInicio.Date == DateTime.Today) != null)
            {
                // Se houver uma reserva com inicio para hoje, define a prioridade como Check-in hoje
                newTicketLimpeza = await Insert(newTicketLimpeza, LimpezaPrioridade.ChekInHoje);
            }
            else
            {
                // Caso contrário, define a prioridade como Check-out
                newTicketLimpeza = await Insert(newTicketLimpeza, LimpezaPrioridade.ChekOut);
            }

            // Retorna o novo ticket de limpeza criado
            return newTicketLimpeza;
        }



        // Método para listar os tickets de limpeza por prioridade
        public async Task<List<TicketLimpeza>> GetByPriorityOrder()
        {
            // Obtém a data atual
            DateTime hoje = DateTime.Today;

            // Atualiza os quartos para limpeza para o dia atual
            await AtualizarQuartosParaLimpeza();

            // Retorna a lista de tickets de limpeza
            var ticketsHoje = await _dbContext.TicketsLimpeza
                .Where(ticket => ticket.LimpezaDataCriacao.Date == hoje)
                .OrderBy(ticket => ticket.LimpezaPrioridade)
                .ToListAsync();

            var ticketsOutrosDias = await _dbContext.TicketsLimpeza
                .Where(ticket => ticket.LimpezaDataCriacao.Date != hoje &&
                                 (ticket.LimpezaEstado == LimpezaEstados.Pendente ||
                                  ticket.LimpezaEstado == LimpezaEstados.Iniciada))
                .OrderBy(ticket => ticket.LimpezaPrioridade)
                .ToListAsync();

            return ticketsHoje.Concat(ticketsOutrosDias).ToList();
        }


        // Método para verificar se já existe um ticket de limpeza para um quarto
        public async Task<TicketLimpeza> TicketLimpezaExist(string quartoId)
        {
            // Verifica se existe um ticket de limpeza ativo para o quarto no mesmo dia
            return await _dbContext.TicketsLimpeza
                .FirstOrDefaultAsync(ticket =>
                    ticket.QuartoQuartoId == quartoId
                    && ticket.LimpezaEstado == LimpezaEstados.Pendente);
        }


        private async Task AtualizarQuartosParaLimpeza()
        {
            // Obtém a data atual
            DateTime hoje = DateTime.Today.Date;

            // Obtém todos os quartos para o dia de hoje
            var todosQuartosHoje = await _dbContext.Reservas
                .Where(r => r.DataInicio.Date == hoje)
                .Select(r => r.QuartoQuartoId)
                .ToListAsync();

            // Verifica e adiciona os quartos para limpeza, se necessário
            foreach (var quartoId in todosQuartosHoje)
            {
                // Verifica se já existe um ticket de limpeza para o quarto com a data de hoje
                bool temTicketLimpeza = await VerificarTicketLimpezaExistente(quartoId);

                if (!temTicketLimpeza)
                {
                    // Cria um novo ticket de limpeza para o quarto
                    var novoTicketLimpeza = new TicketLimpeza
                    {
                        QuartoQuartoId = quartoId,
                        LimpezaDataCriacao = hoje,
                        LimpezaEstado = LimpezaEstados.Pendente,
                        LimpezaPrioridade = LimpezaPrioridade.ChekInHoje
                    };

                    // Insere o novo ticket de limpeza no banco de dados
                    await InserirTicketLimpeza(novoTicketLimpeza);
                }
            }
        }

    

        private async Task<bool> VerificarTicketLimpezaExistente(string quartoId)
        {
            // Verifica se já existe um ticket de limpeza para o quarto com a data de hoje
            var hoje = DateTime.Today;
            return await _dbContext.TicketsLimpeza.AnyAsync(ticket =>
                ticket.QuartoQuartoId == quartoId &&
                ticket.LimpezaDataCriacao.Date == hoje);
        }



        private async Task InserirTicketLimpeza(TicketLimpeza ticketLimpeza)
        {
            // Adiciona o novo ticket de limpeza no banco de dados
            _dbContext.TicketsLimpeza.Add(ticketLimpeza);
            await _dbContext.SaveChangesAsync();
        }
    }
}

