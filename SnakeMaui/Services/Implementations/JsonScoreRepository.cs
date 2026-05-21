using System.Text.Json;
using SnakeMaui.Models;
using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.Services.Implementations
{
    public sealed class JsonScoreRepository : IScoreRepository
    {
        private const string ScoresFileName = "scores.json";
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private string ScoresPath => Path.Combine(FileSystem.AppDataDirectory, ScoresFileName);

        public async Task<IReadOnlyList<ScoreEntry>> GetBestScoresAsync(int limit, CancellationToken cancellationToken = default)
        {
            var scores = await ReadScoresAsync(cancellationToken);

            return scores
                .OrderByDescending(score => score.Score)
                .ThenBy(score => score.PlayedAt)
                .Take(limit)
                .ToArray();
        }

        public async Task AddScoreAsync(ScoreEntry scoreEntry, int limit, CancellationToken cancellationToken = default)
        {
            if (scoreEntry.Score <= 0)
            {
                return;
            }

            var scores = await ReadScoresAsync(cancellationToken);
            scores.Add(scoreEntry);

            var bestScores = scores
                .OrderByDescending(score => score.Score)
                .ThenBy(score => score.PlayedAt)
                .Take(limit)
                .ToArray();

            var json = JsonSerializer.Serialize(bestScores, _jsonOptions);
            Directory.CreateDirectory(FileSystem.AppDataDirectory);
            await File.WriteAllTextAsync(ScoresPath, json, cancellationToken);
        }

        private async Task<List<ScoreEntry>> ReadScoresAsync(CancellationToken cancellationToken)
        {
            await EnsureScoresFileExistsAsync(cancellationToken);

            try
            {
                await using var fileStream = File.OpenRead(ScoresPath);
                var scores = await JsonSerializer.DeserializeAsync<List<ScoreEntry>>(fileStream, _jsonOptions, cancellationToken);

                return scores ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
            catch (IOException)
            {
                return [];
            }
        }

        private async Task EnsureScoresFileExistsAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(ScoresPath))
            {
                return;
            }

            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            try
            {
                await using var packageStream = await FileSystem.OpenAppPackageFileAsync(ScoresFileName);
                await using var fileStream = File.Create(ScoresPath);
                await packageStream.CopyToAsync(fileStream, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                await File.WriteAllTextAsync(ScoresPath, "[]", cancellationToken);
            }
        }
    }
}
