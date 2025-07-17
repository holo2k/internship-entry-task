using Application.DTO;
using Application.Models;
using Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace TicTacToeApi.Controllers
{
    [Route("api/moves")]
    [ApiController]
    public class MoveController : ControllerBase
    {

        private readonly IMoveService moveService;

        public MoveController(IMoveService moveService)
        {
            this.moveService = moveService;
        }

        /// <summary>
        /// Получить список всех ходов по игре.
        /// </summary>
        [HttpGet("{gameId}")]
        public async Task<ActionResult<IReadOnlyList<GameMoveDto>>> GetMoves(int gameId)
        {
            var moves = await moveService.GetMovesAsync(gameId);
            return Ok(moves);
        }

        /// <summary>
        /// Совершить ход в игре.
        /// </summary>
        [HttpPost("make-move/{gameId}")]
        public async Task<ActionResult<MoveResult>> MakeMove(
            int gameId,
            [FromQuery] int x,
            [FromQuery] int y,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest("Idempotency-Key is required in the header.");
            }

            try
            {
                var result = await moveService.MakeMoveAsync(gameId, x, y, idempotencyKey);
                var etag = GenerateETag(result);
                var ifNoneMatch = Request.Headers["If-None-Match"].ToString();
                if (ifNoneMatch == $"\"{etag}\"")
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
                Response.Headers["ETag"] = $"\"{etag}\"";
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GenerateETag(MoveResult result)
        {
            var raw = $"{result.Move.Id}-{result.Move.GameId}-{result.Move.CreatedAt.ToBinary()}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(hash);
        }
    }
}
