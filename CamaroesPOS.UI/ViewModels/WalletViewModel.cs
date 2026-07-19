using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CustomerWallet> Wallets { get; set; }

        public WalletViewModel()
        {
            Wallets = new ObservableCollection<CustomerWallet>();
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new CamaroesPOS.Data.PosDbContext())
            {
                var wallets = context.CustomerWallets.ToList();
                Wallets.Clear();
                foreach (var w in wallets)
                {
                    Wallets.Add(w);
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
