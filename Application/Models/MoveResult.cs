using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class MoveResult
    {
        public bool Success { get; set; }
        public bool IsGameOver { get; set; }
        public string? ErrorMessage { get; set; }
        public GameDto? UpdatedGame { get; set; }
        public GameMoveDto? Move { get; set; }
    }
}
