using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public class PosStoreSnapshot
{
    public List<Product> Products { get; set; } = new();
    public List<Brand> Brands { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Vendor> Vendors { get; set; } = new();
    public List<CustomerWallet> CustomerWallets { get; set; } = new();
    public List<WalletTransaction> WalletTransactions { get; set; } = new();
    public List<SoldItem> SoldItems { get; set; } = new();
    public List<Stock> Stocks { get; set; } = new();
    public List<CancelledOrder> CancelledOrders { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public Store Store { get; set; } = new()
    {
        StoreName = "CAMAROES POS",
        Address = "Douala, Cameroun",
        Logo = Array.Empty<byte>(),
        MomoCode = "MTN-237-001",
        OrangeCode = "OM-237-002"
    };
}
