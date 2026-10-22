using BettyPlayerWallet.Domain;

namespace BettyPlayerWallet.Application.DTO
{
    public record GameRoundResult(
        bool IsWin, 
        decimal WinAmount, 
        Money NewBalance);
}
