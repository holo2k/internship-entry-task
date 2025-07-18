using Application.Models;
using Application.Services.Abstractions;
using Application.Services.Implementations;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repository.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared;

namespace TicTacToe.Tests.IntegrationTests
{
    public class MoveServiceIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MoveService _moveService;
        private readonly GameRepository _gameRepository;
        private readonly MoveRepository _moveRepository;

        public MoveServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _gameRepository = new GameRepository(_context);
            _moveRepository = new MoveRepository(_context);

            var randomMock = new Mock<IRandomProvider>();
            randomMock.Setup(r => r.NextDouble()).Returns(1.0);

            _moveService = new MoveService(_moveRepository, _gameRepository, randomMock.Object);
        }

        private async Task SeedGameAsync(GameEntity game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task MakeMoveAsync_ReturnsExistingMove_IfIdempotencyKeyExists()
        {
            var game = new GameEntity
            {
                BoardSize = 3,
                WinLength = 3,
                CurrentTurnPlayerNumber = Constants.FIRST_PLAYER,
                Status = (int)GameStatus.Playing,
                Moves = new List<GameMoveEntity>()
            };
            await SeedGameAsync(game);

            var existingMove = new GameMoveEntity
            {
                GameId = game.Id,
                PointX = 0,
                PointY = 0,
                PlacedSymbol = 'X',
                IdempotencyKey = "idem-123",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Moves.AddAsync(existingMove);
            await _context.SaveChangesAsync();

            var cmd = new MoveCommand
            {
                GameId = game.Id,
                IdempotencyKey = existingMove.IdempotencyKey
            };

            var result = await _moveService.MakeMoveAsync(cmd);

            Assert.True(result.Success);
            Assert.Equal(existingMove.Id, result.Move.Id);
        }

        [Fact]
        public async Task MakeMoveAsync_ThrowsValidationException_IfGameNotFound()
        {
            var cmd = new MoveCommand
            {
                GameId = 999,
                X = 0,
                Y = 0,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            await Assert.ThrowsAsync<ValidationException>(() => _moveService.MakeMoveAsync(cmd));
        }

        [Fact]
        public async Task MakeMoveAsync_CreatesMoveAndUpdatesGame_WhenValid()
        {
            Environment.SetEnvironmentVariable("BOARD_SIZE", "3");
            Environment.SetEnvironmentVariable("WIN_LENGTH", "3");
            Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");

            var game = new GameEntity
            {
                BoardSize = 3,
                WinLength = 3,
                CurrentTurnPlayerNumber = Constants.FIRST_PLAYER,
                Status = (int)GameStatus.Playing,
                Moves = new List<GameMoveEntity>()
            };
            await SeedGameAsync(game);

            var cmd = new MoveCommand
            {
                GameId = game.Id,
                X = 0,
                Y = 0,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var result = await _moveService.MakeMoveAsync(cmd);

            Assert.True(result.Success);
            Assert.Equal(game.Id, result.Move.GameId);
            Assert.NotNull(result.UpdatedGame);
            Assert.Contains(result.Move.PointX, new[] { 0, 1, 2 });
            Assert.Contains(result.Move.PointY, new[] { 0, 1, 2 });
        }

        [Fact]
        public async Task MakeMoveAsync_WinsForFirstPlayer_On3x3Board()
        {
            Environment.SetEnvironmentVariable("BOARD_SIZE", "3");
            Environment.SetEnvironmentVariable("WIN_LENGTH", "3");
            Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");

            var moves = new List<GameMoveEntity>
            {
                new() { PointX = 0, PointY = 0, PlacedSymbol = 'X', IdempotencyKey = Guid.NewGuid().ToString() },
                new() { PointX = 1, PointY = 0, PlacedSymbol = 'X', IdempotencyKey = Guid.NewGuid().ToString() }
            };

            var game = new GameEntity
            {
                BoardSize = 3,
                WinLength = 3,
                CurrentTurnPlayerNumber = Constants.FIRST_PLAYER,
                Status = (int)GameStatus.Playing,
                Moves = moves
            };
            await SeedGameAsync(game);

            var cmd = new MoveCommand
            {
                GameId = game.Id,
                X = 2,
                Y = 0,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var result = await _moveService.MakeMoveAsync(cmd);

            Assert.True(result.Success);
            Assert.Equal(GameStatus.Won1, result.UpdatedGame.InternalStatus);
            Assert.Equal(Constants.FIRST_PLAYER, result.UpdatedGame.WinnerPlayerNumber);
            Assert.True(result.IsGameOver);
        }

        [Fact]
        public async Task MakeMoveAsync_WinsForSecondPlayer_On4x4Board()
        {
            Environment.SetEnvironmentVariable("BOARD_SIZE", "4");
            Environment.SetEnvironmentVariable("WIN_LENGTH", "4");
            Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");

            var moves = new List<GameMoveEntity>
            {
                new() { PointX = 0, PointY = 0, PlacedSymbol = '0', IdempotencyKey = Guid.NewGuid().ToString() },
                new() { PointX = 1, PointY = 1, PlacedSymbol = '0', IdempotencyKey = Guid.NewGuid().ToString() },
                new() { PointX = 2, PointY = 2, PlacedSymbol = '0', IdempotencyKey = Guid.NewGuid().ToString() }
            };

            var game = new GameEntity
            {
                BoardSize = 4,
                WinLength = 4,
                CurrentTurnPlayerNumber = Constants.SECOND_PLAYER,
                Status = (int)GameStatus.Playing,
                Moves = moves
            };
            await SeedGameAsync(game);

            var cmd = new MoveCommand
            {
                GameId = game.Id,
                X = 3,
                Y = 3,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var result = await _moveService.MakeMoveAsync(cmd);

            Assert.True(result.Success);
            Assert.Equal(GameStatus.Won2, result.UpdatedGame.InternalStatus);
            Assert.Equal(Constants.SECOND_PLAYER, result.UpdatedGame.WinnerPlayerNumber);
            Assert.True(result.IsGameOver);
        }

        [Fact]
        public async Task MakeMoveAsync_DrawOn5x5Board()
        {
            Environment.SetEnvironmentVariable("BOARD_SIZE", "5");
            Environment.SetEnvironmentVariable("WIN_LENGTH", "5");
            Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");

            int size = 5;

            var moves = new List<GameMoveEntity>();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    char symbol;

                    if (i == j)
                        symbol = i % 2 == 0 ? 'X' : '0';
                    else if (i + j == size - 1)
                        symbol = i % 2 == 0 ? '0' : 'X';
                    else
                        symbol = (i + j) % 2 == 0 ? 'X' : '0';

                    moves.Add(new GameMoveEntity
                    {
                        PointX = i,
                        PointY = j,
                        PlacedSymbol = symbol,
                        IdempotencyKey = Guid.NewGuid().ToString()
                    });
                }
            }

            var lastMoveX = 4;
            var lastMoveY = 4;
            moves.RemoveAll(m => m.PointX == lastMoveX && m.PointY == lastMoveY);

            var game = new GameEntity
            {
                BoardSize = size,
                WinLength = 5,
                CurrentTurnPlayerNumber = Constants.SECOND_PLAYER,
                Status = (int)GameStatus.Playing,
                Moves = moves
            };
            await SeedGameAsync(game);

            var cmd = new MoveCommand
            {
                GameId = game.Id,
                X = lastMoveX,
                Y = lastMoveY,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var result = await _moveService.MakeMoveAsync(cmd);

            Assert.True(result.Success);
            Assert.Equal(GameStatus.Draw, result.UpdatedGame.InternalStatus);
            Assert.Null(result.UpdatedGame.WinnerPlayerNumber);
            Assert.True(result.IsGameOver);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
