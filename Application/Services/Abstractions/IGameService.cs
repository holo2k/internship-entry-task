using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Abstractions
{
    public interface IGameService
    {
        Task<GameDto> CreateGameAsync();
        Task<GameDto> GetGameByIdAsync(int gameId);
        Task<bool> IsGameOverAsync(int gameId);
    }
}
