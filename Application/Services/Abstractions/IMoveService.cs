﻿using Application.DTO;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Abstractions
{
    public interface IMoveService
    {
        Task<MoveResult> MakeMoveAsync(MoveCommand moveCommand);
        Task<IReadOnlyList<GameMoveDto>> GetMovesAsync(int gameId);
    }
}
