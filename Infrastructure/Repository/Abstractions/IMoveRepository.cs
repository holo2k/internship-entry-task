using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Abstractions
{
    public interface IMoveRepository
    {
        public Task<GameMoveEntity> GetMoveByIdAsync(int id);
        public Task<GameMoveEntity> GetMoveByIdempotencyKeyAsync(string idempotencyKey);
        public Task<IEnumerable<GameMoveEntity>> GetMovesByGameIdAsync(int gameId);
        public Task<int> CreateAsync(GameMoveEntity moveEntity);
        public Task<int> UpdateAsync(GameMoveEntity moveEntity);
        public Task DeleteAsync(int id);
    }
}
