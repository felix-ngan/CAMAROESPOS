namespace CamaroesPOS.Domain.Entities
{
    public class Product
    {
        public string ProductCode { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public int Reorder { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public bool IsTaxable { get; set; } = true;
    }
}
