using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> InventoryList { get; set; }

        public InventoryViewModel()
        {
            InventoryList = new ObservableCollection<Product>();
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            InventoryList.Add(new Product { ProductCode = "P001", Description = "Laptop Gamer", Price = 1500.00, Qty = 10 });
            InventoryList.Add(new Product { ProductCode = "P002", Description = "Souris Sans Fil", Price = 25.50, Qty = 45 });
            InventoryList.Add(new Product { ProductCode = "P003", Description = "Clavier Mécanique", Price = 75.00, Qty = 5 }); // Faible stock
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
