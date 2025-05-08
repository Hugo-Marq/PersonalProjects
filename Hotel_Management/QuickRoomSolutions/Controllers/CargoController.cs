using System;
using System.Collections.Generic;
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
    public class CargoController : ControllerBase
    {
        private readonly ICargoRepository<Cargo> _cargoRepository;

        public CargoController(ICargoRepository<Cargo> cargoRepository)
        {
            _cargoRepository = cargoRepository;
        }


        // GET: api/Cargo
        [HttpGet, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<IEnumerable<Cargo>>> GetCargos()
        {
            List<Cargo> cargos = await _cargoRepository.GetAll();
            if (cargos.Count == 0) return StatusCode(204, "Sem cargos registados!");
            return Ok(cargos);

        }

        // GET: api/Cargo/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cargo>> GetCargo(int id)
        {
            Cargo cargo = await _cargoRepository.GetById(id);
            if (cargo == null) return NotFound($"Cargo com o {id} não existe!");
            return Ok(cargo);
        }

        // PUT: api/Cargo/5
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cargo>> PutCargo(int id, Cargo entity)
        {

            if (id != entity.CargoId)
            {
                return BadRequest();
            }

            Cargo cargo = await _cargoRepository.GetById(entity.CargoId);
            if (cargo == null) return NotFound($"Cliente com o ID {entity.CargoId} não existe!");

            cargo = await _cargoRepository.Update(entity, id);

            return Ok(cargo);
        }

        // POST: api/Cargo
        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cargo>> PostCargo(Cargo cargo)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _cargoRepository.GetById(cargo.CargoId)) != null) return StatusCode(500, $"Cargo com o id {cargo.CargoId} já existe!");


            await _cargoRepository.Insert(cargo);

            
            return CreatedAtAction("GetCargo", new { id = cargo.CargoId }, cargo);
        }

        // DELETE: api/Cargo/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Cargo>> DeleteCargo(int id)
        {

            Cargo cargo = await _cargoRepository.GetById(id);
            if (cargo == null) return NotFound($"Cargo com o ID {id} não existe!");


            bool apagado = await _cargoRepository.DeleteById(id);

            return Ok();

        }

    }
}
