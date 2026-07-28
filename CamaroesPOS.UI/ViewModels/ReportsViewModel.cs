using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Data;

namespace CamaroesPOS.UI.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SoldItem> Transactions { get; set; }
        public ObservableCollection<Product> ProductsList { get; set; }
        public ObservableCollection<Stock> StockHistoryList { get; set; }
        public ObservableCollection<CancelledOrder> CancelledOrdersList { get; set; }

        private DateTime? _startDate;
        public DateTime? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

        private DateTime? _endDate;
        public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        private double _totalSalesAmount;
        public double TotalSalesAmount { get => _totalSalesAmount; set { _totalSalesAmount = value; OnPropertyChanged(); } }

        private int _totalItemsSold;
        public int TotalItemsSold { get => _totalItemsSold; set { _totalItemsSold = value; OnPropertyChanged(); } }

        public ICommand FilterCommand { get; }
        public ICommand ResetFilterCommand { get; }

        public ReportsViewModel()
        {
            Transactions = new ObservableCollection<SoldItem>();
            ProductsList = new ObservableCollection<Product>();
            StockHistoryList = new ObservableCollection<Stock>();
            CancelledOrdersList = new ObservableCollection<CancelledOrder>();

            FilterCommand = new RelayCommand<object>(ApplyFilter);
            ResetFilterCommand = new RelayCommand<object>(ResetFilter);

            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now.Date;

            LoadTransactions();
        }

        public void LoadTransactions()
        {
            ApplyFilter(null);
        }

        private void ApplyFilter(object obj)
        {
            using (var context = new PosDbContext())
            {
                // 1. Ventes (Transactions)
                var querySales = context.SoldItems.AsQueryable();
                if (StartDate.HasValue) querySales = querySales.Where(t => t.SoldDate >= StartDate.Value.Date);
                if (EndDate.HasValue)
                {
                    var end = EndDate.Value.Date.AddDays(1);
                    querySales = querySales.Where(t => t.SoldDate < end);
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    querySales = querySales.Where(t => t.TransNo.ToLower().Contains(search) || 
                                             t.ProductCode.ToLower().Contains(search) || 
                                             t.Description.ToLower().Contains(search) ||
                                             t.Cashier.ToLower().Contains(search));
                }

                var salesResults = querySales.OrderByDescending(t => t.SoldDate).ToList();
                Transactions.Clear();
                foreach (var item in salesResults) Transactions.Add(item);

                TotalSalesAmount = salesResults.Sum(t => (double?)t.Total) ?? 0;
                TotalItemsSold = salesResults.Sum(t => (int?)t.Qty) ?? 0;

                // 2. Produits (Inventory Status)
                var queryProducts = context.Products.AsQueryable();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    queryProducts = queryProducts.Where(p => p.ProductCode.ToLower().Contains(search) || p.Description.ToLower().Contains(search));
                }
                var productResults = queryProducts.OrderBy(p => p.Description).ToList();
                ProductsList.Clear();
                foreach (var item in productResults) ProductsList.Add(item);

                // 3. Historique Stock
                var queryStock = context.Stocks.AsQueryable();
                if (StartDate.HasValue) queryStock = queryStock.Where(s => s.StockInDate >= StartDate.Value.Date);
                if (EndDate.HasValue)
                {
                    var end = EndDate.Value.Date.AddDays(1);
                    queryStock = queryStock.Where(s => s.StockInDate < end);
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    queryStock = queryStock.Where(s => s.ProductCode.ToLower().Contains(search) || s.Description.ToLower().Contains(search) || s.RefNo.ToLower().Contains(search));
                }
                var stockResults = queryStock.OrderByDescending(s => s.StockInDate).ToList();
                StockHistoryList.Clear();
                foreach (var item in stockResults) StockHistoryList.Add(item);

                // 4. Annulations
                var queryCancelled = context.CancelledOrders.AsQueryable();
                if (StartDate.HasValue) queryCancelled = queryCancelled.Where(c => c.SoldDate >= StartDate.Value.Date);
                if (EndDate.HasValue)
                {
                    var end = EndDate.Value.Date.AddDays(1);
                    queryCancelled = queryCancelled.Where(c => c.SoldDate < end);
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    queryCancelled = queryCancelled.Where(c => c.TransNo.ToLower().Contains(search) || c.ProductCode.ToLower().Contains(search));
                }
                var cancelledResults = queryCancelled.OrderByDescending(c => c.SoldDate).ToList();
                CancelledOrdersList.Clear();
                foreach (var item in cancelledResults) CancelledOrdersList.Add(item);
            }
        }

        private void ResetFilter(object obj)
        {
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now.Date;
            SearchText = string.Empty;
            ApplyFilter(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
