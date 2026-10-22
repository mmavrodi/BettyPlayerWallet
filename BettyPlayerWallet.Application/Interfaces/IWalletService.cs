using BettyPlayerWallet.Application.DTO;
using BettyPlayerWallet.Domain;

namespace BettyPlayerWallet.Application.Interfaces
{
    public interface IWalletService
    {
        Task<Money> GetCurrentBalanceAsync(int playerId, CancellationToken cancellationToken = default);
        Task<Money> DepositAsync(int playerId, decimal amount, CancellationToken cancellationToken = default);
        Task<Money> WithdrawAsync(int playerId, decimal amount, CancellationToken cancellationToken = default);
        Task<GameRoundResult> PlaySlotGameAsync(int playerId, decimal betAmount, CancellationToken cancellationToken = default);
    }
}
