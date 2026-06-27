using System;

namespace FinalPOS.Domain
{
    public class CancelledOrder
    {
        public int Id { get; set; }
        public string TransNo { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public double Total { get; set; }
        public DateTime SoldDate { get; set; }
        public string VoidBy { get; set; }
        public string CancelledBy { get; set; }
        public string Reason { get; set; }
        public string Action { get; set; }
    }
}
