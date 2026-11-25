using System;

namespace CamaroesPOS.Domain.Entities
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string TransNo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 'Credit', 'Debit', or 'Refund'
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
