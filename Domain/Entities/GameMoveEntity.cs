using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GameMoveEntity
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PointX { get; set; }
        public int PointY { get; set; }
        public char PlacedSymbol { get; set; }
        public bool IsSwapped { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IdempotencyKey { get; set; }

        public virtual GameEntity Game { get;set; }
    }
}
