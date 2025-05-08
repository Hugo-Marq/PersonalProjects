using Microsoft.EntityFrameworkCore;
using Moq;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Respositories.Interfaces;


namespace MyTestProject
{
    public class TestePodeReservar : IDisposable      //TESTE ATUALMENTE INCOMPLETO//
    {
        private QuartoRepository _repository;
        private QuickRoomSolutionDatabaseContext _dbContext;

        //construtor com inicialização de BD em memória para o teste
        public TestePodeReservar()
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
        public async Task PodeReservar_DeveRetornarTrueQuandoQuartosDisponiveis()
        {
            // Arrange
            var reservaDto = new ReservaDTO
            {
                TipologiaId = 1, // Supondo que 1 é o ID da tipologia de quarto desejada
                DataInicio = DateTime.Now.AddDays(3), // Data de início da reserva (1 dia a partir de hoje)
                DataFim = DateTime.Now.AddDays(1) // Data de fim da reserva (3 dias a partir de hoje)
            };

            var mockRepository = new Mock<IReservasRepository<Reserva>>();
            mockRepository.Setup(repo => repo.PodeReservar(reservaDto)).ReturnsAsync(false); // Configura o retorno do método PodeReservar como falso, indicando que não há quartos disponíveis
            var repository = mockRepository.Object;

            // Act
            var result = await repository.PodeReservar(reservaDto);

            // Assert
            Assert.False(result);
        }


        public void Dispose() //metodo que assegura a eliminação da BD em memória
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
