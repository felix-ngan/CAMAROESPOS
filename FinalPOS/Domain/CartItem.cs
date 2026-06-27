using System;

namespace FinalPOS.Domain
{
    public class CartItem
    {
        public int Id { get; set; }
        public string TransactionNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountPercent { get; set; }
        public double Total { get; set; }
        public DateTime Date { get; set; }
        public string Cashier { get; set; }
        public string Status { get; set; }
    }
}
