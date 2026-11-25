namespace CamaroesPOS.Domain.Entities
{
    public class Store
    {
        public int Id { get; set; } = 1;
        public string StoreName { get; set; } = "CAMAROES POS";
        public string Address { get; set; } = "Douala, Cameroun";
        public string Phone { get; set; } = "+237 690 00 00 00";
        public string Niu { get; set; } = "M012612345678A";
        public string Rccm { get; set; } = "RC/DLA/2026/B/0421";
        public decimal TaxRate { get; set; } = 19.25m;
        public string Currency { get; set; } = "FCFA";
        public byte[] Logo { get; set; } = System.Array.Empty<byte>();
        public string MomoCode { get; set; } = "MTN-237-001";
        public string OrangeCode { get; set; } = "OM-237-002";
    }
}
