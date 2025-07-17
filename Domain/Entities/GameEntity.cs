using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GameEntity
    {
        public int Id { get; set; }
        public int CurrentTurnPlayerNumber { get; set; }
        public int BoardSize { get; set; }
        public int WinLength { get; set; }
        public int Status { get; set; }
        public int? WinnerPlayerNumber { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<GameMoveEntity> Moves { get; set; } = new List<GameMoveEntity>();
    }
}
