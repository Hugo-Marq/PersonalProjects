using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoaController : ControllerBase
    {
        private readonly IPessoasRepository<Pessoa> _pessoasRepository;

        public PessoaController (IPessoasRepository<Pessoa> pessoasRepository)
        {
            _pessoasRepository = pessoasRepository;
        }

        [HttpGet, Authorize(Roles ="Gerente")]
        public async Task<ActionResult<List<Pessoa>>> GetAllPessoas()
        {
            List<Pessoa> pessoas = await _pessoasRepository.GetAll();
            if (pessoas.Count == 0) return StatusCode(500, "Sem pessoas!");
            return Ok(pessoas);
        }

        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Pessoa>> GetPessoabyId(int Nif)
        {
            Pessoa pessoa = await _pessoasRepository.GetbyId(Nif);
            if (pessoa == null) return NotFound($"Pessoa com o {Nif} não existe!");
            return Ok(pessoa);
        }


        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Reserva>> InsertPessoa([FromBody] Pessoa entity)
        {
            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
          
            //Verifica se a pessoa existe
            if ((await _pessoasRepository.GetbyId(entity.Nif)) != null) return StatusCode(500, $"Pessoa com o {entity.Nif} já existe!");

            //Inserir a pessoa
            entity = await _pessoasRepository.Insert(entity);
            return Ok(entity);

        }

        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Pessoa>> UpdatePessoa([FromBody] Pessoa entity, int id)
        {
            
            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Verifica se a pessoa existe
            Pessoa pessoa = await _pessoasRepository.GetbyId(entity.Nif);
            if (pessoa == null) return StatusCode(500, $"Pessoa com o {entity.Nif} não existe!");


            pessoa = await _pessoasRepository.Update(entity, id);
            return Ok(pessoa);

        }

        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Pessoa>> DeletePessoa(int id)
        {

            bool apagado = await _pessoasRepository.DeleteById(id);
            return Ok(apagado);

        }

    }
}
