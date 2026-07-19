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
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            Wallets.Add(new CustomerWallet { Phone = "0601020304", Balance = 150.00 });
            Wallets.Add(new CustomerWallet { Phone = "0611223344", Balance = 50.00 });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
