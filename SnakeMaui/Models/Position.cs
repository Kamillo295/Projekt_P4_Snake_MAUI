namespace SnakeMaui.Models
{
    public readonly record struct Position(int Row, int Column)
    {
        public Position Move(Direction direction)
        {
            return direction switch
            {
                Direction.Up => this with { Row = Row - 1 },
                Direction.Down => this with { Row = Row + 1 },
                Direction.Left => this with { Column = Column - 1 },
                Direction.Right => this with { Column = Column + 1 },
                _ => this
            };
        }
    }
}
