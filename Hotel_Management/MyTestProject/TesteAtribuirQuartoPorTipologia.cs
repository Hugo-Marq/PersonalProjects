using Microsoft.EntityFrameworkCore;
using Moq;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;
using System;
using Xunit;

namespace MyTestProject
{
    public class TesteAtribuirQuartoAUmaReserva : IDisposable
    {
        private readonly ReservasRepository _repository;
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        public TesteAtribuirQuartoAUmaReserva()
        {
            var options = new DbContextOptionsBuilder<QuickRoomSolutionDatabaseContext>()
                .UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid()}").Options;

            _dbContext = new QuickRoomSolutionDatabaseContext(options);
            _dbContext.Database.EnsureCreated();

            var quartoRepositoryMock = new Mock<IBaseQuartoRepository<Quarto>>();
            var tipologiasRepositoryMock = new Mock<ITipologiasRepository<Tipologia>>();

            _repository = new ReservasRepository(_dbContext, quartoRepositoryMock.Object, tipologiasRepositoryMock.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _dbContext.Quartos.Add(new Quarto { QuartoId = "Quarto1", Bloco = "A", Piso = 1, Porta = 1, QuartoEstado = QuartoEstados.Disponivel, TipologiaTipologiaId = 1 });
            _dbContext.Quartos.Add(new Quarto { QuartoId = "Quarto2", Bloco = "B", Piso = 1, Porta = 2, QuartoEstado = QuartoEstados.Disponivel, TipologiaTipologiaId = 2 });
            // Add more Quarto objects with different tipologias

            _dbContext.Reservas.Add(new Reserva { ReservaId = 1, ClienteClienteId = 1, DataInicio = new DateTime(2024, 04, 01), DataFim = new DateTime(2024, 04, 10), QuartoQuartoId = "Quarto1", EstadoReserva = ReservaEstado.Iniciada });
            // Add reservations for different Quarto objects with various tipologias

            _dbContext.SaveChanges();
        }


        [Theory]
        [InlineData("2024-03-20", "2024-03-30", "Quarto1")]// Assuming Quarto1 is available
        [InlineData("2024-04-01", "2024-04-05", null)] // Assuming no available room 
        public async void TestAtribuirQuartoAUmaReserva(string start, string end, string expectedQuartoId)
        {
            // Arrange
            DateTime dataInicio = DateTime.Parse(start);
            DateTime dataFim = DateTime.Parse(end);
            var reservaDTO = new ReservaDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TipologiaId = 1
            };

            // Act & Assert
            if (expectedQuartoId != null)
            {
                string quartoAtribuido = await _repository.AtribuirQuartoAUmaReserva(reservaDTO);
                Assert.Equal(expectedQuartoId, quartoAtribuido);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(() => _repository.AtribuirQuartoAUmaReserva(reservaDTO));
            }
        }
        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }    
}