using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public class TicketLimpezaController : ControllerBase
    {
        private readonly ITicketLimpezaRepository<TicketLimpeza> _registoLimpezasRepository;
        private readonly IReservasRepository<Reserva> _reservasRepository;
        private readonly IBaseQuartoRepository<Quarto> _quartoRepository;

        public TicketLimpezaController(ITicketLimpezaRepository<TicketLimpeza> registoLimpezasRepository, IReservasRepository<Reserva> reservasRepository, IBaseQuartoRepository<Quarto> baseQuartoRepository)
        {
            _registoLimpezasRepository = registoLimpezasRepository;
            _reservasRepository = reservasRepository;
            _quartoRepository = baseQuartoRepository;
        }

        // GET: api/RegistoLimpezas
        [HttpGet, Authorize(Roles = "Limpeza")]
        public async Task<ActionResult<List<TicketLimpeza>>> GetRegistoLimpezas()
        {
            List<TicketLimpeza> registoLimpezas = await _registoLimpezasRepository.GetAll();
            if (registoLimpezas.Count == 0) return NotFound("Sem registos de limpeza registados!");
            return Ok(registoLimpezas);
        }

        // GET: api/RegistoLimpezas/5
        [HttpGet("{id}"), Authorize(Roles = "Limpeza")]
        public async Task<ActionResult<TicketLimpeza>> GetRegistoLimpeza(int id)
        {
            TicketLimpeza registoLimpeza = await _registoLimpezasRepository.GetById(id);
            if (registoLimpeza == null) return NotFound($"Registo de limpeza com o {id} não existe!");
            return Ok(registoLimpeza);
        }

        // PUT: api/RegistoLimpezas/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<TicketLimpeza>> PutRegistoLimpeza(int id, TicketLimpeza entity)
        {
            if (id != entity.LimpezaId)
            {
                return BadRequest();
            }

            TicketLimpeza registoLimpeza = await _registoLimpezasRepository.GetById(entity.LimpezaId);
            if (registoLimpeza == null) return NotFound($"Registo de limpeza com o ID {entity.LimpezaId} não existe!");

            registoLimpeza = await _registoLimpezasRepository.Update(entity, id);

            return Ok(registoLimpeza);
        }

        // POST: api/RegistoLimpezas
        [HttpPost, Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<TicketLimpeza>> PostRegistoLimpeza(TicketLimpeza entity)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                TicketLimpeza ticket = await _registoLimpezasRepository.Insert(entity, LimpezaPrioridade.ReqCliente);
                
                return Ok (ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/RegistoLimpezas/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteRegistoLimpeza(int id)
        {
            TicketLimpeza registoLimpeza = await _registoLimpezasRepository.GetById(id);
            if (registoLimpeza == null) return NotFound($"Registo de limpeza com o ID {id} não existe!");

            bool apagado = await _registoLimpezasRepository.DeleteById(id);

            return Ok();
        }

        // GET: api/TicketLimpeza/OrderByPriority
        [HttpGet("OrderByPriority"), Authorize (Roles = "Limpeza")]
        public async Task<ActionResult<List<TicketLimpeza>>> GetByPriorityOrder()
        {
            List<TicketLimpeza> tickets = await _registoLimpezasRepository.GetByPriorityOrder();
            if (tickets.Count == 0)
                return StatusCode(404, "Nenhum ticket de limpeza encontrado.");

            return Ok(tickets);
        }


        [HttpPut("InicializarLimpeza"), Authorize(Roles = "Limpeza")]
        public async Task<ActionResult<TicketLimpeza>> InicializarLimpeza(int id, int AuxLimpezaID)
        {
            try
            {
                TicketLimpeza ticketLimpeza = await _registoLimpezasRepository.InicializarLimpeza(id, AuxLimpezaID);
                if (ticketLimpeza == null) return NotFound($"Ticket de limpeza {id} não existe!"); 
                return Ok(ticketLimpeza);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("FinalizarLimpeza"), Authorize(Roles = "Limpeza")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TicketLimpeza))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketLimpeza>> FinalizarLimpeza(int id, [EnumDataType(typeof(LimpezaEstados))] [Range(3, 4)] LimpezaEstados estadoFinalizacao)
        {
            try
            {
                // Obtém o ticket de limpeza pelo seu ID
                TicketLimpeza ticketLimpeza = await _registoLimpezasRepository.FinalizarLimpeza(id, estadoFinalizacao);

                // Verifica se o ticket não existe
                if (ticketLimpeza == null) return NotFound($"Ticket de limpeza {id} não existe!");

                // Obtém a lista de reservas associadas ao quarto do ticket de limpeza
                List<Reserva> listReservasQuarto = await _reservasRepository.GetReservasQuartoId(ticketLimpeza.QuartoQuartoId);

                // Verifica se não há reservas para o quarto
                if (listReservasQuarto.Count == 0) await _quartoRepository.AtualizarEstadoQuarto(ticketLimpeza.QuartoQuartoId, 1); // Atualiza o estado do quarto para Disponível
                else if (listReservasQuarto.FirstOrDefault(x => x.EstadoReserva == ReservaEstado.Ativa) == null) await _quartoRepository.AtualizarEstadoQuarto(ticketLimpeza.QuartoQuartoId, 1); // Atualiza o estado do quarto para Disponível
                else await _quartoRepository.AtualizarEstadoQuarto(ticketLimpeza.QuartoQuartoId, 3); // Atualiza o estado do quarto para Ocupado

                // Retorna o ticket de limpeza
                return Ok(ticketLimpeza);
            }
            catch (InvalidOperationException ex)
            {
                // Retorna uma resposta BadRequest com a mensagem de erro
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("Get Tickets Ativos Por AuxLimpeza"), Authorize(Roles = "Limpeza")]
        public async Task<ActionResult<List<TicketLimpeza>>> GetTicketPorAuxLimpeza(int id)
        {
            List<TicketLimpeza> registoLimpezas = await _registoLimpezasRepository.GetTicketsActivosPorAuxLimpeza(id);
            if (registoLimpezas.Count == 0) return StatusCode(500, "Sem registos de limpeza registados!");
            return Ok(registoLimpezas);
        }


    }
}
