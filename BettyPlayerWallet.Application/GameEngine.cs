using BettyPlayerWallet.Application.Interfaces;

namespace BettyPlayerWallet.Application
{
    public class GameEngine : IGameEngine
    {
        private readonly Random _random;

        public GameEngine(Random? random = null)
        {
            _random = random ?? Random.Shared;
        }

        public decimal CalculateWin(decimal betAmount)
        {
            int roll = _random.Next(1, 101);

            if (roll <= 50)
            {
                return 0m;
            }
            else if (roll <= 90)
            {
                double multiplier = _random.NextDouble() * 2.0;
                return Math.Round((decimal)multiplier * betAmount, 2);
            }
            else
            {
                double multiplier = 2.0 + (_random.NextDouble() * 8.0);
                return Math.Round((decimal)multiplier * betAmount, 2);
            }
        }
    }
}
