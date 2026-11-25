using System;

namespace CamaroesPOS.Domain.Entities
{
    public class SoldItem
    {
        public int Id { get; set; }
        public string TransNo { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotalHT { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public DateTime SoldDate { get; set; }
        public string Cashier { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
    }
}
