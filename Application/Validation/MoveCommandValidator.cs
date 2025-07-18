using Application.Models;
using Application.Services.Implementations;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validation
{
    public class MoveCommandValidator : AbstractValidator<MoveCommand>
    {
        public MoveCommandValidator(IGameRepository gameRepository)
        {
            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("IdempotencyKey обязателен");

            RuleFor(x => x)
                .CustomAsync(async (cmd, context, ct) =>
                {
                    var game = await gameRepository.GetByIdAsync(cmd.GameId);
                    if (game == null)
                    {
                        context.AddFailure("GameId", $"Игра с ID {cmd.GameId} не найдена.");
                        return;
                    }

                    if (game.Status is (int)GameStatus.Won1 or (int)GameStatus.Won2 or (int)GameStatus.Draw)
                    {
                        context.AddFailure("GameId", "Игра уже завершена.");
                    }

                    if (cmd.X < 0 || cmd.X >= game.BoardSize || cmd.Y < 0 || cmd.Y >= game.BoardSize)
                    {
                        context.AddFailure("X/Y", $"Координаты ({cmd.X},{cmd.Y}) выходят за пределы {game.BoardSize}x{game.BoardSize}.");
                        return;
                    }

                    var board = MoveService.BuildBoardMatrix(game.Moves.ToList(), game.BoardSize);
                    if (board[cmd.X, cmd.Y] != '-')
                    {
                        context.AddFailure("X/Y", $"Ячейка ({cmd.X},{cmd.Y}) уже занята.");
                    }
                });
        }
    }
}
