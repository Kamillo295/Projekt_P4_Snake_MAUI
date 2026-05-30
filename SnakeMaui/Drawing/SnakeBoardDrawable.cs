using SnakeMaui.Models;

namespace SnakeMaui.Drawing
{
    public sealed class SnakeBoardDrawable : IDrawable
    {
        private static readonly Color BoardColor = Color.FromArgb("#111812");
        private static readonly Color GridColor = Color.FromArgb("#223026");
        private static readonly Color SnakeColor = Color.FromArgb("#67C96B");
        private static readonly Color SnakeHeadColor = Color.FromArgb("#D7F75B");
        private static readonly Color FoodColor = Color.FromArgb("#EF5B5B");
        private static readonly Color TextColor = Color.FromArgb("#F4F7EE");

        public GameSnapshot? Snapshot { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = BoardColor;
            canvas.FillRectangle(dirtyRect);

            if (Snapshot is null)
            {
                return;
            }

            var boardSize = Math.Min(dirtyRect.Width, dirtyRect.Height);
            var cellSize = boardSize / Snapshot.BoardSize;    
            var boardLeft = dirtyRect.Left + (dirtyRect.Width - boardSize) / 2;
            var boardTop = dirtyRect.Top + (dirtyRect.Height - boardSize) / 2;

            DrawGrid(canvas, Snapshot.BoardSize, boardLeft, boardTop, boardSize, cellSize);
            DrawFood(canvas, Snapshot.Food, boardLeft, boardTop, cellSize);
            DrawSnake(canvas, Snapshot.Snake, boardLeft, boardTop, cellSize);

            if (Snapshot.Status is GameStatus.Ready or GameStatus.GameOver)
            {
                DrawOverlay(canvas, dirtyRect);
            }
        }

        private static void DrawGrid(ICanvas canvas, int size, float left, float top, float boardSize, float cellSize)
        {
            canvas.StrokeColor = GridColor;
            canvas.StrokeSize = 1;

            for (var index = 0; index <= size; index++)
            {
                var offset = index * cellSize;
                canvas.DrawLine(left + offset, top, left + offset, top + boardSize);
                canvas.DrawLine(left, top + offset, left + boardSize, top + offset);
            }
        }

        private static void DrawFood(ICanvas canvas, Position food, float left, float top, float cellSize)
        {
            var padding = cellSize * 0.18f;
            canvas.FillColor = FoodColor;
            canvas.FillEllipse(
                left + food.Column * cellSize + padding,
                top + food.Row * cellSize + padding,
                cellSize - padding * 2,
                cellSize - padding * 2);
        }

        private static void DrawSnake(ICanvas canvas, IReadOnlyList<Position> snake, float left, float top, float cellSize)
        {
            for (var index = snake.Count - 1; index >= 0; index--)
            {
                var segment = snake[index];
                var padding = cellSize * 0.08f;
                canvas.FillColor = index == 0 ? SnakeHeadColor : SnakeColor;
                canvas.FillRoundedRectangle(
                    left + segment.Column * cellSize + padding,
                    top + segment.Row * cellSize + padding,
                    cellSize - padding * 2,
                    cellSize - padding * 2,
                    cellSize * 0.22f);
            }
        }

        private static void DrawOverlay(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Color.FromRgba(0, 0, 0, 120);
            canvas.FillRectangle(dirtyRect);
            canvas.FontColor = TextColor;
            canvas.FontSize = 24;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.DrawString("START", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
