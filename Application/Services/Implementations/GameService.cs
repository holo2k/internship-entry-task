using Application.DTO;
using Application.Models;
using Application.Services.Abstractions;
using Domain.Entities;
using Infrastructure.Repository.Abstractions;

namespace Application.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IGameRepository repository;

        public GameService(IGameRepository repository)
        {
            this.repository = repository;
        }

        public async Task<GameDto> CreateGameAsync()
        {
            var game = await repository.CreateAsync();

            return MapToDto(game);
        }

        public async Task<GameDto> GetGameByIdAsync(int gameId)
        {
            var game = await repository.GetByIdAsync(gameId);
            if (game == null) return null!;

            return MapToDto(game);
        }

        public async Task<bool> IsGameOverAsync(int gameId)
        {
            var game = await repository.GetByIdAsync(gameId);
            if (game == null) return true;

            return game.Status == (int)GameStatus.Draw || game.Status == (int)GameStatus.Won1 || game.Status == (int)GameStatus.Won2;
        }

        private GameDto MapToDto(GameEntity game)
        {
            return new GameDto
            {
                Id = game.Id,
                CurrentTurnPlayerNumber = game.CurrentTurnPlayerNumber,
                BoardSize = game.BoardSize,
                WinLength = game.WinLength,
                InternalStatus = (GameStatus)game.Status,
                WinnerPlayerNumber = game.WinnerPlayerNumber,
                CreatedAt = game.CreatedAt
            };
        }
    }
}
