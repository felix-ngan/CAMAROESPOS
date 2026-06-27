namespace FinalPOS.Domain
{
    public class Product
    {
        public string ProductCode { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public int Reorder { get; set; }
    }
}
