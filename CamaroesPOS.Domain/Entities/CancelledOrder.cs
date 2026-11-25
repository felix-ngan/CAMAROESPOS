using System;

namespace CamaroesPOS.Domain.Entities
{
    public class CancelledOrder
    {
        public int Id { get; set; }
        public string TransNo { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal Total { get; set; }
        public DateTime SoldDate { get; set; }
        public DateTime CancelledAt { get; set; } = DateTime.Now;
        public string VoidBy { get; set; } = string.Empty;
        public string CancelledBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Action { get; set; } = "Remboursement";
        public bool WalletRefunded { get; set; }
    }
}
