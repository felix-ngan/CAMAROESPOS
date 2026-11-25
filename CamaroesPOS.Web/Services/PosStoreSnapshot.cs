using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public class PosStoreSnapshot
{
    public int SchemaVersion { get; set; } = 2;
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
    public List<CashShift> CashShifts { get; set; } = new();
    public List<AuditEntry> AuditLogs { get; set; } = new();
    public Store Store { get; set; } = new();
}
