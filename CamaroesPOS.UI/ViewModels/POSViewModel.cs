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

        private decimal _totalAmount;
        public decimal TotalAmount
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
            Products.Add(new Product { pcode = "P001", pdesc = "Laptop Gamer", price = 1500.00m });
            Products.Add(new Product { pcode = "P002", pdesc = "Souris Sans Fil", price = 25.50m });
            Products.Add(new Product { pcode = "P003", pdesc = "Clavier Mécanique", price = 75.00m });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
