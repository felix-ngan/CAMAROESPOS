using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class POSViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<CartItem> Cart { get; set; }

        private double _totalAmount;
        public double TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public POSViewModel()
        {
            Products = new ObservableCollection<Product>();
            Cart = new ObservableCollection<CartItem>();
            
            // Simulation de chargement de produits pour tester le design
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            Products.Add(new Product { ProductCode = "P001", Description = "Laptop Gamer", Price = 1500.00 });
            Products.Add(new Product { ProductCode = "P002", Description = "Souris Sans Fil", Price = 25.50 });
            Products.Add(new Product { ProductCode = "P003", Description = "Clavier Mécanique", Price = 75.00 });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
