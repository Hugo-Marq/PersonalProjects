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
using QuickRoomSolutions.Repositories;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Services;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioFornecedorController : ControllerBase
    {
        private readonly IFuncionarioFornecedorRepository<FuncionarioFornecedor> _funcionarioFornecedoresRepository;
        private readonly JWTService _jWTService;

        public FuncionarioFornecedorController(IFuncionarioFornecedorRepository<FuncionarioFornecedor> funcionarioFornecedoresRepository, JWTService jWTService)
        {
            _funcionarioFornecedoresRepository = funcionarioFornecedoresRepository;
            _jWTService = jWTService;
        }

        // GET: api/FuncionarioFornecedores
        [HttpGet,Authorize]
        public async Task<ActionResult<List<FuncionarioFornecedor>>> GetFuncionarioFornecedores()
        {
            List<FuncionarioFornecedor> funcionarioFornecedores = await _funcionarioFornecedoresRepository.GetAll();
            if (funcionarioFornecedores.Count == 0) return NotFound("Sem funcionario registados!");
            return Ok(funcionarioFornecedores);
        }

        // GET: api/FuncionarioFornecedors/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<FuncionarioFornecedor>> GetFuncionarioFornecedor(int id)
        {
            FuncionarioFornecedor funcionarioFornecedor = await _funcionarioFornecedoresRepository.GetById(id);
            if (funcionarioFornecedor == null) return NotFound($"Funcionario com o {id} não existe!");
            return Ok(funcionarioFornecedor);
        }

        // PUT: api/FuncionarioFornecedores/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<FuncionarioFornecedor>> PutFuncionarioFornecedor(int id, FuncionarioFornecedor entity)
        {
            if (id != entity.FuncFornecedorId)
            {
                return BadRequest();
            }

            FuncionarioFornecedor funcionarioFornecedor = await _funcionarioFornecedoresRepository.GetById(entity.FuncFornecedorId);
            if (funcionarioFornecedor == null) return NotFound($"Funcionario com o ID {entity.FuncFornecedorId} não existe!");

            funcionarioFornecedor = await _funcionarioFornecedoresRepository.Update(entity, id);

            return Ok(funcionarioFornecedor);
        }

        // POST: api/FuncionarioFornecedors
        [HttpPost("register"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<FuncionarioFornecedor>> PostFuncionarioFornecedor(FuncionarioFornecedor entity)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(entity.FuncFornecedorPassword);

            entity.FuncFornecedorPassword = hashedPassword;

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _funcionarioFornecedoresRepository.GetById(entity.FuncFornecedorId)) != null) return StatusCode(500, $"FuncionarioFornecedor com o id {entity.FuncFornecedorId} já existe!");


            await _funcionarioFornecedoresRepository.Insert(entity);

            return CreatedAtAction("GetFuncionarioFornecedor", new { id = entity.FuncFornecedorId }, entity);
        }

        // DELETE: api/FuncionarioFornecedores/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteFuncionarioFornecedor(int id)
        {
            FuncionarioFornecedor funcionarioFornecedor = await _funcionarioFornecedoresRepository.GetById(id);
            if (funcionarioFornecedor == null) return NotFound($"FuncionarioFornecedor com o ID {id} não existe!");

            bool apagado = await _funcionarioFornecedoresRepository.DeleteById(id);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDTO user)
        {
            try
            {
                FuncionarioFornecedor funcionarioFornecedor = await _funcionarioFornecedoresRepository.GetById(user.ID);
                if (funcionarioFornecedor == null) return NotFound($"Funcionario com o ID {user.ID} não existe!");

                if (!BCrypt.Net.BCrypt.Verify(user.Password, funcionarioFornecedor.FuncFornecedorPassword)) return StatusCode(401, "Password incorreta!");

                var token = new { Token = $"{_jWTService.GenerateToken<FuncionarioFornecedor>(funcionarioFornecedor)}" };

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
