namespace SnakeMaui.Models
{
    public sealed record GameSnapshot(  //sealed record jest użyty, aby zapobiec dziedziczeniu i modyfikacji tej klasy, co jest ważne dla zachowania integralności danych gry
        int BoardSize,
        IReadOnlyList<Position> Snake,
        Position Food,
        int Score,
        GameStatus Status);
}
