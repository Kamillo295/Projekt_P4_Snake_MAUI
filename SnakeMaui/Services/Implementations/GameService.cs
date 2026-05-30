using SnakeMaui.Models;

namespace SnakeMaui.Services.Implementations
{
    public sealed class GameService
    {
        private readonly GameOptions _options;
        private readonly Random _random = new();
        private readonly List<Position> _snake = [];
        private Direction _currentDirection = Direction.Right;
        private Direction _pendingDirection = Direction.Right;
        private bool _hasPendingDirectionChange;
        private Position _food;
        private int _score;
        private GameStatus _status;

        public GameService(GameOptions options)
        {
            _options = options;
            ResetBoard(GameStatus.Ready);
        }

        public event EventHandler<GameSnapshot>? StateChanged;

        public GameSnapshot Snapshot => CreateSnapshot();

        public void StartNewGame()
        {
            ResetBoard(GameStatus.Running);
            PublishState();
        }

        public void ChangeDirection(Direction direction)
        {
            if (_status != GameStatus.Running ||
                _hasPendingDirectionChange ||
                direction == _currentDirection ||
                IsOpposite(direction, _currentDirection))
            {
                return;
            }

            _pendingDirection = direction;
            _hasPendingDirectionChange = true;
        }

        public void Pause()
        {
            if (_status != GameStatus.Running)
            {
                return;
            }

            _status = GameStatus.Paused;
            PublishState();
        }

        public void Resume()
        {
            if (_status != GameStatus.Paused)
            {
                return;
            }

            _status = GameStatus.Running;
            PublishState();
        }

        public void MoveNext()
        {
            if (_status != GameStatus.Running)
            {
                return;
            }

            _currentDirection = _pendingDirection;
            _hasPendingDirectionChange = false;

            var nextHead = _snake[0].Move(_currentDirection);
            var eatsFood = nextHead == _food;

            if (IsOutsideBoard(nextHead) || HitsSnake(nextHead, eatsFood))
            {
                _status = GameStatus.GameOver;
                PublishState();
                return;
            }

            _snake.Insert(0, nextHead);

            if (eatsFood)
            {
                _score += _options.PointsPerFood;
                _food = CreateFood();
            }
            else
            {
                _snake.RemoveAt(_snake.Count - 1);
            }

            PublishState();
        }

        private void ResetBoard(GameStatus status)
        {
            _snake.Clear();
            _currentDirection = Direction.Right;
            _pendingDirection = Direction.Right;
            _hasPendingDirectionChange = false;
            _score = 0;
            _status = status;

            var center = _options.BoardSize / 2;

            for (var i = 0; i < _options.StartLength; i++)
            {
                _snake.Add(new Position(center, center - i));
            }

            _food = CreateFood();
        }

        private Position CreateFood()
        {
            var freePositions = new List<Position>();

            for (var row = 0; row < _options.BoardSize; row++)
            {
                for (var column = 0; column < _options.BoardSize; column++)
                {
                    var position = new Position(row, column);

                    if (!_snake.Contains(position))
                    {
                        freePositions.Add(position);
                    }
                }
            }

            if (freePositions.Count == 0)
            {
                _status = GameStatus.GameOver;
                return _snake[0];
            }

            return freePositions[_random.Next(0, freePositions.Count)];
        }

        private bool HitsSnake(Position nextHead, bool eatsFood)
        {
            var checkedLength = eatsFood ? _snake.Count : _snake.Count - 1;

            for (var i = 0; i < checkedLength; i++)
            {
                if (_snake[i] == nextHead)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOutsideBoard(Position position)
        {
            return position.Row < 0 ||
                position.Column < 0 ||
                position.Row >= _options.BoardSize ||
                position.Column >= _options.BoardSize;
        }

        private static bool IsOpposite(Direction first, Direction second)
        {
            return first == Direction.Up && second == Direction.Down ||
                first == Direction.Down && second == Direction.Up ||
                first == Direction.Left && second == Direction.Right ||
                first == Direction.Right && second == Direction.Left;
        }

        private GameSnapshot CreateSnapshot()
        {
            return new GameSnapshot(_options.BoardSize, _snake.ToArray(), _food, _score, _status);
        }

        private void PublishState()
        {
            StateChanged?.Invoke(this, CreateSnapshot());
        }
    }
}
