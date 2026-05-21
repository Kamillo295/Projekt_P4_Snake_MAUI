namespace SnakeMaui
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();

            Items.Add(new ShellContent
            {
                Title = "Snake",
                Route = nameof(MainPage),
                Content = mainPage
            });
        }
    }
}
