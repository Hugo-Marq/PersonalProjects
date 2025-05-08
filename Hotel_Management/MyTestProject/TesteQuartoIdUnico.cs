using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.DataBase;
using QuickRoomSolutions.Respositories;
using QuickRoomSolutions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTestProject
{
    public class TesteQuartoIdUnico : IDisposable
    {
        private QuartoRepository _repository;
        private QuickRoomSolutionDatabaseContext _dbContext;

        public TesteQuartoIdUnico()
        {
            var options = new DbContextOptionsBuilder<QuickRoomSolutionDatabaseContext>()
                .UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid()}").Options;

            _dbContext = new QuickRoomSolutionDatabaseContext(options);
            _dbContext.Database.EnsureCreated(); 
            _repository = new QuartoRepository(_dbContext);
        }
        [Fact]
        public async Task TesteQuartoIdUnicos()
        {
            // Arrange
            Quarto quarto1 = new Quarto { Bloco = "A", Piso = 1, Porta = 101 };
            Quarto quarto2 = new Quarto { Bloco = "A", Piso = 1, Porta = 101 };
            // Act
            await _repository.Insert(quarto1);
            Func<Task> act = async () => await _repository.Insert(quarto2);
            // Assert
            await Assert.ThrowsAsync<Exception>(act);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
