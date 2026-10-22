using BettyPlayerWallet.Domain;
using BettyPlayerWallet.Infrastructure;

namespace BettyPlayerWallet.Tests;

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
        var player = new Player { Id = 42 };

        var saveResult = await repository.SaveAsync(player);
        var result = await repository.GetByIdAsync(42);

        Assert.Equal(Task.CompletedTask, saveResult);
        Assert.Same(player, result);
    }

    [Fact]
    public async Task SaveAsync_WithExistingId_ReplacesStoredPlayer()
    {
        var repository = new PlayerRepository();
        var first = new Player { Id = 42 };
        var replacement = new Player { Id = 42 };

        await repository.SaveAsync(first);
        await repository.SaveAsync(replacement);

        Assert.Same(replacement, await repository.GetByIdAsync(42));
    }
}
