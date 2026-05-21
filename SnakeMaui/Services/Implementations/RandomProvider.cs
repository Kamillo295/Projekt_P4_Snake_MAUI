using SnakeMaui.Services.Interfaces;

namespace SnakeMaui.Services.Implementations
{
    public sealed class RandomProvider : IRandomProvider
    {
        private readonly Random _random = new();

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
