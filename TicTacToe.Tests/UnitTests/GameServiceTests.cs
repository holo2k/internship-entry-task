using Application.Models;
using Application.Services.Implementations;
using Domain.Entities;
using Infrastructure.Repository.Abstractions;
using Moq;

namespace TicTacToe.Tests.UnitTests
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> gameRepoMock = new();

        private GameService CreateService() => new(gameRepoMock.Object);

        [Fact]
        public async Task CreateGameAsync_ReturnsGameDto()
        {
            // Arrange
            var gameEntity = new GameEntity
            {
                Id = 1,
                CurrentTurnPlayerNumber = 1,
                BoardSize = 3,
                WinLength = 3,
                Status = (int)GameStatus.Playing,
                WinnerPlayerNumber = null,
                CreatedAt = DateTime.UtcNow
            };

            gameRepoMock.Setup(r => r.CreateAsync()).ReturnsAsync(gameEntity);

            var service = CreateService();

            // Act
            var result = await service.CreateGameAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(gameEntity.Id, result.Id);
            Assert.Equal(gameEntity.CurrentTurnPlayerNumber, result.CurrentTurnPlayerNumber);
            Assert.Equal(gameEntity.BoardSize, result.BoardSize);
            Assert.Equal(gameEntity.WinLength, result.WinLength);
            Assert.Equal(GameStatus.Playing, result.InternalStatus);
            Assert.Null(result.WinnerPlayerNumber);
        }

        [Fact]
        public async Task GetGameByIdAsync_ReturnsGameDto_IfFound()
        {
            // Arrange
            var gameEntity = new GameEntity
            {
                Id = 2,
                CurrentTurnPlayerNumber = 2,
                BoardSize = 4,
                WinLength = 4,
                Status = (int)GameStatus.Won1,
                WinnerPlayerNumber = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            gameRepoMock.Setup(r => r.GetByIdAsync(gameEntity.Id)).ReturnsAsync(gameEntity);

            var service = CreateService();

            // Act
            var result = await service.GetGameByIdAsync(gameEntity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(gameEntity.Id, result.Id);
            Assert.Equal(gameEntity.CurrentTurnPlayerNumber, result.CurrentTurnPlayerNumber);
            Assert.Equal(gameEntity.BoardSize, result.BoardSize);
            Assert.Equal(gameEntity.WinLength, result.WinLength);
            Assert.Equal(GameStatus.Won1, result.InternalStatus);
            Assert.Equal(1, result.WinnerPlayerNumber);
        }

        [Fact]
        public async Task GetGameByIdAsync_ReturnsNull_IfNotFound()
        {
            // Arrange
            int gameId = 999;
            gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((GameEntity)null);

            var service = CreateService();

            // Act
            var result = await service.GetGameByIdAsync(gameId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(GameStatus.Draw, true)]
        [InlineData(GameStatus.Won1, true)]
        [InlineData(GameStatus.Won2, true)]
        [InlineData(GameStatus.Playing, false)]
        public async Task IsGameOverAsync_ReturnsCorrectValue(GameStatus status, bool expectedIsOver)
        {
            // Arrange
            var gameEntity = new GameEntity
            {
                Id = 1,
                Status = (int)status
            };
            gameRepoMock.Setup(r => r.GetByIdAsync(gameEntity.Id)).ReturnsAsync(gameEntity);

            var service = CreateService();

            // Act
            var result = await service.IsGameOverAsync(gameEntity.Id);

            // Assert
            Assert.Equal(expectedIsOver, result);
        }

        [Fact]
        public async Task IsGameOverAsync_ReturnsTrue_IfGameNotFound()
        {
            // Arrange
            int gameId = 1000;
            gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((GameEntity)null);

            var service = CreateService();

            // Act
            var result = await service.IsGameOverAsync(gameId);

            // Assert
            Assert.True(result);
        }
    }

}
