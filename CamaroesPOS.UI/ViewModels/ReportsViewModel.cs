using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Data;
using System.Collections.Generic;

namespace CamaroesPOS.UI.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SoldItem> Transactions { get; set; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        private double _totalSalesAmount;
        public double TotalSalesAmount
        {
            get => _totalSalesAmount;
            set { _totalSalesAmount = value; OnPropertyChanged(); }
        }

        private int _totalItemsSold;
        public int TotalItemsSold
        {
            get => _totalItemsSold;
            set { _totalItemsSold = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }
        public ICommand ResetFilterCommand { get; }

        public ReportsViewModel()
        {
            Transactions = new ObservableCollection<SoldItem>();
            FilterCommand = new RelayCommand<object>(ApplyFilter);
            ResetFilterCommand = new RelayCommand<object>(ResetFilter);

            // Filtre par défaut sur le mois en cours
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
                var query = context.SoldItems.AsQueryable();

                if (StartDate.HasValue)
                {
                    query = query.Where(t => t.SoldDate >= StartDate.Value.Date);
                }

                if (EndDate.HasValue)
                {
                    // Jusqu'à la fin de la journée
                    var end = EndDate.Value.Date.AddDays(1);
                    query = query.Where(t => t.SoldDate < end);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    query = query.Where(t => t.TransNo.ToLower().Contains(search) || 
                                             t.ProductCode.ToLower().Contains(search) || 
                                             t.Description.ToLower().Contains(search) ||
                                             t.Cashier.ToLower().Contains(search));
                }

                // Trier par date décroissante
                var results = query.OrderByDescending(t => t.SoldDate).ToList();

                Transactions.Clear();
                foreach (var item in results)
                {
                    Transactions.Add(item);
                }

                TotalSalesAmount = results.Sum(t => (double?)t.Total) ?? 0;
                TotalItemsSold = results.Sum(t => (int?)t.Qty) ?? 0;
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
