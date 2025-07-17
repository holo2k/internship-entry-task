using Application.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class GameDto
    {
        public int Id { get; set; }
        public int CurrentTurnPlayerNumber { get; set; }
        public int BoardSize { get; set; }
        public int WinLength { get; set; }
        public string Status => GetStatusDisplayName();
        [JsonIgnore]
        public GameStatus InternalStatus { get; set; }
        public int? WinnerPlayerNumber { get; set; }
        public DateTime CreatedAt { get; set; }

        private string GetStatusDisplayName()
        {
            var field = typeof(GameStatus).GetField(InternalStatus.ToString());
            var attr = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                            .Cast<DisplayAttribute>()
                            .FirstOrDefault();
            return attr?.Name ?? InternalStatus.ToString();
        }
    }

}
