using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using LiveCharts;
using LiveCharts.Wpf;

namespace CamaroesPOS.UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private double _todaySales;
        public double TodaySales
        {
            get => _todaySales;
            set { _todaySales = value; OnPropertyChanged(); }
        }

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set { _totalProducts = value; OnPropertyChanged(); }
        }

        private int _totalStockQuantity;
        public int TotalStockQuantity
        {
            get => _totalStockQuantity;
            set { _totalStockQuantity = value; OnPropertyChanged(); }
        }

        private System.Collections.Generic.List<CamaroesPOS.Domain.Entities.User> _allUsers;
        
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterUsers();
            }
        }

        private void FilterUsers()
        {
            if (_allUsers == null) return;
            if (UsersList == null) UsersList = new System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.User>();
            
            UsersList.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var u in _allUsers) UsersList.Add(u);
            }
            else
            {
                var lower = SearchText.ToLower();
                var filtered = _allUsers.Where(u => 
                    (u.Username != null && u.Username.ToLower().Contains(lower)) ||
                    (u.Role != null && u.Role.ToLower().Contains(lower)) ||
                    (u.Name != null && u.Name.ToLower().Contains(lower))
                ).ToList();
                foreach (var u in filtered) UsersList.Add(u);
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.User> _usersList;
        public System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.User> UsersList
        {
            get => _usersList;
            set { _usersList = value; OnPropertyChanged(); }
        }

        public SeriesCollection StockMovementsChart { get; set; }

        private int _lowStockItems;
        public int LowStockItems
        {
            get => _lowStockItems;
            set { _lowStockItems = value; OnPropertyChanged(); }
        }

        private bool _hasCriticalStock;
        public bool HasCriticalStock
        {
            get => _hasCriticalStock;
            set { _hasCriticalStock = value; OnPropertyChanged(); }
        }

        private System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.Product> _criticalStockList;
        public System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.Product> CriticalStockList
        {
            get => _criticalStockList;
            set { _criticalStockList = value; OnPropertyChanged(); }
        }

        private double _totalWalletsBalance;
        public double TotalWalletsBalance
        {
            get => _totalWalletsBalance;
            set { _totalWalletsBalance = value; OnPropertyChanged(); }
        }

        private int _totalCustomers;
        public int TotalCustomers
        {
            get => _totalCustomers;
            set { _totalCustomers = value; OnPropertyChanged(); }
        }

        private int _totalLoyaltyPoints;
        public int TotalLoyaltyPoints
        {
            get => _totalLoyaltyPoints;
            set { _totalLoyaltyPoints = value; OnPropertyChanged(); }
        }

        private double _totalSalesAllTime;
        public double TotalSalesAllTime
        {
            get => _totalSalesAllTime;
            set { _totalSalesAllTime = value; OnPropertyChanged(); }
        }

        private int _totalItemsSoldToday;
        public int TotalItemsSoldToday
        {
            get => _totalItemsSoldToday;
            set { _totalItemsSoldToday = value; OnPropertyChanged(); }
        }

        private int _totalItemsSoldAllTime;
        public int TotalItemsSoldAllTime
        {
            get => _totalItemsSoldAllTime;
            set { _totalItemsSoldAllTime = value; OnPropertyChanged(); }
        }

        private double _totalCancelledAmount;
        public double TotalCancelledAmount
        {
            get => _totalCancelledAmount;
            set { _totalCancelledAmount = value; OnPropertyChanged(); }
        }

        private int _totalUsers;
        public int TotalUsers
        {
            get => _totalUsers;
            set { _totalUsers = value; OnPropertyChanged(); }
        }

        private SeriesCollection _salesChart;
        public SeriesCollection SalesChart
        {
            get => _salesChart;
            set { _salesChart = value; OnPropertyChanged(); }
        }

        public SeriesCollection TodaySalesChart { get; set; }
        public SeriesCollection TotalProductsChart { get; set; }
        public SeriesCollection LowStockChart { get; set; }
        public SeriesCollection WalletsChart { get; set; }
        public SeriesCollection ItemsSoldTodayChart { get; set; }
        public SeriesCollection CancelledChart { get; set; }
        public SeriesCollection UsersChart { get; set; }
        public SeriesCollection DiscountChart { get; set; }
        public SeriesCollection PaymentModesChart { get; set; }

        private double _totalDiscountAmount;
        public double TotalDiscountAmount
        {
            get => _totalDiscountAmount;
            set { _totalDiscountAmount = value; OnPropertyChanged(); }
        }

        public string[] ChartLabels { get; set; }
        public string[] ItemsSoldLabels { get; set; }
        public string[] WalletsLabels { get; set; }
        public string[] ProductsLabels { get; set; }

        private double _totalCogs;
        public double TotalCOGS { get => _totalCogs; set { _totalCogs = value; OnPropertyChanged(); } }

        private double _totalExpenses;
        public double TotalExpensesDashboard { get => _totalExpenses; set { _totalExpenses = value; OnPropertyChanged(); } }

        private double _netProfit;
        public double NetProfit { get => _netProfit; set { _netProfit = value; OnPropertyChanged(); } }

        public SeriesCollection ProfitChart { get; set; }
        public string[] ProfitLabels { get; set; }
        
        public Func<double, string> YFormatter { get; set; }

        public DashboardViewModel()
        {
            YFormatter = value => value.ToString("N0");
            LoadStats();
        }

        public void LoadStats()
        {
            using (var context = new PosDbContext())
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var yesterday = today.AddDays(-1);
                
                // 1 & 2. Ventes du jour et Chiffre d'affaires
                TodaySales = context.SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => (double?)s.Total) ?? 0;
                TotalSalesAllTime = context.SoldItems.Sum(s => (double?)s.Total) ?? 0;
                
                var yesterdaySales = context.SoldItems.Where(s => s.SoldDate >= yesterday && s.SoldDate < today).Sum(s => (double?)s.Total) ?? 0;
                TodaySalesChart = new SeriesCollection
                {
                    new PieSeries { Title = "Aujourd'hui", Values = new ChartValues<double> { TodaySales }, Fill = System.Windows.Media.Brushes.DodgerBlue },
                    new PieSeries { Title = "Hier", Values = new ChartValues<double> { yesterdaySales }, Fill = System.Windows.Media.Brushes.LightGray }
                };

                // Graphique des ventes des 7 derniers jours (Chiffre d'affaires)
                var salesValues = new ChartValues<double>();
                var labels = new string[7];
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    var nextD = d.AddDays(1);
                    var dailyTotal = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (double?)s.Total) ?? 0;
                    salesValues.Add(dailyTotal);
                    labels[6 - i] = d.ToString("dd MMM");
                }
                ChartLabels = labels;
                SalesChart = new SeriesCollection
                {
                    new LineSeries { Title = "Ventes", Values = salesValues, Stroke = System.Windows.Media.Brushes.DodgerBlue, StrokeThickness = 3, PointGeometrySize = 10, Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 30, 136, 229)), DataLabels = true }
                };

                // 3. Total produits
                TotalProducts = context.Products.Count();
                TotalStockQuantity = context.Products.Sum(p => (int?)p.Qty) ?? 0;
                var topProducts = context.Products.OrderByDescending(p => p.Qty).Take(5).ToList();
                ProductsLabels = topProducts.Select(p => p.ProductCode).ToArray();
                TotalProductsChart = new SeriesCollection
                {
                    new ColumnSeries { Title = "Stock", Values = new ChartValues<int>(topProducts.Select(p => p.Qty)), Fill = System.Windows.Media.Brushes.MediumPurple }
                };

                // Graphique des Mouvements de Stock
                var stockInValues = new ChartValues<int>();
                var stockOutValues = new ChartValues<int>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    var nextD = d.AddDays(1);
                    
                    int stockIn = context.Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty > 0).Sum(s => (int?)s.Qty) ?? 0;
                    stockInValues.Add(stockIn);
                    
                    int manualStockOut = context.Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty < 0).Sum(s => (int?)s.Qty) ?? 0;
                    int salesStockOut = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (int?)s.Qty) ?? 0;
                    
                    stockOutValues.Add(Math.Abs(manualStockOut) + salesStockOut);
                }
                StockMovementsChart = new SeriesCollection
                {
                    new LineSeries { Title = "Entrées", Values = stockInValues, Stroke = System.Windows.Media.Brushes.MediumSeaGreen, StrokeThickness = 3, PointGeometrySize = 10, Fill = System.Windows.Media.Brushes.Transparent, DataLabels = true },
                    new LineSeries { Title = "Sorties", Values = stockOutValues, Stroke = System.Windows.Media.Brushes.Crimson, StrokeThickness = 3, PointGeometrySize = 10, Fill = System.Windows.Media.Brushes.Transparent, DataLabels = true }
                };

                // 4. Alertes stock
                LowStockItems = context.Products.Count(p => p.Qty <= p.Reorder);
                int normalStock = TotalProducts - LowStockItems;
                LowStockChart = new SeriesCollection
                {
                    new PieSeries { Title = "Critique", Values = new ChartValues<int> { LowStockItems }, Fill = System.Windows.Media.Brushes.OrangeRed },
                    new PieSeries { Title = "Normal", Values = new ChartValues<int> { normalStock }, Fill = System.Windows.Media.Brushes.MediumSeaGreen }
                };

                var criticalItems = context.Products.Where(p => p.Qty <= p.Reorder).ToList();
                CriticalStockList = new System.Collections.ObjectModel.ObservableCollection<CamaroesPOS.Domain.Entities.Product>(criticalItems);
                HasCriticalStock = CriticalStockList.Any();


                // 5. Portefeuilles et CRM
                TotalCustomers = context.CustomerWallets.Count();
                TotalLoyaltyPoints = context.CustomerWallets.Sum(w => (int?)w.LoyaltyPoints) ?? 0;
                TotalWalletsBalance = context.CustomerWallets.Sum(w => (double?)w.Balance) ?? 0;
                
                var topCustomers = context.CustomerWallets.OrderByDescending(w => w.TotalSpent).Take(5).ToList();
                WalletsLabels = topCustomers.Select(w => string.IsNullOrEmpty(w.CustomerName) ? w.Phone : w.CustomerName).ToArray();
                WalletsChart = new SeriesCollection
                {
                    new RowSeries { Title = "Total Achats", Values = new ChartValues<double>(topCustomers.Select(w => w.TotalSpent)), Fill = System.Windows.Media.Brushes.Teal }
                };

                // 6. Articles vendus
                TotalItemsSoldToday = context.SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => (int?)s.Qty) ?? 0;
                TotalItemsSoldAllTime = context.SoldItems.Sum(s => (int?)s.Qty) ?? 0;
                
                var topSoldToday = context.SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow)
                                    .GroupBy(s => s.Description)
                                    .Select(g => new { Desc = g.Key, Qty = g.Sum(s => s.Qty) })
                                    .OrderByDescending(x => x.Qty).Take(5).ToList();
                ItemsSoldLabels = topSoldToday.Select(x => x.Desc.Length > 10 ? x.Desc.Substring(0, 10) + ".." : x.Desc).ToArray();
                ItemsSoldTodayChart = new SeriesCollection
                {
                    new ColumnSeries { Title = "Vendus", Values = new ChartValues<int>(topSoldToday.Select(x => x.Qty)), Fill = System.Windows.Media.Brushes.SteelBlue }
                };

                // 7. Annulations
                TotalCancelledAmount = context.CancelledOrders.Sum(c => (double?)c.Total) ?? 0;
                var cancelValues = new ChartValues<double>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    var nextD = d.AddDays(1);
                    var dailyCancel = context.CancelledOrders.Where(c => c.SoldDate >= d && c.SoldDate < nextD).Sum(c => (double?)c.Total) ?? 0;
                    cancelValues.Add(dailyCancel);
                }
                CancelledChart = new SeriesCollection
                {
                    new LineSeries { Title = "Annulations", Values = cancelValues, Stroke = System.Windows.Media.Brushes.Crimson, Fill = System.Windows.Media.Brushes.Transparent }
                };

                // 8. Utilisateurs
                var users = context.Users.ToList();
                _allUsers = users;
                TotalUsers = users.Count;
                
                FilterUsers();
                
                var usersByRole = users.GroupBy(u => u.Role).Select(g => new { Role = g.Key, Count = g.Count() }).ToList();
                UsersChart = new SeriesCollection();
                foreach (var ur in usersByRole)
                {
                    UsersChart.Add(new PieSeries { Title = ur.Role, Values = new ChartValues<int> { ur.Count } });
                }

                // 9. Réductions
                TotalDiscountAmount = context.SoldItems.Sum(s => (double?)s.Discount) ?? 0;
                var discountValues = new ChartValues<double>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    var nextD = d.AddDays(1);
                    var dailyDiscount = context.SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => (double?)s.Discount) ?? 0;
                    discountValues.Add(dailyDiscount);
                }
                DiscountChart = new SeriesCollection
                {
                    new LineSeries { Title = "Réductions", Values = discountValues, Stroke = System.Windows.Media.Brushes.Orange, Fill = System.Windows.Media.Brushes.Transparent }
                };
                // 10. Fournisseurs
                TotalVendors = context.Vendors.Count();
                var productsByVendor = context.Products
                    .Where(p => p.VendorId > 0)
                    .GroupBy(p => p.VendorName)
                    .Select(g => new { Vendor = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();
                
                VendorsChart = new SeriesCollection();
                foreach (var pv in productsByVendor)
                {
                    VendorsChart.Add(new PieSeries { Title = pv.Vendor ?? "Inconnu", Values = new ChartValues<int> { pv.Count } });
                }

                // 11. Calcul du Bénéfice Net (Revenus - Coûts d'achat - Dépenses)
                TotalCOGS = context.SoldItems.Sum(s => (double?)(s.CostPrice * s.Qty)) ?? 0;
                TotalExpensesDashboard = context.Expenses.Sum(e => (double?)e.Amount) ?? 0;
                NetProfit = TotalSalesAllTime - TotalCOGS - TotalExpensesDashboard;

                ProfitLabels = new[] { "Revenus", "Coûts", "Dépenses", "Bénéfice Net" };
                ProfitChart = new SeriesCollection
                {
                    new ColumnSeries 
                    { 
                        Title = "Montants", 
                        Values = new ChartValues<double> { TotalSalesAllTime, TotalCOGS, TotalExpensesDashboard, NetProfit },
                        Fill = System.Windows.Media.Brushes.DodgerBlue
                    }
                };

                // 12. Statistiques des modes de paiement
                var payments = context.SoldItems
                    .GroupBy(s => s.PaymentMode ?? "Espèces")
                    .Select(g => new { Mode = g.Key, Total = g.Sum(s => s.Total) })
                    .ToList();

                PaymentModesChart = new SeriesCollection();
                foreach (var p in payments)
                {
                    PaymentModesChart.Add(new PieSeries 
                    { 
                        Title = p.Mode, 
                        Values = new ChartValues<double> { p.Total }, 
                        DataLabels = true 
                    });
                }
            }
        }

        private int _totalVendors;
        public int TotalVendors
        {
            get => _totalVendors;
            set { _totalVendors = value; OnPropertyChanged(); }
        }

        public SeriesCollection VendorsChart { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
