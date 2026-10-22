using BettyPlayerWallet.Domain;

namespace BettyPlayerWallet.Infrastructure.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByIdAsync(int playerId, CancellationToken cancellationToken = default);
        Task<Player> GetOrCreateAsync(int playerId, CancellationToken cancellationToken = default);
        Task SaveAsync(Player player, CancellationToken cancellationToken = default);
    }
}
