using BettyPlayerWallet.Application.Interfaces;
using System.Globalization;

namespace BettyPlayerWallet.Presentation
{
    public class ConsoleUIService
    {
        private readonly IWalletService _walletService;

        public ConsoleUIService(IWalletService walletService)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var playerId = await HandlePlayerSelectionAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.Write("\nPlease, submit action:\n> ");
                string? input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLowerInvariant();

                try
                {
                    switch (command)
                    {
                        case "deposit":
                            await HandleDepositAsync(playerId, parts, cancellationToken);
                            break;
                        case "withdraw":
                            await HandleWithdrawAsync(playerId, parts, cancellationToken);
                            break;
                        case "bet":
                            await HandleBetAsync(playerId, parts, cancellationToken);
                            break;
                        case "player":
                            playerId = await HandlePlayerSelectionAsync(cancellationToken);
                            break;
                        case "exit":
                            Console.WriteLine("Thank you for playing! Hope to see you again soon.");
                            return;
                        default:
                            Console.WriteLine("Unknown command. Available commands:player, deposit <amount>, withdraw <amount>, bet <amount>, exit");
                            break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                catch (Exception)
                {
                    Console.WriteLine("An unexpected error occurred. Please try again.");
                }
            }
        }

        private async Task<int> HandlePlayerSelectionAsync(CancellationToken cancellationToken)
        {
            int playerId = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.Write("\nSelect Player ID (1-1000):\n>");
                if (!int.TryParse(Console.ReadLine()?.Trim(), out playerId) || playerId < 1 || playerId > 1000)
                {
                    Console.WriteLine("Invalid Player ID. Please enter a number between 1 and 1000.");
                }
                else
                {
                    var currentBalance = await _walletService.GetCurrentBalanceAsync(playerId, cancellationToken);
                    Console.WriteLine($"Player {playerId} current balance is: {currentBalance}");
                }
            } while (playerId < 1 || playerId > 1000);

            return playerId;
        }

        private async Task HandleDepositAsync(int playerId, string[] parts, CancellationToken cancellationToken)
        {
            if (parts.Length < 2 || !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            {
                Console.WriteLine("Invalid format. Usage: deposit <amount>");
                return;
            }

            var currentBalance = await _walletService.DepositAsync(playerId, amount, cancellationToken);
            Console.WriteLine($"Your deposit of ${amount:N2} was successful. Player {playerId} current balance is: {currentBalance}");
        }

        private async Task HandleWithdrawAsync(int playerId, string[] parts, CancellationToken cancellationToken)
        {
            if (parts.Length < 2 || !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            {
                Console.WriteLine("Invalid format. Usage: withdraw <amount>");
                return;
            }

            var currentBalance = await _walletService.WithdrawAsync(playerId, amount, cancellationToken);
            Console.WriteLine($"Your withdrawal of ${amount:N2} was successful. Player {playerId} current balance is: {currentBalance}");
        }

        private async Task HandleBetAsync(int playerId, string[] parts, CancellationToken cancellationToken)
        {
            if (parts.Length < 2 || !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            {
                Console.WriteLine("Invalid format. Usage: bet <amount>");
                return;
            }

            var result = await _walletService.PlaySlotGameAsync(playerId, amount, cancellationToken);

            if (result.IsWin)
            {
                Console.WriteLine($"Congrats you won ${result.WinAmount:N2}! Player {playerId} current balance is: {result.NewBalance}");
            }
            else
            {
                Console.WriteLine($"No luck this time! Player {playerId} current balance is: {result.NewBalance}");
            }
        }
    }
}
