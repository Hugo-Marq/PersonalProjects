using Microsoft.EntityFrameworkCore;
using Moq;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using System.Threading.Tasks;
using Xunit;

namespace MyTestProject
{
    public class AtualizarEstadoQuartoAposCheckout : IDisposable
    {
        private QuartoRepository _repository;
        private QuickRoomSolutionDatabaseContext _dbContext;

        public AtualizarEstadoQuartoAposCheckout()
        {
            var options = new DbContextOptionsBuilder<QuickRoomSolutionDatabaseContext>()
                .UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid()}").Options;
            //$"TestDatabase-{Guid.NewGuid()}" serve para garantir BD única em memória para
            //evitar conflitos quando existem diversos testes a correr em simultâneo.

            _dbContext = new QuickRoomSolutionDatabaseContext(options);
            _dbContext.Database.EnsureCreated();// assegura a criação da BD em memória 
            _repository = new QuartoRepository(_dbContext);
        }

        [Fact]
        public async Task AtualizarEstadoQuarto_ShouldUpdateQuartoState_WhenCalled()
        {
            // Arrange

            var quarto = new Quarto
            {
                QuartoId = "sampleId",
                Bloco = "sampleBloco",
                Piso = 1,
                Porta = 1,
                QuartoEstado = QuartoEstados.Disponivel,
                TipologiaTipologiaId = 1
            };

            _dbContext.Quartos.Add(quarto);
            _dbContext.SaveChanges();

            var quartoRepository = new QuartoRepository(_dbContext);

            // Act
            var updatedQuarto = await quartoRepository.AtualizarEstadoQuarto("sampleId", 5); // Assuming 2 corresponds to "Manutencao"

            // Assert
            var retrievedQuarto = await _dbContext.Quartos.FindAsync("sampleId");
            Assert.NotNull(retrievedQuarto);
            Assert.Equal(5, (int)retrievedQuarto.QuartoEstado); // Assuming QuartoEstado is an enum
        }
        public void Dispose() //metodo que assegura a eliminação da BD em memória
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
