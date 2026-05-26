using SnakeMaui.Drawing;
using SnakeMaui.Models;
using SnakeMaui.ViewModels;

#if WINDOWS
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace SnakeMaui
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly SnakeBoardDrawable _boardDrawable = new();
        private readonly Grid _rootGrid;
        private readonly Grid _contentGrid;
        private readonly VerticalStackLayout _boardSection;
        private readonly Border _panelSection;
        private readonly GraphicsView _gameCanvas;
        private bool? _isNarrowLayout;

#if WINDOWS
        private Microsoft.UI.Xaml.UIElement? _keyboardElement;
#endif

        public MainPage(MainPageViewModel viewModel)
        {
            LoadPageXaml();

            _viewModel = viewModel;
            BindingContext = viewModel;
            _rootGrid = FindRequiredElement<Grid>("RootGrid");
            _contentGrid = FindRequiredElement<Grid>("ContentGrid");
            _boardSection = FindRequiredElement<VerticalStackLayout>("BoardSection");
            _panelSection = FindRequiredElement<Border>("PanelSection");
            _gameCanvas = FindRequiredElement<GraphicsView>("GameCanvas");
            _gameCanvas.Drawable = _boardDrawable;

            AddSwipeGestures();
            _viewModel.BoardChanged += OnBoardChanged;
            SizeChanged += OnPageSizeChanged;

            RenderBoard();
        }

        private void LoadPageXaml()
        {
            Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof(MainPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
            RenderBoard();
        }

#if WINDOWS
        protected override void OnHandlerChanged()
        {
            if (_keyboardElement is not null)
            {
                _keyboardElement.KeyDown -= OnWindowsKeyDown;
            }

            base.OnHandlerChanged();

            _keyboardElement = Handler?.PlatformView as Microsoft.UI.Xaml.UIElement;

            if (_keyboardElement is null)
            {
                return;
            }

            _keyboardElement.KeyDown += OnWindowsKeyDown;

            if (_keyboardElement is Microsoft.UI.Xaml.Controls.Control control)
            {
                control.IsTabStop = true;
                control.Loaded += OnWindowsControlLoaded;
            }
        }

        private static void OnWindowsControlLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Control control)
            {
                control.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        }

        private void OnWindowsKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var handled = true;

            switch (e.Key)
            {
                case VirtualKey.Up:
                case VirtualKey.W:
                    _viewModel.ChangeDirection(Direction.Up);
                    break;
                case VirtualKey.Down:
                case VirtualKey.S:
                    _viewModel.ChangeDirection(Direction.Down);
                    break;
                case VirtualKey.Left:
                case VirtualKey.A:
                    _viewModel.ChangeDirection(Direction.Left);
                    break;
                case VirtualKey.Right:
                case VirtualKey.D:
                    _viewModel.ChangeDirection(Direction.Right);
                    break;
                default:
                    handled = false;
                    break;
            }

            e.Handled = handled;
        }
#endif

        private void AddSwipeGestures()
        {
            AddSwipeGesture(SwipeDirection.Up, Direction.Up);
            AddSwipeGesture(SwipeDirection.Down, Direction.Down);
            AddSwipeGesture(SwipeDirection.Left, Direction.Left);
            AddSwipeGesture(SwipeDirection.Right, Direction.Right);
        }

        private void AddSwipeGesture(SwipeDirection swipeDirection, Direction gameDirection)
        {
            var gesture = new SwipeGestureRecognizer { Direction = swipeDirection };
            gesture.Swiped += (_, _) => _viewModel.ChangeDirection(gameDirection);
            _gameCanvas.GestureRecognizers.Add(gesture);
        }

        private T FindRequiredElement<T>(string name) where T : Element
        {
            return this.FindByName<T>(name)
                ?? throw new InvalidOperationException($"Nie znaleziono elementu '{name}' w MainPage.xaml.");
        }

        private void OnBoardChanged(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(RenderBoard);
        }

        private void RenderBoard()
        {
            _boardDrawable.Snapshot = _viewModel.Snapshot;
            _gameCanvas.Invalidate();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            UpdateResponsiveLayout();
            UpdateBoardSize();
        }

        private void UpdateResponsiveLayout()
        {
            var isNarrow = Width < 880;

            if (_isNarrowLayout == isNarrow)
            {
                return;
            }

            _isNarrowLayout = isNarrow;
            _contentGrid.ColumnDefinitions.Clear();
            _contentGrid.RowDefinitions.Clear();

            if (isNarrow)
            {
                _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                Grid.SetColumn(_boardSection, 0);
                Grid.SetRow(_boardSection, 0);
                Grid.SetColumn(_panelSection, 0);
                Grid.SetRow(_panelSection, 1);
                return;
            }

            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(320)));
            _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(_boardSection, 0);
            Grid.SetRow(_boardSection, 0);
            Grid.SetColumn(_panelSection, 1);
            Grid.SetRow(_panelSection, 0);
        }

        private void UpdateBoardSize()
        {
            if (Width <= 0)
            {
                return;
            }

            var horizontalPadding = Width < 720 ? 16 : 24;
            _rootGrid.Padding = new Thickness(horizontalPadding);

            var availableWidth = _isNarrowLayout == true
                ? Width - horizontalPadding * 2 - 26
                : Width - horizontalPadding * 2 - 380;

            var boardSize = Math.Clamp(availableWidth, 280, 560);
            _gameCanvas.WidthRequest = boardSize;
            _gameCanvas.HeightRequest = boardSize;
        }
    }
}
