using System.Globalization;

namespace Domain
{
    public static class Constants
    {
        public static int BOARD_SIZE = int.Parse(Environment.GetEnvironmentVariable("BOARD_SIZE")!);
        public static int WIN_LENGTH = int.Parse(Environment.GetEnvironmentVariable("WIN_LENGTH")!);
        public static double ENEMY_MOVE_CHANCE = double.Parse(
            Environment.GetEnvironmentVariable("ENEMY_MOVE_CHANCE")!,
            CultureInfo.InvariantCulture);

        public static int FIRST_PLAYER = 1;
        public static int SECOND_PLAYER = 2;
    }
}
