using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickRoomSolutions.Respositories
{
    public class QuartoRepository : IBaseQuartoRepository<Quarto>
    {
        private readonly QuickRoomSolutionDatabaseContext _DBContext;

        public QuartoRepository(QuickRoomSolutionDatabaseContext quickRoomSolutionDBContext) 
        {
            _DBContext = quickRoomSolutionDBContext;
        }


        public async Task<Quarto> Insert(Quarto quarto)
        {
            string novoQuartoId = await GerarQuartoId(quarto.Bloco, quarto.Piso, quarto.Porta);

            if (await ExisteQuartoId(novoQuartoId))
            {
                throw new Exception($"Já existe um quarto com o ID {novoQuartoId}.");
            }
                  
            quarto.QuartoId = novoQuartoId;

            await _DBContext.Quartos.AddAsync(quarto);
               
            await _DBContext.SaveChangesAsync();
                
            return quarto; 
        }


        public async Task<Quarto> Update(Quarto quarto, string id)
        {
            Quarto quartoPorId = await GetbyId(id);

            if (quartoPorId == null)
            {
                throw new Exception($"Quarto com ID {id} não encontrado.");
            }

            string novoQuartoId = await GerarQuartoId(quarto.Bloco, quarto.Piso, quarto.Porta);

            if (await ExisteQuartoId(novoQuartoId))
            {
                throw new Exception($"Já existe um quarto com o ID {novoQuartoId}.");
            }

            // Atualiza os dados do quarto existente com os dados do quarto recebido
            quartoPorId.Bloco = quarto.Bloco;

            quartoPorId.Piso = quarto.Piso;

            quartoPorId.Porta = quarto.Porta;

            quartoPorId.QuartoId = novoQuartoId;

            _DBContext.Quartos.Update(quartoPorId);

            await _DBContext.SaveChangesAsync();

            return quartoPorId;
        }

        public async Task<bool> DeleteById(string id)
        {
            Quarto quartoPorId = await GetbyId(id);

            if (quartoPorId == null)
            {
                throw new Exception($"Quarto com ID {id} não encontrado.");
            }

            // falta implementar logica para verificar estado do quarto antes de eliminar

            _DBContext.Quartos.Remove(quartoPorId);

            await _DBContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<Quarto>> GetAll()
        {
            return await _DBContext.Quartos.ToListAsync();
        }

        public async Task<Quarto> GetbyId(string id)
        {
            return await _DBContext.Quartos.FirstOrDefaultAsync(x => x.QuartoId == id);
        }

        public async Task<bool> ExisteQuartoId(string id)
        {
            // Verifica se existe algum quarto com o ID especificado
            var quartoExiste = await _DBContext.Quartos.AnyAsync(x => x.QuartoId == id);

            return quartoExiste;
        }
        public async Task<string> GerarQuartoId(string bloco, int piso, int numero)
        {
            // O identificador é composto pelo bloco, piso e número do quarto.
            return $"{bloco}-{piso:00}-{numero:00}";
        }


        /// <summary>
        /// Atualiza o estado de um quarto com base no identificador do quarto e no estado fornecido.
        /// </summary>
        /// <param name="quartoId">O identificador único do quarto.</param>
        /// <param name="estado">O estado para atribuir ao quarto.</param>
        /// <returns>O objeto Quarto após a atualização do estado.</returns>
        /// <exception cref="InvalidOperationException">Exceção lançada quando o estado fornecido não é reconhecido.</exception>
        public async Task<Quarto> AtualizarEstadoQuarto(string quartoId, int estado)
        {
            // Obtém o quarto pelo seu ID
             Quarto quarto = await GetbyId(quartoId);

            // Verifica se o quarto não é nulo
            if (quarto == null)
            {
                return null;
            }

            // Atualiza o estado do quarto com base no valor fornecido
            switch (estado)
            {
                case 1:
                    quarto.AlterarEstado(QuartoEstados.Disponivel);
                    break;
                case 2:
                    quarto.AlterarEstado(QuartoEstados.Manutencao);
                    break;
                case 3:
                    quarto.AlterarEstado(QuartoEstados.Ocupado);
                    break;
                case 4:
                    quarto.AlterarEstado(QuartoEstados.Indisponivel);
                    break;
                case 5:
                    quarto.AlterarEstado(QuartoEstados.Limpeza);
                    break;
                default:
                    throw new InvalidOperationException($"Estado {estado} não encontrado.");
            }

            // Atualiza o quarto na base de dados
            _DBContext.Quartos.Update(quarto);

            await _DBContext.SaveChangesAsync();

            return quarto;
        }

        public async Task UpdateQuartoAsync(Quarto quarto)
        {
            _DBContext.Entry(quarto).State = EntityState.Modified;

            try
            {
                await _DBContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (NullReferenceException)
            {
                throw;
            }
        }

        public async Task<QuartoEstados> GetEstadoQuarto(string id)
        {
            Quarto quarto = await GetbyId(id);
            if (quarto == null)
            {
                throw new Exception($"Quarto com ID {id} não encontrado.");
            }
            return quarto.QuartoEstado;
        }
        public int GetTipologiaIdByQuartoId(string tipo)//aguarda aprovação
        {
            List<Quarto> quartos = _DBContext.Quartos.ToList();
            foreach (var quarto in quartos)
            {
                if (quarto.QuartoId == tipo)
                {
                    return quarto.TipologiaTipologiaId;
                }
            }
            return -1;
        }

        public string GetQuartoIdByTipologiaId(int id)//aguarda aprovação
        {
            List<Quarto> quartos = _DBContext.Quartos.ToList();
            foreach (var quarto in quartos)
            {
                if (quarto.TipologiaTipologiaId == id)
                {
                    return quarto.QuartoId;
                }
            }
            return null;
        }

        public bool DisponibilidadeQuarto(string id, DateTime inicio, DateTime fim)
        {
            Reserva reserva = _DBContext.Reservas.FirstOrDefault(x => x.QuartoQuartoId == id);
            if (reserva == null)
            {
                return true;
            }
            if (reserva.DataInicio <= fim && reserva.DataFim >= inicio)
            {
                return false;
            }
            return true;
        }

        public async Task<List<Quarto>> QuartosLivresNoPeriodoReserva(int reservaId)
        {
            Reserva reserva = _DBContext.Reservas.FirstOrDefault(x => x.ReservaId == reservaId);
            if (reserva == null)
            {
                throw new Exception($"Reserva com ID {reservaId} não encontrada.");
            }

            //Quartos não Ocupados nem em Manutenção
            List<Quarto> quartosLivres = await GetAllQuartosLivres();

            //Reservas que se sobrepõem à reserva atual
            List<Reserva> reservas = await _DBContext.Reservas
                .Where(x => x.DataFim > reserva.DataInicio && x.DataInicio < reserva.DataFim)
                .ToListAsync();

            //ID's de quartos filtrados disponiveis e não reservados
            List<string> quartosIds = quartosLivres.Select(item => item.QuartoId)
                             .Except(reservas.Select(item => item.QuartoQuartoId))
                             .ToList();

            List<Quarto> quartosDisponiveis = quartosLivres
                            .Where(quarto => quartosIds.Contains(quarto.QuartoId))
                            .ToList();

            return quartosDisponiveis;
        }

        public async Task<List<Quarto>> GetAllQuartosLivres()
        {
            return await _DBContext.Quartos
                .Where(quarto => quarto.QuartoEstado != QuartoEstados.Indisponivel &&
                                 quarto.QuartoEstado != QuartoEstados.Manutencao)
                .ToListAsync();
        }

        public async Task<bool> CanBook(int id, DateTime inicio, DateTime fim)
        {
            // veridfica se existem reservas para a tipologia no periodo selecionado
            List<Reserva> reservas = await _DBContext.Reservas
               .Where(x => x.DataFim > inicio && x.DataInicio < fim && x.TipologiaId == id)
               .ToListAsync();

            if (reservas.Count == 0)
            {
                return true;
            }

            List<Quarto> quartos = await _DBContext.Quartos
                .Where(x => x.TipologiaTipologiaId == id)
                .ToListAsync();
            if (quartos.Count > reservas.Count)
            {
                return true;
            }
            return false;

        }
    }
}
