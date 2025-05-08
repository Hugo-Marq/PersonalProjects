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
    public class TipologiasController : ControllerBase
    {
        private readonly ITipologiasRepository<Tipologia> _tipologiasRepository;

        public TipologiasController(ITipologiasRepository<Tipologia> tipologiasRepository)
        {
            _tipologiasRepository = tipologiasRepository;
        }

        // GET: api/Tipologias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tipologia>>> GetTipologia()
        {
             List<Tipologia> tipologias = await _tipologiasRepository.GetAll();
            if (tipologias.Count == 0) return StatusCode(500, "Sem tipologias registadas!");
            return Ok(tipologias);
        }

        // GET: api/Tipologias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tipologia>> GetTipologia(int id)
        {
            Tipologia tipologia = await _tipologiasRepository.GetById(id);
            if (tipologia == null) return NotFound($"Tipologia com o {id} não existe!");
            return Ok(tipologia);
        }


        // PUT: api/Tipologias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<IActionResult> PutTipologia(int id, Tipologia entity)
        {
            if (id != entity.TipologiaId)
            {
                return BadRequest();
            }

            Tipologia tipologia = await _tipologiasRepository.GetById(id);
            if (tipologia == null) return NotFound($"Tipologia com o {id} não existe!");

            tipologia = await _tipologiasRepository.Update(entity, id);

            return Ok(tipologia);
        }

        // POST: api/Tipologias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Tipologia>> PostTipologia(Tipologia entity)
        {

            //Verificar se o pedido cumpre os requesitos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((await _tipologiasRepository.GetById(entity.TipologiaId)) != null) return StatusCode(500, $"Tipologia com o id {entity.TipologiaId} já existe!");


            Tipologia tipologia = await _tipologiasRepository.Insert(entity);

            return Ok(tipologia);



        }

        // DELETE: api/Tipologias/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<IActionResult> DeleteTipologia(int id)
        {

            Tipologia tipologia  = await _tipologiasRepository.GetById(id);
            if (tipologia == null) return NotFound($"Tipologia com o ID {id} não existe!");

            bool apagado = await _tipologiasRepository.DeleteById(id);

            return Ok();

        }

    }
}
