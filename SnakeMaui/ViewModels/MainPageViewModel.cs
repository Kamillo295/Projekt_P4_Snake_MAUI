using System.Collections.ObjectModel;
using System.Windows.Input;
using SnakeMaui.Models;
using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.ViewModels
{
    public sealed class MainPageViewModel : ObservableObject
    {
        private const int MaxScores = 10;
        private readonly IGameService _gameService;
        private readonly IScoreRepository _scoreRepository;
        private readonly IGameClock _gameClock;
        private GameSnapshot _snapshot;
        private string _playerName = "Gracz";
        private string _statusMessage = "Gotowy do gry";
        private bool _isBusy;
        private bool _scoreSavedForCurrentGame;

        public MainPageViewModel(
            IGameService gameService,
            IScoreRepository scoreRepository,
            IGameClock gameClock)
        {
            _gameService = gameService;
            _scoreRepository = scoreRepository;
            _gameClock = gameClock;
            _snapshot = gameService.Snapshot;

            StartCommand = new Command(async () => await StartAsync());
            PauseCommand = new Command(TogglePause);
            ChangeDirectionCommand = new Command<Direction>(ChangeDirection);

            _gameService.StateChanged += OnGameStateChanged;
            _gameClock.Tick += OnGameClockTick;
        }

        public event EventHandler? BoardChanged;

        public ObservableCollection<ScoreEntry> HighScores { get; } = [];

        public ICommand StartCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand ChangeDirectionCommand { get; }

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

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public bool IsPauseEnabled => Snapshot.Status is GameStatus.Running or GameStatus.Paused;

        public string PauseButtonText => Snapshot.Status == GameStatus.Paused ? "WZNÓW" : "PAUZA";

        public string StartButtonText => Snapshot.Status == GameStatus.Running ? "OD NOWA" : "START";

        public async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await RefreshScoresAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ChangeDirection(Direction direction)
        {
            _gameService.ChangeDirection(direction);
        }

        private async Task StartAsync()
        {
            _scoreSavedForCurrentGame = false;
            StatusMessage = "Gra trwa";
            _gameService.StartNewGame();
            _gameClock.Start();

            await Task.CompletedTask;
        }

        private void TogglePause()
        {
            if (Snapshot.Status == GameStatus.Running)
            {
                _gameService.Pause();
                _gameClock.Stop();
                StatusMessage = "Pauza";
                return;
            }

            if (Snapshot.Status == GameStatus.Paused)
            {
                _gameService.Resume();
                _gameClock.Start();
                StatusMessage = "Gra trwa";
            }
        }

        private void OnGameClockTick(object? sender, EventArgs e)
        {
            _gameService.MoveNext();
        }

        private async void OnGameStateChanged(object? sender, GameSnapshot snapshot)
        {
            Snapshot = snapshot;

            if (snapshot.Status == GameStatus.GameOver)
            {
                _gameClock.Stop();
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
