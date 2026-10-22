using BettyPlayerWallet.Domain;
using BettyPlayerWallet.Infrastructure.Interfaces;

namespace BettyPlayerWallet.Infrastructure
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<int, Player> _store = new();
        private readonly object _lock = new();

        public Task<Player?> GetByIdAsync(int playerId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                _store.TryGetValue(playerId, out var player);
                return Task.FromResult(player);
            }
        }

        public Task<Player> GetOrCreateAsync(int playerId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                if (!_store.TryGetValue(playerId, out var player))
                {
                    player = new Player(playerId);
                    _store[playerId] = player;
                }

                return Task.FromResult(player);
            }
        }

        public Task SaveAsync(Player player, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(player);
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                _store[player.Id] = player;
                return Task.CompletedTask;
            }
        }
    }
}
