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

        private int _lowStockItems;
        public int LowStockItems
        {
            get => _lowStockItems;
            set { _lowStockItems = value; OnPropertyChanged(); }
        }

        private double _totalWalletsBalance;
        public double TotalWalletsBalance
        {
            get => _totalWalletsBalance;
            set { _totalWalletsBalance = value; OnPropertyChanged(); }
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

        public string[] ChartLabels { get; set; }
        public string[] ItemsSoldLabels { get; set; }
        public string[] WalletsLabels { get; set; }
        public string[] ProductsLabels { get; set; }
        
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
                var topProducts = context.Products.OrderByDescending(p => p.Qty).Take(5).ToList();
                ProductsLabels = topProducts.Select(p => p.ProductCode).ToArray();
                TotalProductsChart = new SeriesCollection
                {
                    new ColumnSeries { Title = "Stock", Values = new ChartValues<int>(topProducts.Select(p => p.Qty)), Fill = System.Windows.Media.Brushes.MediumPurple }
                };

                // 4. Alertes stock
                LowStockItems = context.Products.Count(p => p.Qty <= p.Reorder);
                int normalStock = TotalProducts - LowStockItems;
                LowStockChart = new SeriesCollection
                {
                    new PieSeries { Title = "Faible", Values = new ChartValues<int> { LowStockItems }, Fill = System.Windows.Media.Brushes.OrangeRed },
                    new PieSeries { Title = "Normal", Values = new ChartValues<int> { normalStock }, Fill = System.Windows.Media.Brushes.MediumSeaGreen }
                };

                // 5. Portefeuilles
                TotalWalletsBalance = context.CustomerWallets.Sum(w => (double?)w.Balance) ?? 0;
                var topWallets = context.CustomerWallets.OrderByDescending(w => w.Balance).Take(5).ToList();
                WalletsLabels = topWallets.Select(w => w.Phone).ToArray();
                WalletsChart = new SeriesCollection
                {
                    new RowSeries { Title = "Solde", Values = new ChartValues<double>(topWallets.Select(w => w.Balance)), Fill = System.Windows.Media.Brushes.Teal }
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
                    var dailyCancel = context.CancelledOrders.Where(c => c.CancelledDate >= d && c.CancelledDate < nextD).Sum(c => (double?)c.Total) ?? 0;
                    cancelValues.Add(dailyCancel);
                }
                CancelledChart = new SeriesCollection
                {
                    new LineSeries { Title = "Annulations", Values = cancelValues, Stroke = System.Windows.Media.Brushes.Crimson, Fill = System.Windows.Media.Brushes.Transparent }
                };

                // 8. Utilisateurs
                TotalUsers = context.Users.Count();
                var usersByRole = context.Users.GroupBy(u => u.Role).Select(g => new { Role = g.Key, Count = g.Count() }).ToList();
                UsersChart = new SeriesCollection();
                foreach (var ur in usersByRole)
                {
                    UsersChart.Add(new PieSeries { Title = ur.Role, Values = new ChartValues<int> { ur.Count } });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
