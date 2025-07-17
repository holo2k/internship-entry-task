using Application.DTO;
using Application.Models;
using Application.Services.Abstractions;
using Application.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace TicTacToeApi.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService gameService;

        public GameController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        /// <summary>
        /// Получить игру по ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGameById(int id)
        {
            var game = await gameService.GetGameByIdAsync(id);
            if (game == null)
                return NotFound();

            return Ok(game);
        }

        /// <summary>
        /// Создать новую игру.
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<GameDto>> CreateGame()
        {
            var createdGame = await gameService.CreateGameAsync();

            return CreatedAtAction(nameof(GetGameById), new { id = createdGame.Id }, createdGame);
        }
    }
}

