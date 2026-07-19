using System;

namespace CamaroesPOS.Domain.Entities
{
    public class StoreRevenue
    {
        public int? StoreId { get; set; }
        public double Revenue { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
