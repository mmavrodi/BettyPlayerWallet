using BettyPlayerWallet.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BettyPlayerWallet.Infrastructure.DI
{
    public static class RepositoriesRegistration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            return services;
        }
    }
}
