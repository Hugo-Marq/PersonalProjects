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
    public class ServicoController : ControllerBase
    {
        private readonly IServicoRepository<Servico> _servicosRepository;

        public ServicoController(IServicoRepository<Servico> servicosRepository)
        {
            _servicosRepository = servicosRepository;
        }

        // GET: api/Servicos
        [HttpGet, Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<List<Servico>>> GetServicos()
        {
            List<Servico> servicos = await _servicosRepository.GetAll();
            if (servicos.Count == 0) return StatusCode(500, "Sem serviços registados!");
            return Ok(servicos);
        }

        // GET: api/Servicos/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Servico>> GetServico(int id)
        {
            Servico servico = await _servicosRepository.GetById(id);
            if (servico == null) return NotFound($"Serviço com o {id} não existe!");
            return Ok(servico);
        }

        // PUT: api/Servicos/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Servico>> PutServico(int id, Servico entity)
        {
            if (id != entity.ServicoId)
            {
                return BadRequest();
            }

            Servico servico = await _servicosRepository.GetById(entity.ServicoId);
            if (servico == null) return NotFound($"Serviço com o ID {entity.ServicoId} não existe!");

            servico = await _servicosRepository.Update(entity, id);

            return Ok(servico);
        }

        // POST: api/Servicos
        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Servico>> PostServico(Servico entity)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _servicosRepository.GetById(entity.ServicoId)) != null) return StatusCode(500, $"Serviço com o id {entity.ServicoId} já existe!");


            await _servicosRepository.Insert(entity);

            return CreatedAtAction("GetServico", new { id = entity.ServicoId }, entity);
        }

        // DELETE: api/Servicos/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteServico(int id)
        {
            Servico servico = await _servicosRepository.GetById(id);
            if (servico == null) return NotFound($"Serviço com o ID {id} não existe!");

            bool apagado = await _servicosRepository.DeleteById(id);

            return Ok();
        }
    }
}
