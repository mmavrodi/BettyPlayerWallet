namespace BettyPlayerWallet.Application.Tests
{
    public class GameEngineTests
    {
        [Fact]
        public void Constructor_WithoutRandomSource_UsesDefaultRandomSource()
        {
            var engine = new GameEngine();

            var result = engine.CalculateWin(5m);

            Assert.True(result >= 0m);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        public void CalculateWin_ForLossRoll_ReturnsZero(int roll)
        {
            var engine = CreateEngine(roll);

            var result = engine.CalculateWin(10m);

            Assert.Equal(0m, result);
        }

        [Theory]
        [InlineData(51)]
        [InlineData(90)]
        public void CalculateWin_ForStandardWinRoll_ReturnsRoundedStandardWin(int roll)
        {
            var engine = CreateEngine(roll, 0.555);

            var result = engine.CalculateWin(3m);

            Assert.Equal(3.33m, result);
        }

        [Theory]
        [InlineData(91)]
        [InlineData(100)]
        public void CalculateWin_ForHighWinRoll_ReturnsRoundedHighWin(int roll)
        {
            var engine = CreateEngine(roll, 0.25);

            var result = engine.CalculateWin(4m);

            Assert.Equal(16m, result);
        }

        private static GameEngine CreateEngine(int roll, double multiplier = 0)
        {
            return new GameEngine(new StubRandom(roll, multiplier));
        }

        private sealed class StubRandom(int roll, double multiplier) : Random
        {
            public override int Next(int minValue, int maxValue) => roll;

            public override double NextDouble() => multiplier;
        }
    }
}
