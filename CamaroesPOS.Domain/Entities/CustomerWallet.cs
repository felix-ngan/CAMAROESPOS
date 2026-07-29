namespace CamaroesPOS.Domain.Entities
{
    public class CustomerWallet
    {
        public string Phone { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public double Balance { get; set; }
        public int LoyaltyPoints { get; set; }
        public double TotalSpent { get; set; }
    }
}
