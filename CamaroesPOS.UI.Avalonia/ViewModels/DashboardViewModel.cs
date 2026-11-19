using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private double _todaySales;
    public double TodaySales { get => _todaySales; set { _todaySales = value; OnPropertyChanged(); } }

    private int _totalProducts;
    public int TotalProducts { get => _totalProducts; set { _totalProducts = value; OnPropertyChanged(); } }

    private int _totalStockQuantity;
    public int TotalStockQuantity { get => _totalStockQuantity; set { _totalStockQuantity = value; OnPropertyChanged(); } }

    private List<User> _allUsers = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); FilterUsers(); }
    }

    private ObservableCollection<User> _usersList = new();
    public ObservableCollection<User> UsersList { get => _usersList; set { _usersList = value; OnPropertyChanged(); } }

    private int _lowStockItems;
    public int LowStockItems { get => _lowStockItems; set { _lowStockItems = value; OnPropertyChanged(); } }

    private bool _hasCriticalStock;
    public bool HasCriticalStock { get => _hasCriticalStock; set { _hasCriticalStock = value; OnPropertyChanged(); } }

    private ObservableCollection<Product> _criticalStockList = new();
    public ObservableCollection<Product> CriticalStockList { get => _criticalStockList; set { _criticalStockList = value; OnPropertyChanged(); } }

    private double _totalWalletsBalance;
    public double TotalWalletsBalance { get => _totalWalletsBalance; set { _totalWalletsBalance = value; OnPropertyChanged(); } }

    private int _totalCustomers;
    public int TotalCustomers { get => _totalCustomers; set { _totalCustomers = value; OnPropertyChanged(); } }

    private int _totalLoyaltyPoints;
    public int TotalLoyaltyPoints { get => _totalLoyaltyPoints; set { _totalLoyaltyPoints = value; OnPropertyChanged(); } }

    private double _totalSalesAllTime;
    public double TotalSalesAllTime { get => _totalSalesAllTime; set { _totalSalesAllTime = value; OnPropertyChanged(); } }

    private int _totalItemsSoldToday;
    public int TotalItemsSoldToday { get => _totalItemsSoldToday; set { _totalItemsSoldToday = value; OnPropertyChanged(); } }

    private int _totalItemsSoldAllTime;
    public int TotalItemsSoldAllTime { get => _totalItemsSoldAllTime; set { _totalItemsSoldAllTime = value; OnPropertyChanged(); } }

    private double _totalCancelledAmount;
    public double TotalCancelledAmount { get => _totalCancelledAmount; set { _totalCancelledAmount = value; OnPropertyChanged(); } }

    private int _totalUsers;
    public int TotalUsers { get => _totalUsers; set { _totalUsers = value; OnPropertyChanged(); } }

    private int _adminCount;
    public int AdminCount { get => _adminCount; set { _adminCount = value; OnPropertyChanged(); } }

    private int _cashierCount;
    public int CashierCount { get => _cashierCount; set { _cashierCount = value; OnPropertyChanged(); } }

    private double _totalDiscountAmount;
    public double TotalDiscountAmount { get => _totalDiscountAmount; set { _totalDiscountAmount = value; OnPropertyChanged(); } }

    private int _totalVendors;
    public int TotalVendors { get => _totalVendors; set { _totalVendors = value; OnPropertyChanged(); } }

    private double _totalCogs;
    public double TotalCOGS { get => _totalCogs; set { _totalCogs = value; OnPropertyChanged(); } }

    private double _totalExpenses;
    public double TotalExpensesDashboard { get => _totalExpenses; set { _totalExpenses = value; OnPropertyChanged(); } }

    private double _netProfit;
    public double NetProfit { get => _netProfit; set { _netProfit = value; OnPropertyChanged(); } }

    private ISeries[] _salesChart = Array.Empty<ISeries>();
    public ISeries[] SalesChart { get => _salesChart; set { _salesChart = value; OnPropertyChanged(); } }

    private ISeries[] _todaySalesChart = Array.Empty<ISeries>();
    public ISeries[] TodaySalesChart { get => _todaySalesChart; set { _todaySalesChart = value; OnPropertyChanged(); } }

    private ISeries[] _totalProductsChart = Array.Empty<ISeries>();
    public ISeries[] TotalProductsChart { get => _totalProductsChart; set { _totalProductsChart = value; OnPropertyChanged(); } }

    private ISeries[] _stockMovementsChart = Array.Empty<ISeries>();
    public ISeries[] StockMovementsChart { get => _stockMovementsChart; set { _stockMovementsChart = value; OnPropertyChanged(); } }

    private ISeries[] _lowStockChart = Array.Empty<ISeries>();
    public ISeries[] LowStockChart { get => _lowStockChart; set { _lowStockChart = value; OnPropertyChanged(); } }

    private ISeries[] _walletsChart = Array.Empty<ISeries>();
    public ISeries[] WalletsChart { get => _walletsChart; set { _walletsChart = value; OnPropertyChanged(); } }

    private ISeries[] _itemsSoldTodayChart = Array.Empty<ISeries>();
    public ISeries[] ItemsSoldTodayChart { get => _itemsSoldTodayChart; set { _itemsSoldTodayChart = value; OnPropertyChanged(); } }

    private ISeries[] _cancelledChart = Array.Empty<ISeries>();
    public ISeries[] CancelledChart { get => _cancelledChart; set { _cancelledChart = value; OnPropertyChanged(); } }

    private ISeries[] _usersChart = Array.Empty<ISeries>();
    public ISeries[] UsersChart { get => _usersChart; set { _usersChart = value; OnPropertyChanged(); } }

    private ISeries[] _discountChart = Array.Empty<ISeries>();
    public ISeries[] DiscountChart { get => _discountChart; set { _discountChart = value; OnPropertyChanged(); } }

    private ISeries[] _vendorsChart = Array.Empty<ISeries>();
    public ISeries[] VendorsChart { get => _vendorsChart; set { _vendorsChart = value; OnPropertyChanged(); } }

    private ISeries[] _profitChart = Array.Empty<ISeries>();
    public ISeries[] ProfitChart { get => _profitChart; set { _profitChart = value; OnPropertyChanged(); } }

    private ISeries[] _paymentModesChart = Array.Empty<ISeries>();
    public ISeries[] PaymentModesChart { get => _paymentModesChart; set { _paymentModesChart = value; OnPropertyChanged(); } }

    private Axis[] _daysXAxes = Array.Empty<Axis>();
    public Axis[] DaysXAxes { get => _daysXAxes; set { _daysXAxes = value; OnPropertyChanged(); } }

    private Axis[] _productsXAxes = Array.Empty<Axis>();
    public Axis[] ProductsXAxes { get => _productsXAxes; set { _productsXAxes = value; OnPropertyChanged(); } }

    private Axis[] _walletsYAxes = Array.Empty<Axis>();
    public Axis[] WalletsYAxes { get => _walletsYAxes; set { _walletsYAxes = value; OnPropertyChanged(); } }

    private Axis[] _itemsSoldXAxes = Array.Empty<Axis>();
    public Axis[] ItemsSoldXAxes { get => _itemsSoldXAxes; set { _itemsSoldXAxes = value; OnPropertyChanged(); } }

    private Axis[] _profitXAxes = Array.Empty<Axis>();
    public Axis[] ProfitXAxes { get => _profitXAxes; set { _profitXAxes = value; OnPropertyChanged(); } }

    public DashboardViewModel()
    {
        LoadStats();
    }

    private void FilterUsers()
    {
        UsersList.Clear();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var u in _allUsers) UsersList.Add(u);
            return;
        }

        var lower = SearchText.ToLowerInvariant();
        foreach (var u in _allUsers.Where(u =>
            (u.Username != null && u.Username.ToLowerInvariant().Contains(lower)) ||
            (u.Role != null && u.Role.ToLowerInvariant().Contains(lower)) ||
            (u.Name != null && u.Name.ToLowerInvariant().Contains(lower))))
        {
            UsersList.Add(u);
        }
    }

    public void LoadStats()
    {
        using var context = new PosDbContext();
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        // 1 & 2. Ventes du jour et Chiffre d'affaires
        TodaySales = context.SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => (double?)s.Total) ?? 0;
        TotalSalesAllTime = context.SoldItems.Sum(s => (double?)s.Total) ?? 0;
        var yesterdaySales = context.SoldItems.Where(s => s.SoldDate >= yesterday && s.SoldDate < today).Sum(s => (double?)s.Total) ?? 0;

        TodaySalesChart = new ISeries[]
        {
            new PieSeries<double> { Name = "Aujourd'hui", Values = new[] { TodaySales }, Fill = new SolidColorPaint(SKColors.DodgerBlue) },
            new PieSeries<double> { Name = "Hier", Values = new[] { yesterdaySales }, Fill = new SolidColorPaint(SKColors.LightGray) }
        };

        var salesValues = new double[7];
        var dayLabels = new string[7];
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            salesValues[6 - i] = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (double?)s.Total) ?? 0;
            dayLabels[6 - i] = d.ToString("dd MMM");
        }
        DaysXAxes = new[] { new Axis { Labels = dayLabels } };
        SalesChart = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Ventes",
                Values = salesValues,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 3),
                Fill = new SolidColorPaint(new SKColor(30, 136, 229, 45)),
                GeometrySize = 8
            }
        };

        // 3. Total produits & Top 5 Stock
        TotalProducts = context.Products.Count();
        TotalStockQuantity = context.Products.Sum(p => (int?)p.Qty) ?? 0;
        var topProducts = context.Products.OrderByDescending(p => p.Qty).Take(5).ToList();
        ProductsXAxes = new[] { new Axis { Labels = topProducts.Select(p => p.ProductCode ?? "").ToArray() } };
        TotalProductsChart = new ISeries[]
        {
            new ColumnSeries<int> { Name = "Stock", Values = topProducts.Select(p => p.Qty).ToArray(), Fill = new SolidColorPaint(SKColors.MediumPurple) }
        };

        // Mouvements de Stock (7 j)
        var stockInValues = new int[7];
        var stockOutValues = new int[7];
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            stockInValues[6 - i] = context.Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty > 0).Sum(s => (int?)s.Qty) ?? 0;
            int manualOut = context.Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty < 0).Sum(s => (int?)s.Qty) ?? 0;
            int salesOut = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (int?)s.Qty) ?? 0;
            stockOutValues[6 - i] = Math.Abs(manualOut) + salesOut;
        }
        StockMovementsChart = new ISeries[]
        {
            new LineSeries<int> { Name = "Entrées", Values = stockInValues, Stroke = new SolidColorPaint(SKColors.MediumSeaGreen, 3), Fill = null, GeometrySize = 8 },
            new LineSeries<int> { Name = "Sorties", Values = stockOutValues, Stroke = new SolidColorPaint(SKColors.Crimson, 3), Fill = null, GeometrySize = 8 }
        };

        // 4. Alertes stock
        LowStockItems = context.Products.Count(p => p.Qty <= p.Reorder);
        int normalStock = Math.Max(0, TotalProducts - LowStockItems);
        LowStockChart = new ISeries[]
        {
            new PieSeries<int> { Name = "Critique", Values = new[] { LowStockItems }, Fill = new SolidColorPaint(SKColors.OrangeRed) },
            new PieSeries<int> { Name = "Normal", Values = new[] { normalStock }, Fill = new SolidColorPaint(SKColors.MediumSeaGreen) }
        };
        var criticalItems = context.Products.Where(p => p.Qty <= p.Reorder).ToList();
        CriticalStockList = new ObservableCollection<Product>(criticalItems);
        HasCriticalStock = CriticalStockList.Any();

        // 5. Portefeuilles & CRM
        TotalCustomers = context.CustomerWallets.Count();
        TotalLoyaltyPoints = context.CustomerWallets.Sum(w => (int?)w.LoyaltyPoints) ?? 0;
        TotalWalletsBalance = context.CustomerWallets.Sum(w => (double?)w.Balance) ?? 0;
        var topCustomers = context.CustomerWallets.OrderByDescending(w => w.TotalSpent).Take(5).ToList();
        WalletsYAxes = new[] { new Axis { Labels = topCustomers.Select(w => string.IsNullOrEmpty(w.CustomerName) ? (w.Phone ?? "") : w.CustomerName).ToArray() } };
        WalletsChart = new ISeries[]
        {
            new RowSeries<double> { Name = "Total Achats", Values = topCustomers.Select(w => w.TotalSpent).ToArray(), Fill = new SolidColorPaint(SKColors.Teal) }
        };

        // 6. Articles vendus
        TotalItemsSoldToday = context.SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => (int?)s.Qty) ?? 0;
        TotalItemsSoldAllTime = context.SoldItems.Sum(s => (int?)s.Qty) ?? 0;
        var topSoldToday = context.SoldItems
            .Where(s => s.SoldDate >= today && s.SoldDate < tomorrow)
            .GroupBy(s => s.Description)
            .Select(g => new { Desc = g.Key ?? "", Qty = g.Sum(s => s.Qty) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToList();
        ItemsSoldXAxes = new[] { new Axis { Labels = topSoldToday.Select(x => x.Desc.Length > 10 ? x.Desc.Substring(0, 10) + ".." : x.Desc).ToArray() } };
        ItemsSoldTodayChart = new ISeries[]
        {
            new ColumnSeries<int> { Name = "Vendus", Values = topSoldToday.Select(x => x.Qty).ToArray(), Fill = new SolidColorPaint(SKColors.SteelBlue) }
        };

        // 7. Annulations
        TotalCancelledAmount = context.CancelledOrders.Sum(c => (double?)c.Total) ?? 0;
        var cancelValues = new double[7];
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            cancelValues[6 - i] = context.CancelledOrders.Where(c => c.SoldDate >= d && c.SoldDate < nextD).Sum(c => (double?)c.Total) ?? 0;
        }
        CancelledChart = new ISeries[]
        {
            new LineSeries<double> { Name = "Annulations", Values = cancelValues, Stroke = new SolidColorPaint(SKColors.Crimson, 3), Fill = null }
        };

        // 8. Utilisateurs
        _allUsers = context.Users.ToList();
        TotalUsers = _allUsers.Count;
        AdminCount = _allUsers.Count(u => u.Role == "Admin");
        CashierCount = _allUsers.Count(u => u.Role == "Cashier");
        FilterUsers();

        var usersByRole = _allUsers.GroupBy(u => u.Role ?? "Autre").Select(g => new { Role = g.Key, Count = g.Count() }).ToList();
        UsersChart = usersByRole.Select(ur => (ISeries)new PieSeries<int> { Name = ur.Role, Values = new[] { ur.Count } }).ToArray();

        // 9. Réductions
        TotalDiscountAmount = context.SoldItems.Sum(s => (double?)s.Discount) ?? 0;
        var discountValues = new double[7];
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            discountValues[6 - i] = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (double?)s.Discount) ?? 0;
        }
        DiscountChart = new ISeries[]
        {
            new LineSeries<double> { Name = "Réductions", Values = discountValues, Stroke = new SolidColorPaint(SKColors.Orange, 3), Fill = null }
        };

        // 10. Fournisseurs
        TotalVendors = context.Vendors.Count();
        var productsByVendor = context.Products
            .Where(p => p.VendorId > 0)
            .GroupBy(p => p.VendorName)
            .Select(g => new { Vendor = g.Key ?? "Inconnu", Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();
        VendorsChart = productsByVendor.Select(pv => (ISeries)new PieSeries<int> { Name = pv.Vendor, Values = new[] { pv.Count } }).ToArray();

        // 11. Bénéfice Net
        TotalCOGS = context.SoldItems.Sum(s => (double?)(s.CostPrice * s.Qty)) ?? 0;
        TotalExpensesDashboard = context.Expenses.Sum(e => (double?)e.Amount) ?? 0;
        NetProfit = TotalSalesAllTime - TotalCOGS - TotalExpensesDashboard;
        ProfitXAxes = new[] { new Axis { Labels = new[] { "Revenus", "Coûts", "Dépenses", "Bénéfice Net" } } };
        ProfitChart = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Montants",
                Values = new[] { TotalSalesAllTime, TotalCOGS, TotalExpensesDashboard, NetProfit },
                Fill = new SolidColorPaint(SKColors.DodgerBlue)
            }
        };

        // 12. Modes de paiement
        var payments = context.SoldItems
            .GroupBy(s => s.PaymentMode ?? "Espèces")
            .Select(g => new { Mode = g.Key, Total = g.Sum(s => s.Total) })
            .ToList();
        PaymentModesChart = payments.Select(p => (ISeries)new PieSeries<double> { Name = p.Mode, Values = new[] { p.Total } }).ToArray();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
