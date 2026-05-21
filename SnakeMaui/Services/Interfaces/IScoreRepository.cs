using SnakeMaui.Models;

namespace SnakeMaui.Services.Interfaces
{
    public interface IScoreRepository
    {
        Task<IReadOnlyList<ScoreEntry>> GetBestScoresAsync(int limit, CancellationToken cancellationToken = default);

        Task AddScoreAsync(ScoreEntry scoreEntry, int limit, CancellationToken cancellationToken = default);
    }
}
