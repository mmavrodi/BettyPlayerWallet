using BettyPlayerWallet.Application.DTO;
using BettyPlayerWallet.Application.Interfaces;
using BettyPlayerWallet.Domain;
using BettyPlayerWallet.Infrastructure.Interfaces;
using FakeItEasy;

namespace BettyPlayerWallet.Application.Tests
{
    public class WalletServiceTests
    {
        [Fact]
        public async Task GetCurrentBalanceAsync_ReturnsExistingPlayerBalance()
        {
            var player = FundedPlayer(12, 25m);
            var repository = A.Fake<IPlayerRepository>();
            A.CallTo(() => repository.GetOrCreateAsync(12, A<CancellationToken>._)).Returns(Task.FromResult(player));
            var service = new WalletService(repository, A.Fake<IGameEngine>());

            var result = await service.GetCurrentBalanceAsync(12);

            Assert.Equal(new Money(25m), result);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task GetCurrentBalanceAsync_CreatesMissingPlayer()
        {
            var repository = A.Fake<IPlayerRepository>();
            A.CallTo(() => repository.GetOrCreateAsync(12, A<CancellationToken>._)).Returns(Task.FromResult(new Player(12)));
            var service = new WalletService(repository, A.Fake<IGameEngine>());

            var result = await service.GetCurrentBalanceAsync(12);

            Assert.Equal(Money.Zero, result);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task DepositAsync_DepositsAndSavesExistingPlayer()
        {
            var player = FundedPlayer(12, 10m);
            var repository = RepositoryWithPlayer(12, player);
            var service = new WalletService(repository, A.Fake<IGameEngine>());
            var cancellationToken = new CancellationTokenSource().Token;

            var result = await service.DepositAsync(12, 7.25m, cancellationToken);

            Assert.Equal(new Money(17.25m), result);
            A.CallTo(() => repository.SaveAsync(player, cancellationToken)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DepositAsync_WithZeroAmount_ThrowsArgumentException()
        {
            var player = new Player(12);
            var repository = RepositoryWithPlayer(12, player);
            var service = new WalletService(repository, A.Fake<IGameEngine>());

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DepositAsync(12, 0m));

            Assert.Equal("Deposit amount must be greater than zero.", exception.Message);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task WithdrawAsync_WithdrawsAndSavesExistingPlayer()
        {
            var player = FundedPlayer(12, 20m);
            var repository = RepositoryWithPlayer(12, player);
            var service = new WalletService(repository, A.Fake<IGameEngine>());

            var result = await service.WithdrawAsync(12, 8m);

            Assert.Equal(new Money(12m), result);
            A.CallTo(() => repository.SaveAsync(player, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WithdrawAsync_WhenFundsAreInsufficient_ThrowsInvalidOperationException()
        {
            var player = FundedPlayer(12, 5m);
            var repository = RepositoryWithPlayer(12, player);
            var service = new WalletService(repository, A.Fake<IGameEngine>());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.WithdrawAsync(12, 6m));

            Assert.Equal("Insufficient funds for withdrawal.", exception.Message);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Theory]
        [InlineData(0.99)]
        [InlineData(10.01)]
        public async Task PlaySlotGameAsync_WithOutOfRangeBet_ThrowsArgumentOutOfRangeException(decimal bet)
        {
            var repository = A.Fake<IPlayerRepository>();
            var engine = A.Fake<IGameEngine>();
            var service = new WalletService(repository, engine);

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.PlaySlotGameAsync(12, bet));

            Assert.Equal("betAmount", exception.ParamName);
            A.CallTo(() => repository.GetOrCreateAsync(A<int>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => engine.CalculateWin(A<decimal>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PlaySlotGameAsync_WhenEngineReturnsNoWin_ReturnsLossAndUpdatedBalance()
        {
            var player = FundedPlayer(12, 20m);
            var repository = RepositoryWithPlayer(12, player);
            var engine = A.Fake<IGameEngine>();
            A.CallTo(() => engine.CalculateWin(5m)).Returns(0m);
            var service = new WalletService(repository, engine);

            var result = await service.PlaySlotGameAsync(12, 5m);

            Assert.Equal(new GameRoundResult(false, 0m, new Money(15m)), result);
            A.CallTo(() => engine.CalculateWin(5m)).MustHaveHappenedOnceExactly();
            A.CallTo(() => repository.SaveAsync(player, default)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PlaySlotGameAsync_WhenEngineReturnsWin_ReturnsWinAndUpdatedBalance()
        {
            var player = FundedPlayer(12, 20m);
            var repository = RepositoryWithPlayer(12, player);
            var engine = A.Fake<IGameEngine>();
            A.CallTo(() => engine.CalculateWin(5m)).Returns(7.5m);
            var service = new WalletService(repository, engine);

            var result = await service.PlaySlotGameAsync(12, 5m);

            Assert.Equal(new GameRoundResult(true, 7.5m, new Money(22.5m)), result);
        }

        [Fact]
        public async Task PlaySlotGameAsync_WhenBalanceIsInsufficient_ThrowsInvalidOperationException()
        {
            var player = FundedPlayer(12, 4m);
            var repository = RepositoryWithPlayer(12, player);
            var engine = A.Fake<IGameEngine>();
            A.CallTo(() => engine.CalculateWin(5m)).Returns(0m);
            var service = new WalletService(repository, engine);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaySlotGameAsync(12, 5m));

            Assert.Equal("Insufficient balance to place this bet.", exception.Message);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PlaySlotGameAsync_WhenEngineReturnsNegativeWin_ThrowsInvalidOperationException()
        {
            var player = FundedPlayer(12, 20m);
            var repository = RepositoryWithPlayer(12, player);
            var engine = A.Fake<IGameEngine>();
            A.CallTo(() => engine.CalculateWin(5m)).Returns(-1m);
            var service = new WalletService(repository, engine);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaySlotGameAsync(12, 5m));

            Assert.Equal("Game winnings cannot be negative.", exception.Message);
            Assert.Equal(new Money(20m), player.Balance);
            A.CallTo(() => repository.SaveAsync(A<Player>._, A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PlaySlotGameAsync_WhenCancellationIsRequestedBeforeEngineCall_ThrowsOperationCanceledException()
        {
            var player = FundedPlayer(12, 20m);
            var repository = RepositoryWithPlayer(12, player);
            var engine = A.Fake<IGameEngine>();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            var service = new WalletService(repository, engine);

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                service.PlaySlotGameAsync(12, 5m, cancellationSource.Token));

            A.CallTo(() => engine.CalculateWin(A<decimal>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PlaySlotGameAsync_ForMissingPlayer_CreatesPlayerAndForwardsCancellationTokenToLookupAndCreation()
        {
            var repository = A.Fake<IPlayerRepository>();
            A.CallTo(() => repository.GetOrCreateAsync(12, A<CancellationToken>._)).Returns(Task.FromResult(new Player(12)));
            var engine = A.Fake<IGameEngine>();
            A.CallTo(() => engine.CalculateWin(1m)).Returns(0m);
            var service = new WalletService(repository, engine);
            var cancellationToken = new CancellationTokenSource().Token;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaySlotGameAsync(12, 1m, cancellationToken));
            Assert.Equal("Insufficient balance to place this bet.", exception.Message);
            A.CallTo(() => repository.GetOrCreateAsync(12, cancellationToken)).MustHaveHappenedOnceExactly();
            A.CallTo(() => repository.SaveAsync(A<Player>._, cancellationToken)).MustNotHaveHappened();
        }

        private static Player FundedPlayer(int id, decimal amount)
        {
            var player = new Player(id);
            if (amount > 0)
                player.Deposit(new Money(amount));
            return player;
        }

        private static IPlayerRepository RepositoryWithPlayer(int id, Player player)
        {
            var repository = A.Fake<IPlayerRepository>();
            A.CallTo(() => repository.GetOrCreateAsync(id, A<CancellationToken>._)).Returns(Task.FromResult(player));
            return repository;
        }
    }
}
