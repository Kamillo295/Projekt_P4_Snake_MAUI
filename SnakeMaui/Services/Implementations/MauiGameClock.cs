using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.Services.Implementations
{
    public sealed class MauiGameClock : IGameClock
    {
        private IDispatcherTimer? _timer;

        public event EventHandler? Tick;

        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(130);

        public bool IsRunning => _timer?.IsRunning == true;

        public void Start()
        {
            EnsureTimer();
            _timer!.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
        }

        private void EnsureTimer()
        {
            if (_timer is not null)
            {
                _timer.Interval = Interval;
                return;
            }

            var dispatcher = Application.Current?.Dispatcher
                ?? throw new InvalidOperationException("Brak aktywnego dispatchera MAUI.");

            _timer = dispatcher.CreateTimer();
            _timer.Interval = Interval;
            _timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}
