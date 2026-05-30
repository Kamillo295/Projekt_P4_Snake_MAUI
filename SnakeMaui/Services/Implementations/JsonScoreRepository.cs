using System.Text.Json;
using SnakeMaui.Models;
using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.Services.Implementations
{
    public sealed class JsonScoreRepository : IScoreRepository  // sealed - klasa nie moze byc dziedziczona, co jest dobrym wyborem dla tej klasy, poniewaz nie ma potrzeby jej rozszerzania i moze to poprawic wydajnosc.
    {
        private const string ScoresFileName = "scores.json";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,     // Umozliwia deserializacje niezaleznie od wielkosci liter w nazwach wlasciwosci
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true    // Ulatwia czytanie pliku JSON przez czlowieka
        };

        private string ScoresPath
        {
            get
            {
                return Path.Combine(FileSystem.AppDataDirectory, ScoresFileName);
            }
        }

        public async Task<IReadOnlyList<ScoreEntry>> GetBestScoresAsync(int limit, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<ScoreEntry> scores = await ReadScoresAsync(cancellationToken);
            return GetBestScores(scores, limit);
        }

        public async Task AddScoreAsync(ScoreEntry scoreEntry, int limit, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (scoreEntry.Score <= 0)
            {
                return;
            }

            List<ScoreEntry> scores = await ReadScoresAsync(cancellationToken);
            scores.Add(scoreEntry);

            List<ScoreEntry> bestScores = GetBestScores(scores, limit);
            await SaveScoresAsync(bestScores, cancellationToken);
        }

        private async Task<List<ScoreEntry>> ReadScoresAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(ScoresPath))
            {
                return new List<ScoreEntry>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(ScoresPath, cancellationToken);
                List<ScoreEntry>? scores = JsonSerializer.Deserialize<List<ScoreEntry>>(json, _jsonOptions);

                if (scores is null)
                {
                    return new List<ScoreEntry>();
                }

                return scores;
            }
            catch (JsonException)
            {
                return new List<ScoreEntry>();
            }
            catch (IOException)
            {
                return new List<ScoreEntry>();
            }
        }

        private async Task SaveScoresAsync(IReadOnlyList<ScoreEntry> scores, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            string json = JsonSerializer.Serialize(scores, _jsonOptions);
            await File.WriteAllTextAsync(ScoresPath, json, cancellationToken);
        }

        private static List<ScoreEntry> GetBestScores(IEnumerable<ScoreEntry> scores, int limit)
        {
            IEnumerable<ScoreEntry> orderedScores = scores
                .OrderByDescending(score => score.Score)
                .ThenBy(score => score.PlayedAt)
                .Take(limit);

            List<ScoreEntry> bestScores = orderedScores.ToList();
            return bestScores;
        }
    }
}
