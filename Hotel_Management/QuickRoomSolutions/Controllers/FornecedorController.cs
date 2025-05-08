using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FornecedorController : ControllerBase
    {

        private readonly IFornecedorRepository<Fornecedor> _fornecedorRepository;

        public FornecedorController(IFornecedorRepository<Fornecedor> fornecedorRepository)
        {
            _fornecedorRepository = fornecedorRepository;
        }

        // GET: api/Fornecedors
        [HttpGet, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Fornecedor>>> GetFornecedors()
        {
            List<Fornecedor> fornecedores = await _fornecedorRepository.GetAll();
            if (fornecedores.Count == 0) return StatusCode(500, "Sem fornecedores registados!");
            return Ok(fornecedores);
        }

        // GET: api/Fornecedors/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Fornecedor>> GetFornecedor(int id)
        {
            Fornecedor fornecedor = await _fornecedorRepository.GetById(id);
            if (fornecedor == null) return NotFound($"Fornecedor com o {id} não existe!");
            return Ok(fornecedor);
        }

        // PUT: api/Fornecedors/5
        [HttpPut("{id}"),Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Fornecedor>> PutFornecedor(int id, Fornecedor entity)
        {
            if (id != entity.FornecedorId)
            {
                return BadRequest();
            }

            Fornecedor Fornecedor = await _fornecedorRepository.GetById(entity.FornecedorId);
            if (Fornecedor == null) return NotFound($"Fornecedor com o ID {entity.FornecedorId} não existe!");

            Fornecedor = await _fornecedorRepository.Update(entity, id);

            return Ok(Fornecedor);
        }

        // POST: api/Fornecedors
        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Fornecedor>> PostFornecedor(Fornecedor entity)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _fornecedorRepository.GetById(entity.FornecedorId)) != null) return StatusCode(500, $"Fornecedor com o id {entity.FornecedorId} já existe!");


            await _fornecedorRepository.Insert(entity);

            return CreatedAtAction("GetFornecedor", new { id = entity.FornecedorId }, entity);
        }



        // DELETE: api/Fornecedors/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteFornecedor(int id)
        {
            Fornecedor fornecedor = await _fornecedorRepository.GetById(id);
            if (fornecedor == null) return NotFound($"Fornecedor com o ID {id} não existe!");

            bool apagado = await _fornecedorRepository.DeleteById(id);

            return Ok();
        }

        [HttpPut("EncerrarParceria"),Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Fornecedor>> EncerrarParceriaComFornecedor(int id)
        {
            Fornecedor fornecedor = await _fornecedorRepository.GetById(id);

            if (fornecedor == null)
            {
                return NotFound($"Fornecedor com o ID {id} não existe!");
            }

            fornecedor.FornecedorAtivo = false;

            fornecedor = await _fornecedorRepository.Update(fornecedor, id);

            return Ok(fornecedor);
        }


        [HttpPut("ReativarParceria"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Fornecedor>> ReativarParceriaComFornecedor(int id)
        {
            Fornecedor fornecedor = await _fornecedorRepository.GetById(id);

            if (fornecedor == null)
            {
                return NotFound($"Fornecedor com o ID {id} não existe!");
            }

            fornecedor.FornecedorAtivo = true;

            fornecedor = await _fornecedorRepository.Update(fornecedor, id);

            return Ok(fornecedor);
        }

        [HttpPut("AdicionarServicoFornecedor"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> AddServicoToFornecedor(int id,[FromBody] ServicosFornecedorDTO servicosFornecedorDTO)
        {
            if (id != servicosFornecedorDTO.FornecedorId)
            {
                return BadRequest("Id do cabeçalho não é coerente com o Body");
            }

            try
            {
                await _fornecedorRepository.AddServicosToFornecedor(servicosFornecedorDTO);
                return Ok("Serviço adicionado ao fornecedor com sucesso");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("AtualizarServicoFornecedor"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> UpdateServicoFornecedor(int id, [FromBody] ServicosFornecedorDTO servicosFornecedorDTO)
        {
            if (id != servicosFornecedorDTO.FornecedorId)
            {
                return BadRequest("Id do cabeçalho não é coerente com o Body");
            }

            try
            {
                await _fornecedorRepository.UpdateServicosFornecedor(servicosFornecedorDTO);
                return Ok("Serviços atualizados com sucesso");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET: api/Servico/FornecedoresAtivos/{servicoId}
        [HttpGet("FornecedoresAtivos/{servicoId}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Fornecedor>>> GetFornecedoresAtivosParaServico(int servicoId)
        {
            try
            {
                var fornecedoresAtivos = await _fornecedorRepository.GetFornecedoresAtivosPorServico(servicoId);

                if (fornecedoresAtivos.Count == 0)
                {
                    return NotFound("Nenhum fornecedor ativo encontrado para este serviço.");
                }

                return Ok(fornecedoresAtivos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar fornecedores ativos: {ex.Message}");
            }
        }
    }
}




