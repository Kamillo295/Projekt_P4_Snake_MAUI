using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.Services.Implementations
{
    public sealed class MauiGameClock : IGameClock
    {
        private IDispatcherTimer? _timer;

        public event EventHandler? Tick;

        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(150);

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
            if (_timer is null)
            {
                var dispatcher = Application.Current?.Dispatcher;

                if (dispatcher is null)
                {
                    throw new InvalidOperationException("Brak aktywnego dispatchera MAUI.");
                }

                _timer = dispatcher.CreateTimer();
                _timer.Tick += OnTimerTick;
            }

            _timer.Interval = Interval;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}
