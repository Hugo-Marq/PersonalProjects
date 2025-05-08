using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickRoomSolutions.Repositories
{
    public class TicketRepository : ITicketRepository<Ticket>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;
        private readonly IFuncionarioFornecedorRepository<FuncionarioFornecedor> _funcionarioFornecedorRepository;
        private readonly IConfiguration _configuration;
        private readonly IOrcamentoRepository<Orcamento> _orcamentoRepository;

        public TicketRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext, IFuncionarioFornecedorRepository<FuncionarioFornecedor> funcionarioFornecedorRepository, IOrcamentoRepository<Orcamento> orcamentoRepository, IConfiguration configuration)
        {
            _dbContext = quickRoomSolutionDatabaseContext;
            _funcionarioFornecedorRepository = funcionarioFornecedorRepository;
            _orcamentoRepository = orcamentoRepository;
            _configuration = configuration;
        }

        public async Task<List<Ticket>> GetAll()
        {
            return await _dbContext.Tickets.ToListAsync();
        }

        public async Task<Ticket> GetById(int id)
        {
            return await _dbContext.Tickets.FirstOrDefaultAsync(entity => entity.TicketId == id);
        }

        public async Task<List<Ticket>> AllFiltrados(TicketEstados estado)
        {
            return await _dbContext.Tickets.Where(entity => entity.TicketEstado == estado).ToListAsync();
        }

        public async Task<List<Ticket>> GetAllFiltradosFuncFornecedorEstadoIniciado(int funcFornecedorId)
        {
            return await _dbContext.Tickets
                .Where(entity => entity.FuncionarioFornecedorFuncFornecedorId == funcFornecedorId && entity.TicketEstado == TicketEstados.Iniciado).ToListAsync();
        }

        public async Task<Ticket> Insert(Ticket entity)
        {
            await _dbContext.Tickets.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Ticket> Update(Ticket entity, int id)
        {
            var existingEntity = await GetById(id);
            if (existingEntity == null)
            {
                return null;
            }

            existingEntity.TicketDescricao = entity.TicketDescricao;
            existingEntity.TicketEstado = entity.TicketEstado;
            existingEntity.TicketDataAbertura = entity.TicketDataAbertura;
            existingEntity.TickectDataAtualizacao = entity.TickectDataAtualizacao;
            existingEntity.FuncionarioFornecedorFuncFornecedorId = entity.FuncionarioFornecedorFuncFornecedorId;
            existingEntity.ServicoId = entity.ServicoId;
            existingEntity.QuartoQuartoId = entity.QuartoQuartoId;
            existingEntity.FuncionarioFuncionarioId = entity.FuncionarioFuncionarioId;
            existingEntity.FuncionarioFuncionario = entity.FuncionarioFuncionario;
            existingEntity.FuncionarioFornecedor = entity.FuncionarioFornecedor;
            existingEntity.Orcamentos = entity.Orcamentos;
            existingEntity.QuartoQuarto = entity.QuartoQuarto;
            existingEntity.ServicoServico = entity.ServicoServico;

            _dbContext.Tickets.Update(existingEntity);
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

            _dbContext.Tickets.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Inicializa uma manuten��o para um ticket espec�fico.
        /// </summary>
        /// <param name="id">O ID do ticket.</param>
        /// <param name="funcFornecedorId">O ID do funcion�rio do fornecedor.</param>
        /// <returns>O ticket atualizado com o estado de manuten��o iniciada.</returns>
        /// <exception cref="InvalidOperationException">Exce��o lan�ada se o ticket n�o estiver dispon�vel para ser iniciado, 
        /// se o ID do funcion�rio n�o for v�lido ou se o funcion�rio n�o puder iniciar o servi�o.</exception>
        public async Task<Ticket> InicializarManutencao(int id, int funcFornecedorId)
        {
            // Obt�m o ticket pelo ID
            Ticket ticket = await GetById(id);
            // Verifica se o ticket existe
            if (ticket == null) return null;
            // Verifica se o ticket est� dispon�vel para ser iniciado
            if (ticket.TicketEstado != TicketEstados.Atribuido && ticket.TicketEstado != TicketEstados.ParaRevisao) throw new InvalidOperationException($"Ticket {id} não se encontra disponível para ser iniciado");
            // Obt�m o funcion�rio fornecedor pelo ID
            FuncionarioFornecedor funcionarioFornecedor = await _funcionarioFornecedorRepository.GetById(funcFornecedorId);
            // Verifica se o funcion�rio do fornecedor existe
            if (funcionarioFornecedor == null) throw new InvalidOperationException($"O seu ID de funcionário não é válido");
            // Verifica se o funcion�rio do fornecedor pode iniciar o servi�o para este ticket
            if (!await VerificarCompatibilidadeFornecedorEntreFuncEOrcamentoAceite(funcionarioFornecedor, ticket)) throw new InvalidOperationException($"O seu ID de funcion�rio n�o lhe permite iniciar este servi�o!");

            // Atualiza o ID do funcion�rio fornecedor no ticket
            ticket.FuncionarioFornecedorFuncFornecedorId = funcionarioFornecedor.FuncFornecedorId;
            // Atualiza o estado do ticket para 'Iniciado'
            ticket.TicketEstado = TicketEstados.Iniciado;
            // Atualiza a data de atualiza��o do ticket
            ticket.TickectDataAtualizacao = DateTime.Now;

            

            // Atualiza o ticket no contexto da base de dados
            _dbContext.Tickets.Update(ticket);

            //Grava as altera��es na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna o ticket atualizado
            return ticket;
        }


        /// <summary>
        /// Verifica se h� compatibilidade entre o fornecedor associado ao funcion�rio do fornecedor
        /// e o fornecedor associado ao or�amento aceite para o ticket.
        /// </summary>
        /// <param name="funcFornecedor">O funcion�rio do fornecedor.</param>
        /// <param name="ticket">O ticket.</param>
        /// <returns>
        /// Verdadeiro se houver compatibilidade entre o fornecedor associado ao funcion�rio
        /// e o fornecedor associado ao or�amento aceite pelo ticket, caso contr�rio, falso.
        /// </returns>
        public async Task<bool> VerificarCompatibilidadeFornecedorEntreFuncEOrcamentoAceite(FuncionarioFornecedor funcFornecedor, Ticket ticket)
        {
            // Obt�m o or�amento aceite pelo ticket
            Orcamento orcamento = await _orcamentoRepository.GetOrcamentoAceiteByTicketId(ticket.TicketId);

            // Se n�o houver um or�amento aceite, n�o h� compatibilidade
            if (orcamento == null) return false;

            // Verifica se o fornecedor associado ao funcion�rio � o mesmo que o fornecedor associado ao or�amento
            if (funcFornecedor.FornecedorFornecedorId != orcamento.FornecedorFornecedorId) return false;

            // Se chegou at� aqui, h� compatibilidade
            return true;
        }


        /// <summary>
        /// Finaliza a manuten��o de um ticket.
        /// </summary>
        /// <param name="ticketId">O ID do ticket a ser finalizado.</param>
        /// <returns>O ticket atualizado com o estado de manuten��o finalizada.</returns>
        /// <exception cref="InvalidOperationException">Exce��o lan�ada se o ticket n�o estiver em andamento.</exception>
        public async Task<Ticket> FinalizarManutencao(int ticketId)
        {
            // Obt�m o ticket pelo ID
            Ticket ticket = await GetById(ticketId);
            // Verifica se o ticket existe
            if (ticket == null) return null;

            // Verifica se o ticket est� em andamento
            if (ticket.TicketEstado != TicketEstados.Iniciado)
                throw new InvalidOperationException($"Ticket {ticketId} não está em andamento");

            // Atualiza o estado do ticket para 'Finalizado'
            ticket.TicketEstado = TicketEstados.Finalizado;
            // Atualiza a data de atualiza��o do ticket
            ticket.TickectDataAtualizacao = DateTime.Now;

            // Atualiza o ticket no contexto da base de dados
            _dbContext.Tickets.Update(ticket);

            // Criar a mensagem de email
            string mensagem = MensagensNotificacoes.MensagensNotificacoes.CriarMensagemTerminoManutencao(ticket);
            // Enviar a mensagem de email
            Notificacoes.Notificacoes.EnviarLembreteConfirmacao("devtest_pikos@hotmail.com", "Término de Manutenção", mensagem);

            // Salva as mudan�as na base de dados
            await _dbContext.SaveChangesAsync();
            // Retorna o ticket atualizado
            return ticket;
        }



        public async Task<Ticket> AprovarTicket(int ticketId)
        {
            // Load the ticket from the database
            Ticket ticket = await _dbContext.Tickets.FindAsync(ticketId);

            // Verifica se o ticket esta pendente
            if (ticket.TicketEstado != TicketEstados.Pendente)
            {
                throw new InvalidOperationException($"Ticket {ticket.TicketId} não está pendente");
            }
            ticket.TicketEstado = TicketEstados.Validado;
            ticket.TickectDataAtualizacao = DateTime.Now;

            // Atualiza o ticket na base de dados
            _dbContext.Tickets.Update(ticket);

            // Grava as mudancas na base de dados
            await _dbContext.SaveChangesAsync();

            return ticket;
        }


        public async Task EnviarEmailFornecedoresComServico(Ticket ticket)
        {
            if (ticket == null)
            {
                   return;
            }
            Servico service = await _dbContext.Servicos.Where(s => s.ServicoId == ticket.ServicoId).FirstOrDefaultAsync();
            List<Fornecedor> fornecedorComServico = await _dbContext.Fornecedores.Where(f => f.ServicosServicos.Any(s => s.ServicoId == ticket.ServicoId)).ToListAsync();

            foreach (Fornecedor fornecedor in fornecedorComServico)
            {
                Notificacoes.Notificacoes.EnviarLembreteConfirmacao("devtest_pikos@hotmail.com"
                    ,$"Pedido de Orçamento para serviço de: {service.ServicoTipo}", $"Numero de Ticket Para inserir no orçamento: {ticket.TicketId}\n\nDescrição do Problema:\n\n{ticket.TicketDescricao}");
            }
        }

        public async Task<Ticket> RejeitarTicket(int ticketId)
        {
            // Load the ticket from the database
            Ticket ticket = await _dbContext.Tickets.FindAsync(ticketId);

            if (ticket == null) return null;

            // Verifica se o ticket esta pendente
            if (ticket.TicketEstado != TicketEstados.Pendente)
            {
                throw new InvalidOperationException($"Ticket {ticket.TicketId} não está pendente");
            }

            // Atualiza o estado do ticket para 'Rejeitado'
            ticket.TicketEstado = TicketEstados.Rejeitado;

            // Atualiza a data de atualizacao do ticket
            ticket.TickectDataAtualizacao = DateTime.Now;

            // Atualiza o ticket na base de dados
            _dbContext.Tickets.Update(ticket);

            // Grava as mudancas na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna o ticket atualizado
            return ticket;
        }

        public async Task<Ticket> AprovarManutencaoPrestada(int ticketId)
        {
            Ticket ticket = await _dbContext.Tickets.FindAsync(ticketId);

            if (ticket == null) return null;

            if (ticket.TicketEstado != TicketEstados.Finalizado)
            {
                throw new InvalidOperationException($"A manutenção não se encontra finalizada!");
            }
            // Atualiza o estado do ticket para 'Concluido'
            ticket.TicketEstado = TicketEstados.Concluido;

            // Atualiza a data de atualizacao do ticket
            ticket.TickectDataAtualizacao = DateTime.Now;

            // Atualiza o ticket na base de dados
            _dbContext.Tickets.Update(ticket);

            // Grava as mudancas na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna o ticket atualizado
            return ticket;
        }

        public async Task<Ticket> PedirRevisaoManutencaoPrestada(int ticketId)
        {
            Ticket ticket = await _dbContext.Tickets.FindAsync(ticketId);

            if (ticket == null) return null;

            if (ticket.TicketEstado != TicketEstados.Finalizado)
            {
                throw new InvalidOperationException($"A manutenção não se encontra finalizada!");
            }
            // Atualiza o estado do ticket para 'Concluido'
            ticket.TicketEstado = TicketEstados.ParaRevisao;

            // Atualiza a data de atualizacao do ticket
            ticket.TickectDataAtualizacao = DateTime.Now;

            // Atualiza o ticket na base de dados
            _dbContext.Tickets.Update(ticket);

            // Grava as mudancas na base de dados
            await _dbContext.SaveChangesAsync();

            // Retorna o ticket atualizado
            return ticket;
        }
        public async Task EnviarEmailConclusaoServico(Ticket ticket)
        {
            if (ticket == null) return;

            // Retrieve the related 'Orcamento' object based on the ticket ID
            FuncionarioFornecedor funcManutencao = await _dbContext.FuncionarioFornecedores
                .Where(f => f.FornecedorFornecedorId == ticket.FuncionarioFuncionarioId)
                .FirstOrDefaultAsync();

            // If no 'Orcamento' is found, exit the method
            if (funcManutencao == null) return;
            Fornecedor fornecedor = await _dbContext.Fornecedores
                .Where(f => f.FornecedorId == funcManutencao.FornecedorFornecedorId)
                .FirstOrDefaultAsync();

            // If the supplier does not exist, exit the method
            if (fornecedor == null) return;

            // Send the email notification
            Notificacoes.Notificacoes.EnviarLembreteConfirmacao(
                "devtest_pikos@hotmail.com" /*should be FornecedorEmail*/,
                $"Manutenção Concluida relativo ao ticket {ticket.TicketId}",
                $"Trabalho concluido"
            );
        }
        public async Task EnviarEmailRevisaoServico(Ticket ticket)
        {
            if (ticket == null) return;

            // Retrieve the related 'Orcamento' object based on the ticket ID
            FuncionarioFornecedor funcManutencao = await _dbContext.FuncionarioFornecedores
                .Where(f => f.FornecedorFornecedorId == ticket.FuncionarioFuncionarioId)
                .FirstOrDefaultAsync();

            // If no 'Orcamento' is found, exit the method
            if (funcManutencao == null) return;
            Fornecedor fornecedor = await _dbContext.Fornecedores
                .Where(f => f.FornecedorId == funcManutencao.FornecedorFornecedorId)
                .FirstOrDefaultAsync();

            // If the supplier does not exist, exit the method
            if (fornecedor == null) return;

            // Send the email notification
            Notificacoes.Notificacoes.EnviarLembreteConfirmacao(
                "devtest_pikos@hotmail.com" /*should be FornecedorEmail*/,
                $"Finalização de serviço não aprovado. ",
                $"O Serviço relativo ao ticket {ticket.TicketId} apresenta problemas e requer novo agendamento"
            );
        }
    }
}
