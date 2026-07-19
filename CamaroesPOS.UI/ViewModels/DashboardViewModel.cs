using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;

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

        public DashboardViewModel()
        {
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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
