using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using QuickRoomSolutions.Respositories;
using Moq;


namespace MyTestProject
{
    public class TesteQuartoDisponivelPeriodo : IDisposable
    {
        private readonly QuartoRepository _repository;
        private readonly QuickRoomSolutionDatabaseContext _dbContext;

        // Constructor with initialization of in-memory DB for testing
        public TesteQuartoDisponivelPeriodo()
        {
            var options = new DbContextOptionsBuilder<QuickRoomSolutionDatabaseContext>()
                .UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid()}").Options;
            // Ensure unique in-memory DB to avoid conflicts with concurrent tests

            _dbContext = new QuickRoomSolutionDatabaseContext(options);
            _dbContext.Database.EnsureCreated(); // Ensure the creation of the in-memory DB
            _repository = new QuartoRepository(_dbContext);

            SeedDatabase(); // Seed the database with initial data
        }

        // Dispose method to cleanup after tests
        

        // Seeding database with initial data for testing
        private void SeedDatabase()
        {
            // Add sample rooms and reservations
            _dbContext.Quartos.Add(new Quarto { QuartoId = "Q1", Bloco = "A", Piso = 1, Porta = 101, QuartoEstado = QuartoEstados.Disponivel, TipologiaTipologiaId = 1 });
            _dbContext.Reservas.Add(new Reserva { ReservaId = 1, ClienteClienteId = 1, DataInicio = new DateTime(2024, 04, 01), DataFim = new DateTime(2024, 04, 10), QuartoQuartoId = "Q1", EstadoReserva = ReservaEstado.Iniciada });
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData("Q1", "2024-03-20", "2024-03-30", true)] // No overlap
        [InlineData("Q1", "2024-04-01", "2024-04-05", false)] // Complete overlap
        [InlineData("Q1", "2024-03-31", "2024-04-02", false)] // Partial overlap at start
        public void TestDisponibilidadeQuarto(string quartoId, string start, string end, bool expected)
        {
            // Convert string dates to DateTime
            DateTime inicio = DateTime.Parse(start);
            DateTime fim = DateTime.Parse(end);

            // Act - check room availability using the repository method
            var isAvailable = _repository.DisponibilidadeQuarto(quartoId, inicio, fim);

            // Assert - verify the output matches expected result
            Assert.Equal(expected, isAvailable);
        }
        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted(); // Delete the in-memory database after test
            _dbContext.Dispose();
        }
    }
}
