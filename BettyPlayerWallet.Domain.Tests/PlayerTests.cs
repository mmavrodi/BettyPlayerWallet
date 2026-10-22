namespace BettyPlayerWallet.Domain.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void NewPlayer_StartsWithZeroBalance()
        {
            var player = new Player(7);

            Assert.Equal(7, player.Id);
            Assert.Equal(Money.Zero, player.Balance);
        }

        [Fact]
        public void Deposit_IncreasesBalance()
        {
            var player = new Player(7);

            player.Deposit(new Money(25.50m));

            Assert.Equal(new Money(25.50m), player.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Deposit_WithNonPositiveAmount_ThrowsArgumentException(decimal amount)
        {
            var player = new Player(7);

            Assert.Throws<ArgumentException>(() => player.Deposit(new Money(amount)));
        }

        [Fact]
        public void Withdraw_DecreasesBalance()
        {
            var player = new Player(7);
            player.Deposit(new Money(30m));

            player.Withdraw(new Money(12.25m));

            Assert.Equal(new Money(17.75m), player.Balance);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Withdraw_WithNonPositiveAmount_ThrowsArgumentException(decimal amount)
        {
            var player = new Player(7);

            Assert.Throws<ArgumentException>(() => player.Withdraw(new Money(amount)));
        }

        [Fact]
        public void Withdraw_WhenFundsAreInsufficient_ThrowsInvalidOperationException()
        {
            var player = new Player(7);
            player.Deposit(new Money(10m));

            var exception = Assert.Throws<InvalidOperationException>(() => player.Withdraw(new Money(10.01m)));

            Assert.Equal("Insufficient funds for withdrawal.", exception.Message);
        }

        [Theory]
        [InlineData(10, 0, 35)]
        [InlineData(10, 25, 60)]
        public void ApplyGameRound_UpdatesBalance(decimal bet, decimal win, decimal expected)
        {
            var player = new Player(7);
            player.Deposit(new Money(45m));

            player.ApplyGameRound(new Money(bet), new Money(win));

            Assert.Equal(new Money(expected), player.Balance);
        }

        [Fact]
        public void ApplyGameRound_WhenBetExceedsBalance_ThrowsInvalidOperationException()
        {
            var player = new Player(7);
            player.Deposit(new Money(10m));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                player.ApplyGameRound(new Money(10.01m), Money.Zero));

            Assert.Equal("Insufficient balance to place this bet.", exception.Message);
        }

        [Fact]
        public void ApplyGameRound_WithZeroBet_ThrowsArgumentException()
        {
            var player = new Player(7);

            var exception = Assert.Throws<ArgumentException>(() => player.ApplyGameRound(Money.Zero, Money.Zero));

            Assert.Equal("betAmount", exception.ParamName);
        }

        [Fact]
        public void ApplyGameRound_WithNullArgument_ThrowsArgumentNullException()
        {
            var player = new Player(7);

            Assert.Throws<ArgumentNullException>(() => player.ApplyGameRound(null!, Money.Zero));
            Assert.Throws<ArgumentNullException>(() => player.ApplyGameRound(new Money(1m), null!));
        }

        [Fact]
        public void Money_UsesAwayFromZeroRounding()
        {
            Assert.Equal(new Money(1.01m), new Money(1.005m));
        }
    }
}
