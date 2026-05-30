namespace SnakeMaui.Models
{
    public sealed class GameOptions
    {
        public int BoardSize { get; init; } = 16;

        public int StartLength { get; init; } = 4;

        public int PointsPerFood { get; init; } = 10;
    }
}
