namespace BettyPlayerWallet.Domain
{
    public record Money
    {
        public decimal Amount { get; init; }

        public Money(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        public static Money Zero => new(0m);

        public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
        public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);
        public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
        public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
        public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;
        public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;

        public override string ToString() => $"${Amount:N2}";
    }
}
