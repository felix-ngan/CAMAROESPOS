using System;

namespace CamaroesPOS.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        public string Cashier { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsTaxable { get; set; } = true;
    }
}
