using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuartosController : ControllerBase
    {
        private readonly ITipologiasRepository<Tipologia> _tipologiasRepository;
        private readonly IBaseQuartoRepository<Quarto> _quartosRepository;
        private readonly QuickRoomSolutionDatabaseContext _context;

        public QuartosController(IBaseQuartoRepository<Quarto> quartosRepository, ITipologiasRepository<Tipologia> tipologiasRepository)
        {
            _quartosRepository = quartosRepository;
            _tipologiasRepository = tipologiasRepository;
        }

        // GET: api/Quartos
        [HttpGet, Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<List<Quarto>>> GetQuartos()
        {
            List<Quarto> quartos = await _quartosRepository.GetAll();
            if (quartos.Count == 0)
                return NotFound("Sem quartos registados!");

            return Ok(quartos);
        }

        // GET: api/Quartos/5
        [HttpGet("{id}"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Quarto>> GetQuarto(string id)
        {
            Quarto quarto = await _quartosRepository.GetbyId(id);
            if (quarto == null)
                return NotFound($"Quarto com o {id} não existe!");

            return Ok(quarto);
        }

        // PUT: api/Quartos/5
        [HttpPut("{id}"),Authorize(Roles = "Gerente")]
        public async Task<ActionResult> PutQuarto(string id, Quarto quarto)
        {
            if (id != quarto.QuartoId)
            {
                return BadRequest();
            }
            try
            {
                await _quartosRepository.UpdateQuartoAsync(quarto);
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }
            catch (NullReferenceException)
            {
                return NotFound("Quarto não encontrado");
            }
        }


        // POST: api/Quartos
        [HttpPost, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Quarto>> PostQuarto(Quarto quarto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _quartosRepository.GetbyId(quarto.QuartoId) != null)
            {
                return Conflict($"Quarto com o id {quarto.QuartoId} já existe!");
            }

            await _quartosRepository.Insert(quarto);

            return CreatedAtAction("GetQuarto", new { id = quarto.QuartoId }, quarto);
        }

        // DELETE: api/Quartos/5
        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> DeleteCliente(string id)
        {
            Quarto quarto = await _quartosRepository.GetbyId(id);
            if (quarto == null)
            {
                return NotFound($"Quarto com o ID {id} não existe!");
            }

            bool apagado = await _quartosRepository.DeleteById(id);

            return Ok($"Quarto com o ID {id} removido!");
        }

        [HttpGet("Disponibilidade-Quarto"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult> GetDisponibilidadeQuarto(string id, DateTime inicio, DateTime fim)
        {
            Quarto quarto = await _quartosRepository.GetbyId(id);
            if (quarto == null)
            {
                return NotFound($"Quarto com o ID {id} não existe!");
            }
            if (inicio > fim)
            {
                return BadRequest("Data de início não pode ser maior que a data de fim!");
            }

            /*******************PARA REATIVAR NO RELEASE*******************/
            //else if (inicio < DateTime.Now)
            //{
            //    return BadRequest("Data de início não pode ser menor que a data atual!");
            //}
            //else if (fim < DateTime.Now)
            //{
            //    return BadRequest("Data de fim não pode ser menor que a data atual!");
            //}
            bool disponivel = _quartosRepository.DisponibilidadeQuarto(id, inicio, fim);
            if (!disponivel)
            {
                return BadRequest("Quarto não disponível para o período selecionado!");
            }

            return Ok("Disponível");
        }

        [HttpPost("Can-Book")]
        public async Task<ActionResult> GetCanBook([FromBody] CanBookDTO request)
        {
            Tipologia tipologia = await _tipologiasRepository.GetById(request.Id);
            if (tipologia == null)
            {
                return NotFound(new { message = "Tipologia inexiste!" });
            }
            if (request.Inicio > request.Fim)
            {
                return BadRequest(new { message = "Data de início não pode ser maior que a data de fim!" });
            }

            /*******************PARA REATIVAR NO RELEASE*******************/
            //else if (inicio < DateTime.Now)
            //{
            //    return BadRequest(new { message = "Data de início não pode ser menor que a data atual!" });
            //}
            //else if (fim < DateTime.Now)
            //{
            //    return BadRequest(new { message = "Data de fim não pode ser menor que a data atual!" });
            //}
            bool existe = await _quartosRepository.CanBook(request.Id, request.Inicio, request.Fim);

            return Ok(new { canBook = existe });
        }


        [HttpGet("Quartos-Livres-Para-Troca"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<List<Quarto>>> GetQuartosLivres( int reservaId)
        {
            try
            {
                List<Quarto> quartos = await _quartosRepository.QuartosLivresNoPeriodoReserva(reservaId);
                if (quartos == null || quartos.Count == 0)
                { 
                    return NotFound("Sem quartos livres, lotação esgotada!");
                }
                return Ok(quartos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
