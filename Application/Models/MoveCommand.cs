using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class MoveCommand
    {
        public int GameId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string IdempotencyKey { get; set; } = null!;
    }
}
