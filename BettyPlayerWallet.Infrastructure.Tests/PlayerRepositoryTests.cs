using BettyPlayerWallet.Domain;

namespace BettyPlayerWallet.Infrastructure.Tests
{
    public class PlayerRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenPlayerDoesNotExist_ReturnsNull()
        {
            var repository = new PlayerRepository();

            var result = await repository.GetByIdAsync(42);

            Assert.Null(result);
        }

        [Fact]
        public async Task SaveAsync_PersistsPlayerForLookup()
        {
            var repository = new PlayerRepository();
            var player = new Player(42);

            await repository.SaveAsync(player);
            var result = await repository.GetByIdAsync(42);

            Assert.Same(player, result);
        }

        [Fact]
        public async Task SaveAsync_WithExistingId_ReplacesStoredPlayer()
        {
            var repository = new PlayerRepository();
            var first = new Player(42);
            var replacement = new Player(42);

            await repository.SaveAsync(first);
            await repository.SaveAsync(replacement);

            Assert.Same(replacement, await repository.GetByIdAsync(42));
        }

        [Fact]
        public async Task GetOrCreateAsync_ReturnsTheSamePlayerForConcurrentCalls()
        {
            var repository = new PlayerRepository();

            var players = await Task.WhenAll(Enumerable.Range(0, 50)
                .Select(_ => repository.GetOrCreateAsync(42)));

            Assert.All(players, player => Assert.Same(players[0], player));
        }

        [Fact]
        public async Task SaveAsync_WithNullPlayer_ThrowsArgumentNullException()
        {
            var repository = new PlayerRepository();

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(null!));
        }
    }
}
