using SnakeMaui.Models;

namespace SnakeMaui.Services.Interfaces
{
    public interface IGameService
    {
        event EventHandler<GameSnapshot>? StateChanged;

        GameSnapshot Snapshot { get; }

        void StartNewGame();

        void ChangeDirection(Direction direction);

        void Pause();

        void Resume();

        void MoveNext();
    }
}
