namespace CamaroesPOS.Domain.Entities
{
    public class CustomerWallet
    {
        public string Phone { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int LoyaltyPoints { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
