using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository<Ticket> _ticketsRepository;
        private readonly IBaseQuartoRepository<Quarto> _quartoRepository;
        private readonly IFuncionarioFornecedorRepository<FuncionarioFornecedor> _fornecedoresRepository;

        public TicketController(ITicketRepository<Ticket> ticketsRepository, IFuncionarioFornecedorRepository<FuncionarioFornecedor> fornecedoresRepository, IBaseQuartoRepository<Quarto> quartoRepository)
        {
            _ticketsRepository = ticketsRepository;
            _fornecedoresRepository = fornecedoresRepository;
            _quartoRepository = quartoRepository;
        }

        // GET: api/Tickets
        [HttpGet, Authorize(Roles ="Gerente, Rececionista")]
        public async Task<ActionResult<List<Ticket>>> GetTickets()
        {
            List<Ticket> tickets = await _ticketsRepository.GetAll();
            if (tickets.Count == 0) return NotFound("Sem tickets registados!");
            return Ok(tickets);
        }

        [HttpGet("Filtrados-Por-Estado"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<List<Ticket>>> GetAllFiltrados(TicketEstados estado)
        {
            List<Ticket> tickets = await _ticketsRepository.AllFiltrados(estado);
            if (tickets.Count == 0) return NotFound("Sem tickets registados!");
            return Ok(tickets);
        }

        [HttpGet("Filtrados-Por-FuncionarioFornecedor-E-Estado-Iniciado"), Authorize(Roles ="FuncionarioFornecedor")]
        public async Task<ActionResult<List<Ticket>>> GetAllFiltradosFuncFornecedorEstadoIniciado(int funcFornecedorId)
        {
            List<Ticket> tickets = await _ticketsRepository.GetAllFiltradosFuncFornecedorEstadoIniciado(funcFornecedorId);
            return Ok(tickets);
        }

        // GET: api/Tickets/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            Ticket ticket = await _ticketsRepository.GetById(id);
            if (ticket == null) return NotFound($"Ticket com o {id} n�o existe!");
            return Ok(ticket);
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Ticket>> PutTicket(int id, Ticket entity)
        {
            if (id != entity.TicketId)
            {
                return BadRequest();
            }

            Ticket ticket = await _ticketsRepository.GetById(entity.TicketId);
            if (ticket == null) return NotFound($"Ticket com o ID {entity.TicketId} n�o existe!");

            ticket = await _ticketsRepository.Update(entity, id);

            return Ok(ticket);
        }

        // POST: api/Tickets
        [HttpPost, Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket entity)
        {
            // Verificar se o pedido cumpre os requisitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            // Verificar se o ticket j� existe
            if ((await _ticketsRepository.GetById(entity.TicketId)) != null)
                return StatusCode(500, $"Ticket com o id {entity.TicketId} j� existe!");
            try
            {
                // Inserir o ticket no banco de dados
                await _ticketsRepository.Insert(entity);

                // Alterar o estado do quarto para Manutencao
                Quarto quarto = await _quartoRepository.GetbyId(entity.QuartoQuartoId);
                if (quarto != null)
                {
                    await _quartoRepository.AtualizarEstadoQuarto(quarto.QuartoId, (int)QuartoEstados.Manutencao);
                }

                // Notificar por e-mail sobre o novo ticket
            QuickRoomSolutions.Notificacoes.Notificacoes
                .EnviarLembreteConfirmacao("devtest_pikos@hotmail.com", $"Novo Pedido de manuten��o para {entity.QuartoQuartoId} ", $"{entity.TicketDataAbertura}\n\n{entity.TicketDescricao}");

                return CreatedAtAction("GetTicket", new { id = entity.TicketId }, entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar o ticket: {ex.Message}");
            }
        }


        // DELETE: api/Tickets/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            Ticket ticket = await _ticketsRepository.GetById(id);
            if (ticket == null) return NotFound($"Ticket com o ID {id} n�o existe!");

            bool apagado = await _ticketsRepository.DeleteById(id);

            return Ok();
        }


        [HttpPut("InicializarManutencao"), Authorize(Roles = "FuncionarioFornecedor")]
        public async Task<ActionResult<Ticket>> InicializarManutencao(int id, int FuncFornecedorID)
        {
            try
            {
                Ticket ticket = await _ticketsRepository.InicializarManutencao(id, FuncFornecedorID);
                if (ticket == null) return NotFound($"Ticket {id} nao existe!");
                return Ok(ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("FinalizarManutencao"), Authorize(Roles = "FuncionarioFornecedor")]
        public async Task<ActionResult<TicketLimpeza>> FinalizarManutencao(int id)
        {
            try
            {
                // Obt�m o ticket de limpeza pelo seu ID
                Ticket ticket = await _ticketsRepository.FinalizarManutencao(id);

                // Verifica se o ticket n�o existe
                if (ticket == null) return NotFound($"Ticket {id} nao existe!");

                // Retorna o ticket de limpeza
                return Ok(ticket);
            }
            catch (InvalidOperationException ex)
            {
                // Retorna uma resposta BadRequest com a mensagem de erro
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("AprovarTicket"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> AprovarTicket(Ticket ticket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (ticket == null || await _ticketsRepository.GetById(ticket.TicketId) == null)
            {
                return BadRequest("Ticket inexistente");
            }
            try
            {
                ticket = await _ticketsRepository.AprovarTicket(ticket.TicketId);

                await _ticketsRepository.EnviarEmailFornecedoresComServico(ticket);

                return Ok(ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("RejeitarTicket"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> RejeitarTicket(Ticket ticket)
        {


            if (ticket == null || await _ticketsRepository.GetById(ticket.TicketId) == null)
            {
                return NotFound("Ticket inexistente");
            }
            try
            {
                ticket = await _ticketsRepository.RejeitarTicket(ticket.TicketId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(ticket);
        }


        [HttpPut("Aprovar-Manutencao-Prestada"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> AprovarManutencaoPrestada(Ticket ticket)

        {
            if (ticket == null || await _ticketsRepository.GetById(ticket.TicketId) == null)
            {
                return NotFound("Ticket inexistente");
            }
            try
            {
                ticket = await _ticketsRepository.AprovarManutencaoPrestada(ticket.TicketId);

                await _ticketsRepository.EnviarEmailConclusaoServico(ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(ticket);
        }


        //[HttpPut("Pedir-Revisao-Manutencao-Prestada")]
        //public async Task<ActionResult> PedirRevisaoManutencaoPrestada(Ticket ticket)
        //{
        //    if (ticket == null || await _ticketsRepository.GetById(ticket.TicketId) == null)
        //    {
        //        return NotFound("Ticket inexistente");
        //    }
        //    try
        //    {
        //        ticket = await _ticketsRepository.PedirRevisaoManutencaoPrestada(ticket.TicketId);

        //        await _ticketsRepository.EnviarEmailRevisaoServico(ticket);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    return Ok(ticket);
        //}
    }
}
