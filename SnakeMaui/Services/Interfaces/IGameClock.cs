namespace SnakeMaui.Services.Interfaces
{
    public interface IGameClock
    {
        event EventHandler? Tick;

        TimeSpan Interval { get; set; }

        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}
