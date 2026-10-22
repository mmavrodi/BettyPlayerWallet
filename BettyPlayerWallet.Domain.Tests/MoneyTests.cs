namespace BettyPlayerWallet.Domain.Tests
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_RoundsAmountToTwoDecimalPlaces()
        {
            var money = new Money(12.346m);

            Assert.Equal(12.35m, money.Amount);
        }

        [Fact]
        public void Constructor_WithNegativeAmount_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new Money(-1m));

            Assert.Equal("amount", exception.ParamName);
            Assert.Equal("Amount cannot be negative. (Parameter 'amount')", exception.Message);
        }

        [Fact]
        public void Zero_ReturnsZeroMoney()
        {
            Assert.Equal(new Money(0m), Money.Zero);
        }

        [Fact]
        public void AdditionAndSubtraction_ReturnExpectedMoney()
        {
            var first = new Money(10.25m);
            var second = new Money(2.75m);

            Assert.Equal(new Money(13m), first + second);
            Assert.Equal(new Money(7.5m), first - second);
        }

        [Fact]
        public void ComparisonOperators_ReturnExpectedResults()
        {
            var lower = new Money(5m);
            var higher = new Money(10m);

            Assert.True(lower < higher);
            Assert.True(higher > lower);
            Assert.True(lower <= higher);
            Assert.True(lower <= new Money(5m));
            Assert.True(higher >= lower);
            Assert.True(higher >= new Money(10m));
            Assert.False(higher < lower);
            Assert.False(lower > higher);
            Assert.False(higher <= lower);
            Assert.False(lower >= higher);
        }

        [Fact]
        public void EqualAmounts_HaveRecordValueEquality()
        {
            Assert.Equal(new Money(4.2m), new Money(4.20m));
        }

        [Fact]
        public void ToString_UsesCurrencyAndTwoDecimalPlaces()
        {
            Assert.Equal("$1,234.50", new Money(1234.5m).ToString());
        }
    }
}
