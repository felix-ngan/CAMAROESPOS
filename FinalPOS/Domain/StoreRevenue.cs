using System;

namespace FinalPOS.Domain
{
    public class StoreRevenue
    {
        public int? StoreId { get; set; }
        public double Revenue { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
