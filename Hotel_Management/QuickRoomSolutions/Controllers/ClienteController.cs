using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Services;
using System.Drawing.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClientesRepository<Cliente> _clientesRepository;
        private readonly JWTService _jWTService;

        public ClienteController(IClientesRepository<Cliente> clientesRepository, JWTService jWTService)
        {
            _clientesRepository = clientesRepository;
            _jWTService = jWTService;
        }


        // GET: api/Clientes
        [HttpGet("GetAllClientes"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            List<Cliente> clientes = await _clientesRepository.GetAll();
            if (clientes.Count == 0) return StatusCode(204,"Sem clientes registados!");
            return Ok(clientes);
        }

        // GET: api/Clientes/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            Cliente cliente = await _clientesRepository.GetbyId(id);
            if (cliente == null) return NotFound($"Cliente com o {id} não existe!");
            return Ok(cliente);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}"),Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cliente>> PutCliente(int id, Cliente entity)
        {
            if (id != entity.ClienteId)
            {
                return BadRequest();
            }

            Cliente cliente = await _clientesRepository.GetbyId(entity.ClienteId);
            if (cliente == null) return NotFound($"Cliente com o ID {entity.ClienteId} não existe!");

            cliente = await _clientesRepository.Update(entity, id);

            return Ok(cliente);
        }

        // POST: api/Clientes
        [HttpPost("register")]
        public async Task<ActionResult<Cliente>>RegistarCliente(Cliente entity)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(entity.ClientPassword);

            entity.ClientPassword = passwordHash;

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _clientesRepository.GetbyId(entity.ClienteId)) != null) return StatusCode(500, $"Cliente com o id {entity.ClienteId} já existe!");

            if (entity.PessoaPessoa == null)
            {
                entity = await _clientesRepository.AssociarPessoaAoRegisto(entity);
            }
           
            await _clientesRepository.Insert(entity);

            return CreatedAtAction("GetCliente", new { id = entity.ClienteId }, entity);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteCliente(int id)
        {
            Cliente cliente = await _clientesRepository.GetbyId(id);
            if (cliente == null) return NotFound($"Cliente com o ID {id} não existe!");

            bool apagado = await _clientesRepository.DeleteById(id);

            return Ok();
        }

        // POST: api/Clientes/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDTO user)
        {
            try
            { 
            Cliente cliente = await _clientesRepository.GetbyId(user.ID);
            if (cliente == null) return NotFound($"Cliente com o ID {user.ID} não existe!");

            if (!BCrypt.Net.BCrypt.Verify(user.Password, cliente.ClientPassword)) return StatusCode(401, "Password incorreta!");

            var token = new { Token = $"{_jWTService.GenerateToken<Cliente>(cliente)}" };

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
