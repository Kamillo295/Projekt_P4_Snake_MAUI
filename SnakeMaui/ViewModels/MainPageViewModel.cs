using System.Collections.ObjectModel;
using System.Windows.Input;
using SnakeMaui.Models;
using SnakeMaui.Services.Implementations;
using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.ViewModels
{
    public sealed class MainPageViewModel : ObservableObject
    {
        private const int MaxScores = 10;
        private static readonly TimeSpan GameInterval = TimeSpan.FromMilliseconds(150);
        private readonly GameService _gameService;
        private readonly IScoreRepository _scoreRepository;
        private IDispatcherTimer? _gameTimer;
        private GameSnapshot _snapshot;
        private string _playerName = "Gracz";
        private string _statusMessage = "Gotowy do gry";
        private bool _isLoadingScores;
        private bool _scoreSavedForCurrentGame;

        public MainPageViewModel(GameService gameService, IScoreRepository scoreRepository)
        {
            _gameService = gameService;
            _scoreRepository = scoreRepository;
            _snapshot = gameService.Snapshot;

            StartCommand = new Command(StartGame);
            PauseCommand = new Command(TogglePause);
            _gameService.StateChanged += OnGameStateChanged;
        }

        public event EventHandler? BoardChanged;

        public ObservableCollection<ScoreEntry> HighScores { get; } = [];

        public ICommand StartCommand { get; }

        public ICommand PauseCommand { get; }

        public GameSnapshot Snapshot
        {
            get => _snapshot;
            private set
            {
                if (SetProperty(ref _snapshot, value))
                {
                    OnPropertyChanged(nameof(CurrentScore));
                    OnPropertyChanged(nameof(IsPauseEnabled));
                    OnPropertyChanged(nameof(PauseButtonText));
                    OnPropertyChanged(nameof(StartButtonText));
                    BoardChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int CurrentScore => Snapshot.Score;

        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsPauseEnabled => Snapshot.Status == GameStatus.Running || Snapshot.Status == GameStatus.Paused;

        public string PauseButtonText => Snapshot.Status == GameStatus.Paused ? "WZNOW" : "PAUZA";

        public string StartButtonText => Snapshot.Status == GameStatus.Running ? "OD NOWA" : "START";

        public async Task LoadAsync()
        {
            if (_isLoadingScores)
            {
                return;
            }

            _isLoadingScores = true;

            try
            {
                await RefreshScoresAsync();
            }
            finally
            {
                _isLoadingScores = false;
            }
        }

        public void ChangeDirection(Direction direction)
        {
            _gameService.ChangeDirection(direction);
        }

        private void StartGame()
        {
            _scoreSavedForCurrentGame = false;
            StatusMessage = "Gra trwa";
            _gameService.StartNewGame();
            StartTimer();
        }

        private void TogglePause()
        {
            if (Snapshot.Status == GameStatus.Running)
            {
                _gameService.Pause();
                StopTimer();
                StatusMessage = "Pauza";
                return;
            }

            if (Snapshot.Status == GameStatus.Paused)
            {
                _gameService.Resume();
                StartTimer();
                StatusMessage = "Gra trwa";
            }
        }

        private void StartTimer()
        {
            if (_gameTimer is null)
            {
                var dispatcher = Application.Current?.Dispatcher;

                if (dispatcher is null)
                {
                    throw new InvalidOperationException("Brak aktywnego dispatchera MAUI.");
                }

                _gameTimer = dispatcher.CreateTimer();
                _gameTimer.Interval = GameInterval;
                _gameTimer.Tick += (_, _) => _gameService.MoveNext();
            }

            _gameTimer.Start();
        }

        private void StopTimer()
        {
            _gameTimer?.Stop();
        }

        private async void OnGameStateChanged(object? sender, GameSnapshot snapshot)
        {
            Snapshot = snapshot;

            if (snapshot.Status == GameStatus.GameOver)
            {
                StopTimer();
                StatusMessage = $"Koniec gry. Wynik: {snapshot.Score}";
                await SaveCurrentScoreAsync(snapshot.Score);
            }
            else if (snapshot.Status == GameStatus.Ready)
            {
                StatusMessage = "Gotowy do gry";
            }
        }

        private async Task SaveCurrentScoreAsync(int score)
        {
            if (_scoreSavedForCurrentGame || score <= 0)
            {
                return;
            }

            _scoreSavedForCurrentGame = true;

            var entry = new ScoreEntry
            {
                PlayerName = string.IsNullOrWhiteSpace(PlayerName) ? "Gracz" : PlayerName.Trim(),
                Score = score,
                PlayedAt = DateTimeOffset.Now
            };

            await _scoreRepository.AddScoreAsync(entry, MaxScores);
            await RefreshScoresAsync();
        }

        private async Task RefreshScoresAsync()
        {
            var scores = await _scoreRepository.GetBestScoresAsync(MaxScores);
            HighScores.Clear();

            foreach (var score in scores)
            {
                HighScores.Add(score);
            }
        }
    }
}
