using Application.DTO;
using Application.Models;
using Application.Services.Abstractions;
using Application.Validation;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Repository.Abstractions;
using Shared;

namespace Application.Services.Implementations
{
    public class MoveService : IMoveService
    {
        private readonly IMoveRepository moveRepository;
        private readonly IGameRepository gameRepository;
        private readonly IRandomProvider randomProvider;

        public MoveService(IMoveRepository moveRepository, IGameRepository gameRepository, IRandomProvider randomProvider)
        {
            this.moveRepository = moveRepository;
            this.gameRepository = gameRepository;
            this.randomProvider = randomProvider;
        }

        public async Task<IReadOnlyList<GameMoveDto>> GetMovesAsync(int gameId)
        {
            var moves = await moveRepository.GetMovesByGameIdAsync(gameId);
            return moves.Select(MapToDto).ToList();
        }

        public async Task<MoveResult> MakeMoveAsync(MoveCommand cmd)
        {
            var existingMove = await moveRepository.GetMoveByIdempotencyKeyAsync(cmd.IdempotencyKey);
            if (existingMove != null)
            {
                return new MoveResult
                {
                    Success = true,
                    Move = MapToDto(existingMove),
                    UpdatedGame = null
                };
            }

            var validator = new MoveCommandValidator(gameRepository);
            var validationResult = await validator.ValidateAsync(cmd);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var game = await gameRepository.GetByIdAsync(cmd.GameId);

            var board = BuildBoardMatrix(game.Moves.ToList(), game.BoardSize);
            bool isSwapped = randomProvider.NextDouble() < Constants.ENEMY_MOVE_CHANCE;

            var symbol = game.CurrentTurnPlayerNumber == Constants.FIRST_PLAYER ? 'X' : '0';
            var placedSymbol = isSwapped ? (symbol == 'X' ? '0' : 'X') : symbol;
            board[cmd.X, cmd.Y] = placedSymbol;

            var move = new GameMoveEntity
            {
                GameId = game.Id,
                PointX = cmd.X,
                PointY = cmd.Y,
                PlacedSymbol = placedSymbol,
                IsSwapped = isSwapped,
                IdempotencyKey = cmd.IdempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            await moveRepository.CreateAsync(move);

            var status = CheckGameStatus(board, game.WinLength);
            game.Status = (int)status;

            if (status == GameStatus.Won1)
                game.WinnerPlayerNumber = Constants.FIRST_PLAYER;
            else if (status == GameStatus.Won2)
                game.WinnerPlayerNumber = Constants.SECOND_PLAYER;

            if (status == GameStatus.Playing)
                game.CurrentTurnPlayerNumber = game.CurrentTurnPlayerNumber == Constants.FIRST_PLAYER ? Constants.SECOND_PLAYER : Constants.FIRST_PLAYER;

            await gameRepository.UpdateAsync(game);

            return new MoveResult
            {
                Success = true,
                Move = MapToDto(move),
                UpdatedGame = new GameDto
                {
                    Id = game.Id,
                    CurrentTurnPlayerNumber = game.CurrentTurnPlayerNumber,
                    BoardSize = game.BoardSize,
                    WinLength = game.WinLength,
                    InternalStatus = (GameStatus)game.Status,
                    WinnerPlayerNumber = game.WinnerPlayerNumber,
                    CreatedAt = game.CreatedAt
                },
                IsGameOver = status is GameStatus.Draw or GameStatus.Won1 or GameStatus.Won2
            };
        }


        public static char[,] BuildBoardMatrix(List<GameMoveEntity> moves, int size)
        {
            var board = new char[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    board[i, j] = '-';

            foreach (var move in moves)
                board[move.PointX, move.PointY] = move.PlacedSymbol;

            return board;
        }

        private GameMoveDto MapToDto(GameMoveEntity move) => new GameMoveDto
        {
            Id = move.Id,
            GameId = move.GameId,
            IdempotencyKey = move.IdempotencyKey,
            PointX = move.PointX,
            PointY = move.PointY,
            PlacedSymbol = move.PlacedSymbol,
            IsSwapped = move.IsSwapped,
            CreatedAt = move.CreatedAt
        };

        private GameStatus CheckGameStatus(char[,] board, int winLength)
        {
            int size = board.GetLength(0);

            bool IsWin(char symbol)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        if (board[x, y] != symbol) continue;

                        int[][] directions =
                        {
                            [1, 0], [0, 1],
                            [1, 1], [1, -1]
                        };

                        foreach (var dir in directions)
                        {
                            int dx = dir[0], dy = dir[1];
                            int count = 0;

                            for (int i = 0; i < winLength; i++)
                            {
                                int nx = x + dx * i, ny = y + dy * i;
                                if (nx >= size || ny >= size || ny < 0 || board[nx, ny] != symbol) break;
                                count++;
                            }

                            if (count == winLength) return true;
                        }
                    }
                }
                return false;
            }

            if (IsWin('X')) return GameStatus.Won1;
            if (IsWin('0')) return GameStatus.Won2;

            foreach (var cell in board)
                if (cell == '-') return GameStatus.Playing;

            return GameStatus.Draw;
        }
    }
}
