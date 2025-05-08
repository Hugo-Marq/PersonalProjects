using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrcamentoController : ControllerBase
    {
        private readonly IOrcamentoRepository<Orcamento> _orcamentosRepository;
        private readonly IFornecedorRepository<Fornecedor> _fornecedoresRepository;
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public OrcamentoController(IOrcamentoRepository<Orcamento> orcamentosRepository, IFornecedorRepository<Fornecedor> fornecedoresRepository)
        {
            _orcamentosRepository = orcamentosRepository;
            _fornecedoresRepository = fornecedoresRepository;
        }

        // GET: api/Orcamentos
        [HttpGet, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Orcamento>>> GetOrcamentos()
        {
            List<Orcamento> orcamentos = await _orcamentosRepository.GetAll();
            if (orcamentos.Count == 0) return NotFound("Sem orçamentos registados!");
            return Ok(orcamentos);
        }

        [HttpGet("Filtrados-Por-Estado"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Orcamento>>> GetOrcamentosFiltrados()
        {
            List<Orcamento> orcamentos = await _orcamentosRepository.GetAllFiltrados();
            return Ok(orcamentos);
        }

        // GET: api/Orcamentos/5
        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<Orcamento>> GetOrcamento(int id)
        {
            Orcamento orcamento = await _orcamentosRepository.GetById(id);
            if (orcamento == null) return NotFound($"Orçamento com o {id} não existe!");
            return Ok(orcamento);
        }

        // PUT: api/Orcamentos/5
        [HttpPut("{id}"), Authorize]
        public async Task<ActionResult<Orcamento>> PutOrcamento(int id, Orcamento entity)
        {
            if (id != entity.OrcamentoId)
            {
                return BadRequest();
            }

            Orcamento orcamento = await _orcamentosRepository.GetById(entity.OrcamentoId);
            if (orcamento == null) return NotFound($"Orçamento com o ID {entity.OrcamentoId} não existe!");

            orcamento = await _orcamentosRepository.Update(entity, id);

            return Ok(orcamento);
        }

        // POST: api/Orcamentos
        [HttpPost, Authorize(Roles = "FuncionarioFornecedor")]
        public async Task<ActionResult<Orcamento>> PostOrcamento(Orcamento entity)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if ((await _orcamentosRepository.GetById(entity.OrcamentoId)) != null)
            {
                return BadRequest($"Orçamento com o id {entity.OrcamentoId} já existe!");
           
            }
            if (await _orcamentosRepository.ExisteTicket(entity.TicketTicketId) == false) //não se pode instanciar em _ticketsRepository senao dá erro, dependência circular
            {
                return BadRequest($"Ticket com o id {entity.TicketTicketId} não existe!");
            }

            if((await _fornecedoresRepository.GetById(entity.FornecedorFornecedorId)) == null)
            {
                return BadRequest($"Fornecedor com o id {entity.FornecedorFornecedorId} não existe!");
            }

            if (await _orcamentosRepository.GetOrcamentoAceiteByTicketId(entity.TicketTicketId) != null)
            {
                return BadRequest("Já existe um orçamento aceite para este ticket!");
            }
            await _orcamentosRepository.Insert(entity);

            return CreatedAtAction("GetOrcamento", new { id = entity.OrcamentoId }, entity);
        }

        // DELETE: api/Orcamentos/5
        [HttpDelete("{id}"), Authorize]
        public async Task<ActionResult> DeleteOrcamento(int id)
        {
            Orcamento orcamento = await _orcamentosRepository.GetById(id);
            if (orcamento == null) return NotFound($"Orçamento com o ID {id} não existe!");

            bool apagado = await _orcamentosRepository.DeleteById(id);

            return Ok();
        }

        [HttpPut("Aceitar-Orcamento"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Orcamento>> AceitarOrcamento(Orcamento entity)
        {
            if (entity == null)
            {
                return NotFound($"Orçamento não existe!");
            }
            try
            {
                entity = await _orcamentosRepository.AceitarOrcamento(entity.OrcamentoId);

                await _orcamentosRepository.NotificarFornecedorAceitacaoOrcamento(entity);
            }
            catch(ArgumentNullException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(entity);
        }

        [HttpPut("Rejeitar-Orcamento"), Authorize(Roles ="Gerente")]
        public async Task<ActionResult<Orcamento>> RejeitarOrcamento(Orcamento entity)
        {
            if (entity == null)
            {
                return NotFound($"Orçamento não existe!");
            }
            try
            {
                entity = await _orcamentosRepository.RejeitarOrcamento(entity.OrcamentoId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(entity);
        }
    }
}
