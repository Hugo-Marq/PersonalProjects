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
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Services;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioController : ControllerBase
    {
        private readonly IFuncionarioRepository<Funcionario> _funcionariosRepository;
        private readonly JWTService _jWTService;

        public FuncionarioController(IFuncionarioRepository<Funcionario> funcionariosRepository, JWTService jWTService)
        {
            _funcionariosRepository = funcionariosRepository;
            _jWTService = jWTService;
        }

        // GET: api/Funcionarios
        [HttpGet, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Funcionario>>> GetFuncionarios()
        {
            List<Funcionario> funcionarios = await _funcionariosRepository.GetAll();
            if (funcionarios.Count == 0) return StatusCode(500, "Sem funcionarios registados!");
            return Ok(funcionarios);
        }

        // GET: api/Funcionarios/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            Funcionario funcionario = await _funcionariosRepository.GetById(id);
            if (funcionario == null) return NotFound($"Funcionario com o {id} não existe!");
            return Ok(funcionario);
        }

        // PUT: api/Funcionarios/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Funcionario>> PutFuncionario(int id, Funcionario entity)
        {
            if (id != entity.FuncionarioId)
            {
                return BadRequest();
            }

            Funcionario funcionario = await _funcionariosRepository.GetById(entity.FuncionarioId);
            if (funcionario == null) return NotFound($"Funcionario com o ID {entity.FuncionarioId} não existe!");

            funcionario = await _funcionariosRepository.Update(entity, id);

            return Ok(funcionario);
        }

        // POST: api/Funcionarios
        [HttpPost("register"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario entity)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(entity.FuncionarioPassword);

            entity.FuncionarioPassword = hashedPassword;

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _funcionariosRepository.GetById(entity.FuncionarioId)) != null) return StatusCode(500, $"Funcionario com o id {entity.FuncionarioId} já existe!");

            if (entity.PessoaPessoa == null)
            {
                entity = await _funcionariosRepository.AssociarPessoaAoRegisto(entity);
            }
            await _funcionariosRepository.Insert(entity);

            return CreatedAtAction("GetFuncionario", new { id = entity.FuncionarioId }, entity);
        }

        // DELETE: api/Funcionarios/5
        [HttpDelete("{id}"),Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteFuncionario(int id)
        {
            Funcionario funcionario = await _funcionariosRepository.GetById(id);
            if (funcionario == null) return NotFound($"Funcionario com o ID {id} não existe!");

            bool apagado = await _funcionariosRepository.DeleteById(id);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDTO user)
        {
            try
            {
                Funcionario funcionario = await _funcionariosRepository.GetById(user.ID);
                if (funcionario == null) return NotFound($"Funcionario com o ID {user.ID} não existe!");

                if (!BCrypt.Net.BCrypt.Verify(user.Password, funcionario.FuncionarioPassword)) return StatusCode(401, "Password incorreta!");

                var token = new { Token = $"{_jWTService.GenerateToken<Funcionario>(funcionario)}", NivelCargo = $"{funcionario.CargoCargoId}"};

                return Ok(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Problemas com a conta");
            }
        }

    }
}

