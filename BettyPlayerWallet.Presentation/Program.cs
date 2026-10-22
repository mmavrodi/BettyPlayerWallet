using BettyPlayerWallet.Application.DI;
using BettyPlayerWallet.Presentation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddServices();
        services.AddScoped<ConsoleUIService>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var uiService = scope.ServiceProvider.GetRequiredService<ConsoleUIService>();
    await uiService.RunAsync();
}