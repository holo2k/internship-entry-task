using Application.DTO;
using Application.Models;
using Application.Services.Abstractions;
using Application.Services.Implementations;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Repository.Abstractions;
using Moq;
using Shared;

namespace TicTacToe.Tests.UnitTests;

public class MoveServiceTests
{
    private readonly Mock<IMoveRepository> moveRepoMock = new();
    private readonly Mock<IGameRepository> gameRepoMock = new();

    private MoveService CreateService() 
    {
        var randomMock = new Mock<IRandomProvider>();
        randomMock.Setup(r => r.NextDouble()).Returns(1.0);
        var service = new MoveService(moveRepoMock.Object, gameRepoMock.Object, randomMock.Object);

        return service;
    }

    [Fact]
    public async Task MakeMoveAsync_ReturnsExistingMove_IfIdempotencyKeyExists()
    {
        // Arrange

        var idempotencyKey = "idem-123";
        var existingMove = new GameMoveEntity
        {
            Id = 1,
            GameId = 1,
            PointX = 0,
            PointY = 0,
            PlacedSymbol = 'X',
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
        moveRepoMock.Setup(x => x.GetMoveByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync(existingMove);

        var service = CreateService();

        // Act
        var result = await service.MakeMoveAsync(new MoveCommand { IdempotencyKey = idempotencyKey });

        // Assert
        Assert.True(result.Success);
        Assert.Equal(existingMove.Id, result.Move.Id);
    }

    [Fact]
    public async Task MakeMoveAsync_ThrowsValidationException_IfGameNotFound()
    {
        // Arrange
        var cmd = new MoveCommand { GameId = 999, X = 0, Y = 0, IdempotencyKey = "key" };
        gameRepoMock.Setup(x => x.GetByIdAsync(cmd.GameId)).ReturnsAsync((GameEntity)null);
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.MakeMoveAsync(cmd));
    }

    [Fact]
    public async Task MakeMoveAsync_CreatesMoveAndUpdatesGame_WhenValid()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BOARD_SIZE", "3");
        Environment.SetEnvironmentVariable("WIN_LENGTH", "3");
        Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");
        var cmd = new MoveCommand
        {
            GameId = 1,
            X = 0,
            Y = 0,
            IdempotencyKey = "key",
        };

        var game = new GameEntity
        {
            Id = 1,
            BoardSize = 3,
            WinLength = 3,
            CurrentTurnPlayerNumber = 1,
            Status = (int)GameStatus.Playing,
            Moves = new List<GameMoveEntity>()
        };

        moveRepoMock.Setup(x => x.GetMoveByIdempotencyKeyAsync(cmd.IdempotencyKey)).ReturnsAsync((GameMoveEntity)null);
        gameRepoMock.Setup(x => x.GetByIdAsync(cmd.GameId)).ReturnsAsync(game);
        moveRepoMock.Setup(x => x.CreateAsync(It.IsAny<GameMoveEntity>())).ReturnsAsync(1);
        gameRepoMock.Setup(x => x.UpdateAsync(It.IsAny<GameEntity>())).ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.MakeMoveAsync(cmd);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(cmd.GameId, result.Move.GameId);
        Assert.NotNull(result.UpdatedGame);
        Assert.Contains(result.Move.PointX, new[] { 0, 1, 2 });
        Assert.Contains(result.Move.PointY, new[] { 0, 1, 2 });
    }

    [Fact]
    public async Task MakeMoveAsync_WinsForFirstPlayer_On3x3Board()
    {
        // Arrange
        Environment.SetEnvironmentVariable("BOARD_SIZE", "3");
        Environment.SetEnvironmentVariable("WIN_LENGTH", "3");
        Environment.SetEnvironmentVariable("ENEMY_MOVE_CHANCE", "0.1");
        
        var cmd = new MoveCommand
        {
            GameId = 1,
            X = 2,
            Y = 0,
            IdempotencyKey = "key-win1"
        };

        var moves = new List<GameMoveEntity>
        {
            new() { PointX = 0, PointY = 0, PlacedSymbol = 'X' },
            new() { PointX = 1, PointY = 0, PlacedSymbol = 'X' },
        };

        var game = new GameEntity
        {
            Id = 1,
            BoardSize = 3,
            WinLength = 3,
            CurrentTurnPlayerNumber = Constants.FIRST_PLAYER,
            Status = (int)GameStatus.Playing,
            Moves = moves
        };

        moveRepoMock.Setup(x => x.GetMoveByIdempotencyKeyAsync(cmd.IdempotencyKey)).ReturnsAsync((GameMoveEntity)null);
        gameRepoMock.Setup(x => x.GetByIdAsync(cmd.GameId)).ReturnsAsync(game);
        moveRepoMock.Setup(x => x.CreateAsync(It.IsAny<GameMoveEntity>())).ReturnsAsync(1);
        gameRepoMock.Setup(x => x.UpdateAsync(It.IsAny<GameEntity>())).ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.MakeMoveAsync(cmd);

        // Assert
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

        var cmd = new MoveCommand
        {
            GameId = 2,
            X = 3,
            Y = 3,
            IdempotencyKey = "key-win2"
        };

        var moves = new List<GameMoveEntity>
        {
            new() { PointX = 0, PointY = 0, PlacedSymbol = '0' },
            new() { PointX = 1, PointY = 1, PlacedSymbol = '0' },
            new() { PointX = 2, PointY = 2, PlacedSymbol = '0' },
        };

        var game = new GameEntity
        {
            Id = 2,
            BoardSize = 4,
            WinLength = 4,
            CurrentTurnPlayerNumber = Constants.SECOND_PLAYER,
            Status = (int)GameStatus.Playing,
            Moves = moves
        };

        moveRepoMock.Setup(x => x.GetMoveByIdempotencyKeyAsync(cmd.IdempotencyKey)).ReturnsAsync((GameMoveEntity)null);
        gameRepoMock.Setup(x => x.GetByIdAsync(cmd.GameId)).ReturnsAsync(game);
        moveRepoMock.Setup(x => x.CreateAsync(It.IsAny<GameMoveEntity>())).ReturnsAsync(1);
        gameRepoMock.Setup(x => x.UpdateAsync(It.IsAny<GameEntity>())).ReturnsAsync(1);

        var service = CreateService();

        var result = await service.MakeMoveAsync(cmd);

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

        var cmd = new MoveCommand
        {
            GameId = 3,
            X = 4,
            Y = 4,
            IdempotencyKey = "key-draw"
        };

        var moves = new List<GameMoveEntity>();
        int size = 5;

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
                    PlacedSymbol = symbol
                });
            }
        }

        moves.RemoveAll(m => m.PointX == cmd.X && m.PointY == cmd.Y);

        var game = new GameEntity
        {
            Id = 3,
            BoardSize = size,
            WinLength = 5,
            CurrentTurnPlayerNumber = Constants.SECOND_PLAYER,
            Status = (int)GameStatus.Playing,
            Moves = moves
        };

        moveRepoMock.Setup(x => x.GetMoveByIdempotencyKeyAsync(cmd.IdempotencyKey)).ReturnsAsync((GameMoveEntity)null);
        gameRepoMock.Setup(x => x.GetByIdAsync(cmd.GameId)).ReturnsAsync(game);
        moveRepoMock.Setup(x => x.CreateAsync(It.Is<GameMoveEntity>(m =>
            m.PointX == cmd.X && m.PointY == cmd.Y && m.PlacedSymbol == '0'))).ReturnsAsync(1);
        gameRepoMock.Setup(x => x.UpdateAsync(It.IsAny<GameEntity>())).ReturnsAsync(1);

        var service = CreateService();

        var result = await service.MakeMoveAsync(cmd);

        Assert.True(result.Success);
        Assert.Equal(GameStatus.Draw, result.UpdatedGame.InternalStatus);
        Assert.Null(result.UpdatedGame.WinnerPlayerNumber);
        Assert.True(result.IsGameOver);
    }
}
