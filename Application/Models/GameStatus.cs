using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public enum GameStatus
    {
        [Display(Name = "Ожидание")]
        Pending,

        [Display(Name = "Игра")]
        Playing,

        [Display(Name = "Ничья")]
        Draw,

        [Display(Name = "Победа игрока 1")]
        Won1,

        [Display(Name = "Победа игрока 2")]
        Won2
    }

}
