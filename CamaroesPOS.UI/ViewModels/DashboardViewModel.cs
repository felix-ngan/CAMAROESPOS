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

        private string[] _chartLabels;
        public string[] ChartLabels
        {
            get => _chartLabels;
            set { _chartLabels = value; OnPropertyChanged(); }
        }

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
                
                // Ventes du jour (somme des totaux des items vendus aujourd'hui)
                TodaySales = context.SoldItems
                    .Where(s => s.SoldDate >= today && s.SoldDate < tomorrow)
                    .Sum(s => (double?)s.Total) ?? 0;

                // Total produits
                TotalProducts = context.Products.Count();

                // Alertes stock (produits dont la quantité est <= seuil critique de 5)
                LowStockItems = context.Products.Count(p => p.Qty <= 5);

                // Solde total des portefeuilles clients
                TotalWalletsBalance = context.CustomerWallets.Sum(w => (double?)w.Balance) ?? 0;

                // Nouvelles Stats
                TotalSalesAllTime = context.SoldItems.Sum(s => (double?)s.Total) ?? 0;
                
                TotalItemsSoldToday = context.SoldItems
                    .Where(s => s.SoldDate >= today && s.SoldDate < tomorrow)
                    .Sum(s => (int?)s.Qty) ?? 0;

                TotalItemsSoldAllTime = context.SoldItems.Sum(s => (int?)s.Qty) ?? 0;

                TotalCancelledAmount = context.CancelledOrders.Sum(c => (double?)c.Total) ?? 0;

                TotalUsers = context.Users.Count();

                // Graphique des ventes des 7 derniers jours
                var salesValues = new ChartValues<double>();
                var labels = new string[7];
                
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    var nextD = d.AddDays(1);
                    var dailyTotal = context.SoldItems
                        .Where(s => s.SoldDate >= d && s.SoldDate < nextD)
                        .Sum(s => (double?)s.Total) ?? 0;
                    
                    salesValues.Add(dailyTotal);
                    labels[6 - i] = d.ToString("dd MMM");
                }

                ChartLabels = labels;
                SalesChart = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Ventes (FCFA)",
                        Values = salesValues,
                        Fill = System.Windows.Media.Brushes.DodgerBlue,
                        DataLabels = true
                    }
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
