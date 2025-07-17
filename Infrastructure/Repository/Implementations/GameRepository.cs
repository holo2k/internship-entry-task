using Domain;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Implementations
{
    public class GameRepository(AppDbContext context) : IGameRepository
    {
        public async Task<GameEntity> CreateAsync()
        {
            var game = new GameEntity
            {
                BoardSize = Constants.BOARD_SIZE,
                CurrentTurnPlayerNumber = Constants.FIRST_PLAYER,
                WinLength = Constants.WIN_LENGTH,
                CreatedAt = DateTime.UtcNow,
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            return game;
        }

        public async Task DeleteAsync(int id)
        {
            var game = await context.Games.FindAsync(id);
            
            if(game is not null)
            {
                context.Games.Remove(game);
                await context.SaveChangesAsync();
            }
        }

        public async Task<GameEntity> GetByIdAsync(int id)
        {
            var game = await context.Games.Include(g => g.Moves).FirstOrDefaultAsync(g => g.Id == id);

            return game;
        }

        public async Task<IEnumerable<GameEntity>> GetGamesAsync()
        {
            var games = await context.Games.ToListAsync();

            return games;
        }

        public async Task<int> UpdateAsync(GameEntity gameEntity)
        {
            context.Games.Update(gameEntity);
            await context.SaveChangesAsync();

            return gameEntity.Id;
        }
    }
}
