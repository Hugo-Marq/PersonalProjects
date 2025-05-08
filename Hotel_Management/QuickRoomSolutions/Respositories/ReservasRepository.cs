using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;



namespace QuickRoomSolutions.Respositories
{
    public class ReservasRepository : IReservasRepository<Reserva>
    {
        private readonly QuickRoomSolutionDatabaseContext _dbContext;
        private readonly IBaseQuartoRepository<Quarto> _quartoRepository;
        private readonly ITipologiasRepository<Tipologia> _tipologiasRepository;

        public ReservasRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDatabaseContext, IBaseQuartoRepository<Quarto> baseQuartoRepository, ITipologiasRepository<Tipologia> tipologiasRepository)
        {
            _dbContext = quickRoomSolutionDatabaseContext;
            _quartoRepository = baseQuartoRepository;
            _tipologiasRepository = tipologiasRepository;
        }

        public async Task<List<Reserva>> GetAll()
        {
            return await _dbContext.Reservas.ToListAsync();
        }

        public async Task<Reserva> GetbyId(int id)
        {
            return await _dbContext.Reservas.FirstOrDefaultAsync(reserva => reserva.ReservaId == id);
            
        }

        public async Task<List<Reserva>> GetReservasWaitingCheckIn()
        {
            return await _dbContext.Reservas.Where(reserva => reserva.CheckIn == false).ToListAsync();
        }

        public async Task<List<Reserva>> GetReservasWaitingCheckOut()
        {
            return await _dbContext.Reservas.Where(reserva => reserva.CheckIn == true && reserva.CheckOut == false).ToListAsync();
        }

        public async Task<Reserva> Insert(Reserva entity)
        {
            entity.CheckIn = false;
            entity.CheckOut = false;
            entity.DataCheckIn = null;
            entity.DataCheckOut = null;
            entity.EstadoReserva = ReservaEstado.Ativa;


            await _dbContext.Reservas.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Reserva> Update(Reserva entity,int id)
        {
            Reserva reserva = await GetbyId(id);

            if (reserva == null)
            {
                throw new Exception($"Reserva {id} não foi encontrada!! ");
            }

            
            reserva.QuartoQuartoId = entity.QuartoQuartoId;
            reserva.ClienteClienteId = entity.ClienteClienteId;
            reserva.DataInicio = entity.DataInicio;
            reserva.DataFim = entity.DataFim;
            reserva.NumeroCartao = entity.NumeroCartao;
            reserva.EstadoReserva = entity.EstadoReserva;
            //reserva.PrecoTotal = entity.PrecoTotal;
            reserva.CheckIn = entity.CheckIn;
            reserva.CheckOut = entity.CheckOut;
            reserva.DataCheckIn = entity.DataCheckIn;
            reserva.DataCheckOut = entity.DataCheckOut;


            _dbContext.Reservas.Update(reserva);
           await  _dbContext.SaveChangesAsync();

            return reserva;

        }
        
        public async Task<Reserva> DoCheckout( int reservaId)
        {
            Reserva reserva = await GetbyId(reservaId);

            if (reserva == null)
            {
                return null;
            }
            if(reserva.CheckIn == false) throw new InvalidOperationException($"Reserva {reservaId} sem check-in efectuado");
            if (reserva.CheckOut == true) throw new InvalidOperationException($"Reserva {reservaId} com check-out já efectuado");

            reserva.CheckOut = true;
            reserva.DataCheckOut = DateTime.Now;
            reserva.EstadoReserva = ReservaEstado.Finalizada;

            _dbContext.Reservas.Update(reserva);
            await _dbContext.SaveChangesAsync();

            return (reserva);
        }

        public async Task<Reserva> CancelarReserva(int reservaId)
        {
            Reserva reserva = await GetbyId(reservaId);

            if (reserva == null)
            {
                return null;
            }

            //VErifica se a reserva nao foi finalizada, incializada ou cancelada
            if (reserva.EstadoReserva == ReservaEstado.Finalizada) throw new InvalidOperationException($"Reserva {reservaId} com check-out já efectuado");
            if (reserva.EstadoReserva == ReservaEstado.Iniciada) throw new InvalidOperationException($"Reserva {reservaId} com check-in já efectuado");
            if (reserva.EstadoReserva == ReservaEstado.Cancelada) throw new InvalidOperationException($"Reserva {reservaId} já foi cancelada");

            //Compara a data de agora com a data de inicio da reserva
            TimeSpan dataDiference = reserva.DataInicio - DateTime.Now;
            
            // se faltar menos de 24Horas retorna a exeção
            if(!(dataDiference.TotalHours>=24)) throw new InvalidOperationException($" Impossivel cancelar reserva {reservaId} sem penalizações, falta menos de 24 horas para o check-in!");

            //Se faltar mais procede ao cancelamento
            reserva.EstadoReserva = ReservaEstado.Cancelada;
            _dbContext.Reservas.Update(reserva);
            await _dbContext.SaveChangesAsync();

            return (reserva);
        }

        public async Task<List<Reserva>> GetReservasQuartoId(string id)
        {
            List<Reserva> reservas = await _dbContext.Reservas.Where(reserva => reserva.QuartoQuartoId == id).ToListAsync();

            if (reservas == null)
            {
                throw new Exception($"Não existem reservas para o quarto {id}");
            }

            return reservas;
        }

        public async Task<bool> DeleteById(int id)
        {
            Reserva reserva = await GetbyId(id);

            if (reserva == null)
            {
                throw new Exception($"Reserva {id} não foi encontrada!! ");
            }

            _dbContext.Reservas.Remove( reserva );
           await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> NumQuartosIndisponiveis(int id, DateTime inicio, DateTime fim)//sujeito a alteração, aguarda aprovação
        {
            List<Reserva> reservas = _dbContext.Reservas.ToList();
            List<Quarto> Tipo = await GetQuartosByTipologia(id);
            int numTipos = Tipo.Count;
            if (numTipos == 0)
            {
                return -1;//rever
            }
            int quartosIndisponiveis = 0;
            foreach (var reserva in reservas)
            {
                if (inicio < reserva.DataFim && reserva.DataInicio < fim && reserva.QuartoQuarto.TipologiaTipologiaId == id)
                {
                    quartosIndisponiveis++;
                }
            }
            foreach (var quarto in Tipo)
            {
                if (quarto.QuartoEstado == QuartoEstados.Indisponivel || quarto.QuartoEstado == QuartoEstados.Manutencao) //ponderar remover manutencao
                {
                    quartosIndisponiveis++;
                }
            }
            return quartosIndisponiveis;
        }

        public Task<List<Quarto>> GetQuartosByTipologia(int id)
        {
            List<Quarto> quartos = _dbContext.Quartos.ToList();
            List<Quarto> Tipo = quartos.Where(quarto => quarto.TipologiaTipologiaId == id).ToList();

            return Task.FromResult(Tipo);
        }

        public async Task<bool> PodeReservar(ReservaDTO entityDTO)
        {
            List<Quarto> Tipo = await GetQuartosByTipologia(entityDTO.TipologiaId);//sujeito a alteração, aguarda aprovação
            int totais = Tipo.Count;
            int indisponiveis = await NumQuartosIndisponiveis(entityDTO.TipologiaId, entityDTO.DataInicio, entityDTO.DataFim);
            if (!await CompararDatas(entityDTO.DataInicio, entityDTO.DataFim))
            {
                throw new InvalidOperationException("Período de reserva inválido");
            }
            if (indisponiveis >= totais)
            {
                throw new Exception($"Quartos do tipo {entityDTO.TipologiaId} lotados. Impossível reservar");//sujeito a alteração, aguarda aprovação
            }
            return true;
        }

        public Task<bool> CompararDatas(DateTime inicio, DateTime fim)
        {
            if (inicio.CompareTo(fim) > 0)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        public Task<bool> ReservasSobrepõem(Reserva reserva, DateTime inicio, DateTime fim)
        {
            return Task.FromResult(inicio < reserva.DataFim && reserva.DataInicio < fim);
        }
       
        public async Task<Reserva> DoCheckIn(int reservaId)
        {
            Reserva reserva = await GetbyId(reservaId);
            if (reserva == null)
            {
                return null;
            }
            if (reserva.CheckIn == true) throw new InvalidOperationException($"Reserva {reservaId} com check-in já efectuado");
            if (reserva.CheckOut == true) throw new InvalidOperationException($"Reserva {reservaId} com check-out já efectuado");
            reserva.CheckIn = true;
            reserva.DataCheckIn = DateTime.Now;
            reserva.EstadoReserva = ReservaEstado.Iniciada;
            _dbContext.Reservas.Update(reserva);
            await _dbContext.SaveChangesAsync();
            return (reserva);
        }

        public Reserva MapReserva(ReservaDTO reservaDTO)
        {
            Reserva reserva = new Reserva
            {
                DataInicio = reservaDTO.DataInicio,
                DataFim = reservaDTO.DataFim,
                NumeroCartao = reservaDTO.NumeroCartao,
                EstadoReserva = reservaDTO.EstadoReserva,
                CheckIn = reservaDTO.CheckIn,
                DataCheckIn = reservaDTO.DataCheckIn,
                CheckOut = reservaDTO.CheckOut,
                DataCheckOut = reservaDTO.DataCheckOut,
                TipologiaId = reservaDTO.TipologiaId,
                QuartoQuartoId = reservaDTO.QuartoId,
                ClienteClienteId = reservaDTO.ClienteClienteId
            };

            return reserva;
        }

        public async Task<float> CalcularPrecoReserva(Reserva reserva)
        {

            Tipologia tipologiaQuarto = await _tipologiasRepository.GetById(reserva.TipologiaId);

            if (tipologiaQuarto == null)
            {
                throw new Exception("Tipologia do quarto não encontrada.");
            }


            float precoDiario = tipologiaQuarto.Preco;


            TimeSpan duracaoReserva = reserva.DataFim - reserva.DataInicio;
            int numeroDiasReserva = (int)duracaoReserva.TotalDays;

            //if (numeroDiasReserva <= 0)
            //{
            //    throw new Exception("A reserva deve ter uma duração mínima de um dia.");
            //}


            float precoTotal = precoDiario * numeroDiasReserva;

            return precoTotal;
        }

        public async Task<string> AtribuirQuartoAUmaReserva(ReservaDTO reservaDTO)
        {
            //obter lista de quartos de tipologia pretendida
            List<Quarto> Tipo = await GetQuartosByTipologia(reservaDTO.TipologiaId);

            //obter lista de quartos disponíveis
            List<Quarto> disponiveis = Tipo.Where(quarto => quarto.QuartoEstado != QuartoEstados.Indisponivel && quarto.QuartoEstado != QuartoEstados.Manutencao).ToList();

            //obter lista de reservas que se sobrepõem ao período da reserva pretendida
            List<Reserva> reservas = await _dbContext.Reservas.Where(reserva => reserva.DataFim > reservaDTO.DataInicio
                                        && reserva.DataInicio < reservaDTO.DataFim).ToListAsync();

            //obter lista de quartos (por ID) disponíveis
            List<string?> quartosDisponiveis = disponiveis.Select(item => item.QuartoId)
                             .Except(reservas.Select(item => item.QuartoQuartoId)).ToList();
            //se não houver quartos disponíveis, lançar exceção
            if (quartosDisponiveis.Count == 0)
            {
                throw new Exception("Não há quartos disponíveis para a tipologia pretendida.");
            }

            //atribuir um quarto disponível à reserva
            string? quartoAtribuido = quartosDisponiveis[0];

            return quartoAtribuido;
        }

        public async Task<(Reserva, bool)> AtualizarQuartoDaReserva(Reserva reserva, Quarto novoQuarto)
        {
            Quarto quarto = await _quartoRepository.GetbyId(reserva.QuartoQuartoId);
            bool needLimpeza = false;

            if(!(quarto.QuartoEstado == QuartoEstados.Manutencao || quarto.QuartoEstado == QuartoEstados.Indisponivel))
            {
                //estado do quarto conforme check-in
            if(reserva.CheckIn == true)
            {
                quarto.QuartoEstado = QuartoEstados.Limpeza;
                needLimpeza = true;
            }
            else
            {
                quarto.QuartoEstado = QuartoEstados.Disponivel;
            }
            _dbContext.Quartos.Update(quarto);
            }
            
            novoQuarto.QuartoEstado = QuartoEstados.Ocupado;
            _dbContext.Quartos.Update(novoQuarto);

            //se a tipologia do novo quarto for inferior à da reserva, atualizar tipologia da reserva
            if (novoQuarto.TipologiaTipologiaId < reserva.TipologiaId)
            {
                reserva.TipologiaId = novoQuarto.TipologiaTipologiaId;
            }
            reserva.QuartoQuartoId = novoQuarto.QuartoId;
            _dbContext.Reservas.Update(reserva);
            await _dbContext.SaveChangesAsync();
            return (reserva, needLimpeza);
        }

        public async Task<List<Reserva>> GetReservasAtuaisByTipologia(int id)
        {
            DateTime dataAtual = DateTime.Now;
            List<Reserva> listReservas = await _dbContext.Reservas
                .Where(reserva => reserva.TipologiaId == id && reserva.DataInicio <= dataAtual && reserva.DataFim >= dataAtual && reserva.CheckOut == false)
                .ToListAsync();

            if (listReservas == null)
            {
                throw new Exception($"Não existem reservas para a tipologia {id}");
            }

            return listReservas;
        }
    }



    
}
