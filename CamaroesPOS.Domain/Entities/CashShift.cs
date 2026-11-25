using System;

namespace CamaroesPOS.Domain.Entities
{
    public class CashShift
    {
        public int Id { get; set; }
        public string OpenedBy { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; } = DateTime.Now;
        public decimal OpeningFloat { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedBy { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal? CountedCash { get; set; }
        public decimal? Discrepancy { get; set; }
        public decimal TotalSales { get; set; }
        public decimal CashSales { get; set; }
        public decimal MobileMoneySales { get; set; }
        public decimal WalletSales { get; set; }
        public int TransactionCount { get; set; }
        public string Notes { get; set; } = string.Empty;

        public bool IsOpen => !ClosedAt.HasValue;
    }
}
