using BettyPlayerWallet.Application.DTO;
using BettyPlayerWallet.Application.Interfaces;
using BettyPlayerWallet.Domain;
using BettyPlayerWallet.Infrastructure.Interfaces;

namespace BettyPlayerWallet.Application
{
    public class WalletService : IWalletService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameEngine _gameEngine;

        public WalletService(IPlayerRepository playerRepository, IGameEngine gameEngine)
        {
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
        }

        public async Task<Money> GetCurrentBalanceAsync(int playerId, CancellationToken cancellationToken = default)
        {
            var player = await GetOrCreatePlayerAsync(playerId, cancellationToken).ConfigureAwait(false);
            return player.Balance;
        }

        public async Task<Money> DepositAsync(int playerId, decimal amount, CancellationToken cancellationToken = default)
        {
            var player = await GetOrCreatePlayerAsync(playerId, cancellationToken).ConfigureAwait(false);
            player.Deposit(new Money(amount));
            await _playerRepository.SaveAsync(player, cancellationToken).ConfigureAwait(false);
            return player.Balance;
        }

        public async Task<Money> WithdrawAsync(int playerId, decimal amount, CancellationToken cancellationToken = default)
        {
            var player = await GetOrCreatePlayerAsync(playerId, cancellationToken).ConfigureAwait(false);
            player.Withdraw(new Money(amount));
            await _playerRepository.SaveAsync(player, cancellationToken).ConfigureAwait(false);
            return player.Balance;
        }

        public async Task<GameRoundResult> PlaySlotGameAsync(int playerId, decimal betAmount, CancellationToken cancellationToken = default)
        {
            if (betAmount < 1m || betAmount > 10m)
                throw new ArgumentOutOfRangeException(nameof(betAmount), "The game accepts bets between $1 and $10.");

            var player = await GetOrCreatePlayerAsync(playerId, cancellationToken).ConfigureAwait(false);

            var betMoney = new Money(betAmount);
            cancellationToken.ThrowIfCancellationRequested();
            decimal rawWin = _gameEngine.CalculateWin(betAmount);
            if (rawWin < 0)
                throw new InvalidOperationException("Game winnings cannot be negative.");

            var winMoney = new Money(rawWin);

            player.ApplyGameRound(betMoney, winMoney);
            await _playerRepository.SaveAsync(player, cancellationToken).ConfigureAwait(false);

            return new GameRoundResult(winMoney.Amount > 0, winMoney.Amount, player.Balance);
        }

        private async Task<Player> GetOrCreatePlayerAsync(int playerId, CancellationToken cancellationToken)
        {
            return await _playerRepository.GetOrCreateAsync(playerId, cancellationToken).ConfigureAwait(false);
        }
    }
}
