using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implementations
{
    public class MoveRepository(AppDbContext context) : IMoveRepository
    {
        public async Task<int> CreateAsync(GameMoveEntity move)
        {
            context.Moves.Add(move);
            await context.SaveChangesAsync();

            return move.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var move = await context.Moves.FindAsync(id);

            if (move is not null)
            {
                context.Moves.Remove(move);
                await context.SaveChangesAsync();
            }
        }

        public async Task<GameMoveEntity> GetMoveByIdAsync(int id)
        {
            var move = await context.Moves.FindAsync(id);

            return move;
        }

        public async Task<GameMoveEntity> GetMoveByIdempotencyKeyAsync(string idempotencyKey)
        {
            var move = await context.Moves.FirstOrDefaultAsync(m => m.IdempotencyKey == idempotencyKey);

            return move;
        }

        public async Task<IEnumerable<GameMoveEntity>> GetMovesAsync()
        {
            var moves = await context.Moves.ToListAsync();

            return moves;
        }

        public async Task<IEnumerable<GameMoveEntity>> GetMovesByGameIdAsync(int gameId)
        {
            var moves = await context.Moves.Where(m => m.GameId == gameId).ToListAsync();

            return moves;
        }

        public async Task<int> UpdateAsync(GameMoveEntity moveEntity)
        {
            context.Moves.Update(moveEntity);
            await context.SaveChangesAsync();

            return moveEntity.Id;
        }
    }
}
