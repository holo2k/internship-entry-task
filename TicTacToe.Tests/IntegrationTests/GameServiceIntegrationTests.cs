using Application.Models;
using Application.Services.Implementations;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repository.Implementations;
using Microsoft.EntityFrameworkCore;

namespace TicTacToe.Tests.IntegrationTests
{
    public class GameServiceIntegrationTests
    {
        private readonly DbContextOptions<AppDbContext> options;

        public GameServiceIntegrationTests()
        {
            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "GameServiceTestDb")
                .Options;
        }

        [Fact]
        public async Task CreateGameAsync_SavesGameAndReturnsDto()
        {
            // Arrange
            await using var context = new AppDbContext(options);
            var repository = new GameRepository(context);
            var service = new GameService(repository);

            // Act
            var result = await service.CreateGameAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var gameInDb = await context.Games.FindAsync(result.Id);
            Assert.NotNull(gameInDb);
        }

        [Fact]
        public async Task GetGameByIdAsync_ReturnsGame_WhenExists()
        {
            // Arrange
            await using var context = new AppDbContext(options);
            var repository = new GameRepository(context);
            var service = new GameService(repository);

            var game = new GameEntity
            {
                BoardSize = 3,
                WinLength = 3,
                CurrentTurnPlayerNumber = 1,
                Status = (int)GameStatus.Playing
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetGameByIdAsync(game.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(game.Id, result.Id);
        }
    }
}
