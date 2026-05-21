namespace SnakeMaui.Models
{
    public sealed class ScoreEntry
    {
        public string PlayerName { get; init; } = "Gracz";

        public int Score { get; init; }

        public DateTimeOffset PlayedAt { get; init; } = DateTimeOffset.Now;

        public string PlayedAtDisplay => PlayedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }
}
