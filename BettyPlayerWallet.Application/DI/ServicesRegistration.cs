using BettyPlayerWallet.Application.Interfaces;
using BettyPlayerWallet.Infrastructure.DI;
using Microsoft.Extensions.DependencyInjection;

namespace BettyPlayerWallet.Application.DI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddRepositories();
            services.AddScoped<IGameEngine, GameEngine>();
            services.AddScoped<IWalletService, WalletService>();
            return services;
        }
    }
}
