using System;

namespace FinalPOS.Domain
{
    public class SoldItem
    {
        public int Id { get; set; }
        public string TransNo { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public DateTime SoldDate { get; set; }
        public string Cashier { get; set; }
    }
}
