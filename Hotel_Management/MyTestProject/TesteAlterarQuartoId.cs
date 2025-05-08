using Xunit;
using QuickRoomSolutions.Respositories;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;

namespace MyTestProject
{
    public class QuartoRepositoryTests : IDisposable  //IMPORTANTE para limpeza da memória
    {

        private QuartoRepository _repository;
        private QuickRoomSolutionDatabaseContext _dbContext;

        //construtor com inicialização de BD em memória para o teste
        public QuartoRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<QuickRoomSolutionDatabaseContext>()
                .UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid()}").Options;
            //$"TestDatabase-{Guid.NewGuid()}" serve para garantir BD única em memória para
            //evitar conflitos quando existem diversos testes a correr em simultâneo.

            _dbContext = new QuickRoomSolutionDatabaseContext(options);
            _dbContext.Database.EnsureCreated();// assegura a criação da BD em memória 
            _repository = new QuartoRepository(_dbContext);
        }

        //[Theory] serve para testar metodos parametrizados com vários conjuntos de valores.
        //         testa multiplas vezes, util para testar diferentes comportamentos consoante o input
        [Theory]
        [InlineData("A", 1, 101, "A-01-101")]
        [InlineData("B", 2, 120, "B-02-120")]
        [InlineData("C", 10, 105, "C-10-105")]
        public async Task GerarQuartoId_ShouldGenerateCorrectId(string bloco, int piso, int porta, string expectedId)
        {
            // Act
            var result = await _repository.GerarQuartoId(bloco, piso, porta);

            // Assert
            Assert.Equal(expectedId, result);
        }
        public void Dispose() //metodo que assegura a eliminação da BD em memória
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
