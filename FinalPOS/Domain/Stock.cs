using System;

namespace FinalPOS.Domain
{
    public class Stock
    {
        public int Id { get; set; }
        public string RefNo { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public int Qty { get; set; }
        public DateTime StockInDate { get; set; }
        public string StockInBy { get; set; }
        public string Status { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
    }
}
