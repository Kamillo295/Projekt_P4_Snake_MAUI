namespace SnakeMaui.Models
{
    public sealed record GameSnapshot(
        int BoardSize,
        IReadOnlyList<Position> Snake,
        Position Food,
        int Score,
        GameStatus Status);
}
