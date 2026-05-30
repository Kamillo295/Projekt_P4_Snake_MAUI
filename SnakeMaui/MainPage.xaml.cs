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
        private readonly SnakeBoardDrawable _boardDrawable = new SnakeBoardDrawable();
        private readonly Grid _rootGrid;
        private readonly Grid _contentGrid;
        private readonly VerticalStackLayout _boardSection;
        private readonly ScrollView _panelScrollView;
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
            _panelScrollView = FindRequiredElement<ScrollView>("PanelScrollView");
            _gameCanvas = FindRequiredElement<GraphicsView>("GameCanvas");
            _gameCanvas.Drawable = _boardDrawable;

            AddSwipeGestures();
            _viewModel.BoardChanged += OnBoardChanged;
            SizeChanged += OnPageSizeChanged;

            RenderBoard();
        }

        private void LoadPageXaml()     //takie coś bo w InitializeComponent() kompilator się pulta że niby nie działa na androidzie 
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
            if (_keyboardElement != null)
            {
                _keyboardElement.KeyDown -= OnWindowsKeyDown;
            }

            base.OnHandlerChanged();

            if (Handler == null)
            {
                return;
            }

            _keyboardElement = Handler.PlatformView as Microsoft.UI.Xaml.UIElement;

            if (_keyboardElement == null)
            {
                return;
            }

            _keyboardElement.KeyDown += OnWindowsKeyDown;

            Microsoft.UI.Xaml.Controls.Control? control = _keyboardElement as Microsoft.UI.Xaml.Controls.Control;

            if (control != null)
            {
                control.IsTabStop = true;
                control.Loaded += OnWindowsControlLoaded;
            }
        }

        private static void OnWindowsControlLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.Control? control = sender as Microsoft.UI.Xaml.Controls.Control;

            if (control != null)
            {
                control.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        }

        private void OnWindowsKeyDown(object sender, KeyRoutedEventArgs e)
        {
            bool handled = true;

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
            AddSwipeGesture(SwipeDirection.Up);
            AddSwipeGesture(SwipeDirection.Down);
            AddSwipeGesture(SwipeDirection.Left);
            AddSwipeGesture(SwipeDirection.Right);
        }

        private void AddSwipeGesture(SwipeDirection swipeDirection)
        {
            SwipeGestureRecognizer gesture = new SwipeGestureRecognizer();
            gesture.Direction = swipeDirection;
            gesture.Swiped += OnSwipe;
            _gameCanvas.GestureRecognizers.Add(gesture);
        }

        private void OnSwipe(object? sender, SwipedEventArgs e)
        {
            if (e.Direction == SwipeDirection.Up)
            {
                _viewModel.ChangeDirection(Direction.Up);
            }
            else if (e.Direction == SwipeDirection.Down)
            {
                _viewModel.ChangeDirection(Direction.Down);
            }
            else if (e.Direction == SwipeDirection.Left)
            {
                _viewModel.ChangeDirection(Direction.Left);
            }
            else if (e.Direction == SwipeDirection.Right)
            {
                _viewModel.ChangeDirection(Direction.Right);
            }
        }

        private T FindRequiredElement<T>(string name) where T : Element
        {
            T? element = this.FindByName<T>(name);

            if (element == null)
            {
                throw new InvalidOperationException($"Nie znaleziono elementu '{name}' w MainPage.xaml.");
            }

            return element;
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
            bool isNarrow = Width < 880;

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
                _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                Grid.SetColumn(_boardSection, 0);
                Grid.SetRow(_boardSection, 0);
                Grid.SetColumn(_panelScrollView, 0);
                Grid.SetRow(_panelScrollView, 1);
                return;
            }

            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(320)));
            _contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(_boardSection, 0);
            Grid.SetRow(_boardSection, 0);
            Grid.SetColumn(_panelScrollView, 1);
            Grid.SetRow(_panelScrollView, 0);
        }

        private void UpdateBoardSize()
        {
            if (Width <= 0)
            {
                return;
            }

            int horizontalPadding;

            if (Width < 720)
            {
                horizontalPadding = 16;
            }
            else
            {
                horizontalPadding = 24;
            }

            _rootGrid.Padding = new Thickness(horizontalPadding);

            double boardSize;

            if (_isNarrowLayout == true)
            {
                double availableWidth = Width - horizontalPadding * 2 - 26;
                double availableHeight = Height - 420;
                double availableBoardSpace = Math.Min(availableWidth, availableHeight);
                boardSize = Math.Clamp(availableBoardSpace, 200, 420);
            }
            else
            {
                double availableWidth = Width - horizontalPadding * 2 - 380;
                boardSize = Math.Clamp(availableWidth, 280, 560);
            }

            if (double.IsNaN(boardSize))
            {
                boardSize = 280;
            }

            _gameCanvas.WidthRequest = boardSize;
            _gameCanvas.HeightRequest = boardSize;
        }
    }
}
