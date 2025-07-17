using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Abstractions
{
    public interface IGameRepository
    {
        public Task<GameEntity> GetByIdAsync(int id);
        public Task<IEnumerable<GameEntity>> GetGamesAsync();
        public Task<GameEntity> CreateAsync();
        public Task<int> UpdateAsync(GameEntity gameEntity);
        public Task DeleteAsync(int id);
    }
}
