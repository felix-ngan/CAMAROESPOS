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
            InventoryList.Add(new Product { pcode = "P001", pdesc = "Laptop Gamer", price = 1500.00m, qty = 10 });
            InventoryList.Add(new Product { pcode = "P002", pdesc = "Souris Sans Fil", price = 25.50m, qty = 45 });
            InventoryList.Add(new Product { pcode = "P003", pdesc = "Clavier Mécanique", price = 75.00m, qty = 5 }); // Faible stock
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
