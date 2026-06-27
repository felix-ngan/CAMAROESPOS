using System;

namespace FinalPOS.Domain
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string TransNo { get; set; }
        public string Type { get; set; } // 'Credit' or 'Debit'
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
