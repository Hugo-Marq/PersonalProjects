using Xunit;
using Moq;
using QuickRoomSolutions.Controllers;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MyTestProject
{
    public class QuartosControllerTests
    {
        /***************** 
         Este teste serve apenas como exemplo.
         Atualmente não é executável por opção
         Motivo: mal configurado, enche memória
                          *********************/

        //[Fact] serve para testar uma única vez e sem parâmetros
        //       testa se a lógica está bem implementada
        public async Task PostQuarto_ReturnsCreatedResult()
        {
            // Arrange
            var mockRepository = new Mock<IBaseQuartoRepository<Quarto>>();
            var mockTipologiaRepository = new Mock<ITipologiasRepository<Tipologia>>();
            var controller = new QuartosController(mockRepository.Object, mockTipologiaRepository.Object);
            var newQuarto = new Quarto
            {
                Bloco = "A",
                Piso = 1,
                Porta = 3,
                TipologiaTipologiaId = 2,
                QuartoEstado = QuartoEstados.Disponivel
            };

        var expectedQuartoId = $"{newQuarto.Bloco}-{newQuarto.Piso:00}-{newQuarto.Porta:00}";

        // Setup Insert directly with expectedQuartoId
        mockRepository.Setup(repo => repo.Insert(It.IsAny<Quarto>()))
                .ReturnsAsync((Quarto q) =>
                {
                    q.QuartoId = expectedQuartoId;
                    return q;
                });

        // Act
        var result = await controller.PostQuarto(newQuarto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var model = Assert.IsType<Quarto>(createdResult.Value);
            Assert.Equal(newQuarto.QuartoId, model.QuartoId);
            Assert.Equal(newQuarto.Bloco, model.Bloco);
            Assert.Equal(newQuarto.Piso, model.Piso);
            Assert.Equal(newQuarto.Porta, model.Porta);
            Assert.Equal(newQuarto.TipologiaTipologiaId, model.TipologiaTipologiaId);
            Assert.Equal(newQuarto.QuartoEstado, model.QuartoEstado);
        }
    }

}
