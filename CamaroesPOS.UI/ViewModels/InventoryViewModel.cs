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
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new CamaroesPOS.Data.PosDbContext())
            {
                var products = context.Products.ToList();
                InventoryList.Clear();
                foreach (var p in products)
                {
                    InventoryList.Add(p);
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
