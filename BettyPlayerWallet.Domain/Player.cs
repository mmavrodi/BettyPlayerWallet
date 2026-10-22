namespace BettyPlayerWallet.Domain
{
    public class Player
    {
        private readonly object _lock = new();

        public int Id { get; private set; }
        public Money Balance { get; private set; } = Money.Zero;

        public Player(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Player ID must be a positive integer.", nameof(id));
            Id = id;
        }

        public void Deposit(Money amount)
        {
            ArgumentNullException.ThrowIfNull(amount);

            if (amount.Amount <= 0)
                throw new ArgumentException("Deposit amount must be greater than zero.");

            lock (_lock)
            {
                Balance += amount;
            }
        }

        public void Withdraw(Money amount)
        {
            ArgumentNullException.ThrowIfNull(amount);

            if (amount.Amount <= 0)
                throw new ArgumentException("Withdrawal amount must be greater than zero.");

            lock (_lock)
            {
                if (amount > Balance)
                    throw new InvalidOperationException("Insufficient funds for withdrawal.");

                Balance -= amount;
            }
        }

        public void ApplyGameRound(Money betAmount, Money winAmount)
        {
            ArgumentNullException.ThrowIfNull(betAmount);
            ArgumentNullException.ThrowIfNull(winAmount);

            if (betAmount.Amount <= 0)
                throw new ArgumentException("Bet amount must be greater than zero.", nameof(betAmount));

            lock (_lock)
            {
                if (betAmount > Balance)
                    throw new InvalidOperationException("Insufficient balance to place this bet.");

                Balance = Balance - betAmount + winAmount;
            }
        }
    }
}
