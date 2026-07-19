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
            Wallets.Add(new CustomerWallet { ClientName = "Jean Dupont", PhoneNumber = "0601020304", Balance = 150.00m, WalletHash = "HASH123" });
            Wallets.Add(new CustomerWallet { ClientName = "Marie Curie", PhoneNumber = "0611223344", Balance = 50.00m, WalletHash = "HASH456" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
