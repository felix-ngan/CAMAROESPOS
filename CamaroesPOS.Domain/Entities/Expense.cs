using System;

namespace CamaroesPOS.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AddedBy { get; set; } = string.Empty;
    }
}
