using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Notificacoes;
using Microsoft.AspNetCore.Authorization;

namespace QuickRoomSolutions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservaController : ControllerBase
    {
        private readonly IReservasRepository<Reserva> _reservaRepository;
        private readonly ITipologiasRepository<Tipologia> _tipologiasRepository;
        private readonly IBaseQuartoRepository<Quarto> _quartoRepository;
        private readonly IClientesRepository<Cliente> _clientesRepository;
        private readonly IPessoasRepository<Pessoa> _pessoasRepository;
        private readonly ITicketLimpezaRepository<TicketLimpeza> _ticketLimpezaRepository;

        public ReservaController(IReservasRepository<Reserva> reservaRepository, ITipologiasRepository<Tipologia> tipologiasRepository, IBaseQuartoRepository<Quarto> quartoRepository, ITicketLimpezaRepository<TicketLimpeza> ticketLimpezaRepository, IClientesRepository<Cliente> clienteRepository, IPessoasRepository<Pessoa> pessoasRepository)
         {
            _reservaRepository = reservaRepository;
            _tipologiasRepository = tipologiasRepository;
            _quartoRepository = quartoRepository;
            _ticketLimpezaRepository = ticketLimpezaRepository;
            _clientesRepository = clienteRepository;
            _pessoasRepository = pessoasRepository;
        }

        [HttpGet, Authorize(Roles = "Gerente")]
        public async Task<ActionResult<List<Reserva>>> GetAllReservas()
        {
            List<Reserva> reservas = await _reservaRepository.GetAll();
            return Ok(reservas);
        }

        // metodo a devolver lista de reservas por tipologia á dáta de hoje
        [HttpGet("tipologia/{tipologiaId}"), Authorize(Roles = "Gerente , Rececionista")]
        public async Task<ActionResult<List<Reserva>>> GetReservasByTipologia(int tipologiaId)
        {
            try
            {
                List<Reserva> reservas = await _reservaRepository.GetReservasAtuaisByTipologia(tipologiaId);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("Reservas-Waiting-CheckIn")]
        public async Task<ActionResult<List<Reserva>>> GetReservasWaitingCheckIn()
        {
            try
            {
                List<Reserva> reservas = await _reservaRepository.GetReservasWaitingCheckIn();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("Reservas-Waiting-CheckOut")]
        public async Task<ActionResult<List<Reserva>>> GetReservasWaitingCheckOut()
        {
            try
            {
                List<Reserva> reservas = await _reservaRepository.GetReservasWaitingCheckOut();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Reserva>> GetReservabyId(int id)
        {
            Reserva reserva = await _reservaRepository.GetbyId(id);
            return Ok(reserva);
        }

        [HttpPost, Authorize(Roles ="Cliente, Gerente, Rececionista")]

        public async Task<ActionResult<Reserva>> InsertReserva([FromBody] ReservaDTO entityDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //subtrair data de fim por data de inicio e verificar se é maior que 1
            if(entityDTO.DataFim.Subtract(entityDTO.DataInicio).Days < 1)
            {
                return BadRequest("Periodo de Reserva tem de ser no mínimo de 1 dia!");
            }
            try
            {
                bool quartoDisponivel = await _reservaRepository.PodeReservar(entityDTO);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)

            {
                return BadRequest(ex.Message);
            }
            try
            {
                entityDTO.QuartoId = await _reservaRepository.AtribuirQuartoAUmaReserva(entityDTO);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            Cliente cliente = await _clientesRepository.GetbyId(entityDTO.ClienteClienteId);
            if (cliente == null)
            {
                return NotFound("Cliente não encontrado");
            }

            Pessoa pessoaCliente = await _pessoasRepository.GetbyId(cliente.PessoaNif);
            Reserva reserva = _reservaRepository.MapReserva(entityDTO);
            reserva = await _reservaRepository.Insert(reserva);
            
            Notificacoes.Notificacoes.EnviarLembreteConfirmacao(pessoaCliente.Email, "Reserva Concluida",MensagensNotificacoes.MensagensNotificacoes.CriarMensagemConfirmacaoReserva(entityDTO, pessoaCliente));
            return Ok(reserva);
        }

        [HttpPut("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Reserva>> UpdateReserva([FromBody] ReservaDTO entityDTO, int id)
        {
            entityDTO.ReservaId = id;

            Reserva reserva = await _reservaRepository.Update(_reservaRepository.MapReserva(entityDTO), id);
            return Ok(reserva);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Gerente")]
        public async Task<ActionResult<Reserva>> DeleteReserva(int id)
        {
            bool apagado = await _reservaRepository.DeleteById(id);
            return Ok(apagado);
        }

        [HttpPut("DoCheckout"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Reserva>> DoCheckout(int id)
        {
            try
            {
                Reserva reserva = await _reservaRepository.DoCheckout(id);
                if (reserva == null) return NotFound($"Reserva {id} não existe!");
                if( await _quartoRepository.AtualizarEstadoQuarto(reserva.QuartoQuartoId, 5)==null) return NotFound($"Quarto {reserva.QuartoQuartoId} associado à reserva {id} não encontrado na base de dados");
                await _ticketLimpezaRepository.CriarTicketLimpezaParaCheckout(reserva.QuartoQuartoId);

                return Ok(reserva); 
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(NotSupportedException ex  )
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("DoCheckin"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Reserva>> DoCheckin(int id)
        {

            try
            {
                Reserva reserva = await _reservaRepository.DoCheckIn(id);
                if (reserva == null) return NotFound($"Reserva {id} não existe!");
                if ((await _quartoRepository.AtualizarEstadoQuarto(reserva.QuartoQuartoId, 3)) == null) return NotFound($"Quarto {reserva.QuartoQuartoId} associado à reserva {id} não encontrado na base de dados");

                return Ok(reserva);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("CancelarReserva"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Reserva>> CancelarReserva(int reservaId)
        {
            try
            {
                Reserva reserva = await _reservaRepository.CancelarReserva(reservaId);
                if (reserva == null) return NotFound($"Reserva {reservaId} não existe!");

                return Ok(reserva);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("preco-total"), Authorize(Roles = "Gerente")] // debater se é melhor usar um DTO ou não
        public async Task<ActionResult<ReservaDTO>> GetReservasComPrecoTotal()
        {

            List<Reserva> reservas = await _reservaRepository.GetAll();

            List<ReservaDTO> reservasComPrecoTotal = new List<ReservaDTO>();


            foreach (var reserva in reservas)
            {
                float precoTotal;
                try 
                {
                    precoTotal = await _reservaRepository.CalcularPrecoReserva(reserva); //rever, caso o preço esteja a ser calculado incorretamente
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                ReservaDTO reservaDTO = new ReservaDTO
                {
                    ReservaId = reserva.ReservaId,
                    DataInicio = reserva.DataInicio,
                    DataFim = reserva.DataFim,
                    NumeroCartao = reserva.NumeroCartao,
                    EstadoReserva = reserva.EstadoReserva,
                    CheckIn = reserva.CheckIn,
                    DataCheckIn = reserva.DataCheckIn,
                    CheckOut = reserva.CheckOut,
                    DataCheckOut = reserva.DataCheckOut,
                    TipologiaId = reserva.TipologiaId,
                    ClienteClienteId = reserva.ClienteClienteId,
                    PrecoTotal = precoTotal
                };

                reservasComPrecoTotal.Add(reservaDTO);
            }
            return Ok(reservasComPrecoTotal);
        }

        [HttpPut("TrocarQuarto"), Authorize(Roles = "Gerente, Rececionista")]
        public async Task<ActionResult<Reserva>> TrocarQuarto(int reservaId, string novoQuartoId)
        {
            Reserva reserva = await _reservaRepository.GetbyId(reservaId);
            Reserva newReserva = new Reserva();
            if (reserva == null)
            {
                return NotFound($"A reserva nº{reservaId} não existe!");
            }

            Quarto quarto = await _quartoRepository.GetbyId(novoQuartoId);
            if (quarto == null)
            {
                return NotFound($"Quarto com o {novoQuartoId} não existe!");
            }

            bool needLimpeza = false;
            (newReserva,needLimpeza) = await _reservaRepository.AtualizarQuartoDaReserva(reserva, quarto);
            if (needLimpeza) await _ticketLimpezaRepository.CriarTicketLimpezaParaCheckout(reserva.QuartoQuartoId);
            return Ok(newReserva);


        }
    }
}
