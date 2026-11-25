using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CustomerWallet> Wallets { get; set; }

        private CustomerWallet _selectedWallet;
        public CustomerWallet SelectedWallet
        {
            get => _selectedWallet;
            set { _selectedWallet = value; OnPropertyChanged(); }
        }

        private bool _isRecharging;
        public bool IsRecharging
        {
            get => _isRecharging;
            set { _isRecharging = value; OnPropertyChanged(); }
        }

        private bool _isNewWallet;
        public bool IsNewWallet
        {
            get => _isNewWallet;
            set { _isNewWallet = value; OnPropertyChanged(); }
        }

        private decimal _rechargeAmount;
        public decimal RechargeAmount
        {
            get => _rechargeAmount;
            set { _rechargeAmount = value; OnPropertyChanged(); }
        }

        public ICommand CreateWalletCommand { get; }
        public ICommand RechargeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteWalletCommand { get; }

        public WalletViewModel()
        {
            Wallets = new ObservableCollection<CustomerWallet>();
            CreateWalletCommand = new RelayCommand<object>(OpenCreateForm);
            RechargeCommand = new RelayCommand<CustomerWallet>(OpenRechargeForm);
            SaveCommand = new RelayCommand<object>(SaveWalletAction);
            CancelCommand = new RelayCommand<object>(CancelAction);
            DeleteWalletCommand = new RelayCommand<object>(DeleteWallet);

            LoadDataFromDatabase();
        }

        private void OpenCreateForm(object obj)
        {
            SelectedWallet = new CustomerWallet();
            RechargeAmount = 0;
            IsNewWallet = true;
            IsRecharging = true;
        }

        private void OpenRechargeForm(CustomerWallet wallet)
        {
            if (wallet == null) return;
            SelectedWallet = wallet;
            RechargeAmount = 0;
            IsNewWallet = false;
            IsRecharging = true;
        }

        private void SaveWalletAction(object obj)
        {
            if (string.IsNullOrWhiteSpace(SelectedWallet?.Phone))
            {
                MessageBox.Show("Le numéro de téléphone est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (RechargeAmount < 0)
            {
                MessageBox.Show("Le montant ne peut pas être négatif.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new PosDbContext())
            {
                if (IsNewWallet)
                {
                    if (context.CustomerWallets.Any(w => w.Phone == SelectedWallet.Phone))
                    {
                        MessageBox.Show("Ce portefeuille existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    SelectedWallet.Balance = RechargeAmount;
                    context.CustomerWallets.Add(SelectedWallet);
                    
                    if (RechargeAmount > 0)
                    {
                        context.WalletTransactions.Add(new WalletTransaction
                        {
                            Phone = SelectedWallet.Phone,
                            TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                            Type = "Credit",
                            Amount = RechargeAmount,
                            CreatedAt = DateTime.Now
                        });
                    }

                    context.SaveChanges();
                    MessageBox.Show("Portefeuille créé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (RechargeAmount <= 0)
                    {
                        MessageBox.Show("Le montant de la recharge doit être supérieur à zéro.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var dbWallet = context.CustomerWallets.FirstOrDefault(w => w.Phone == SelectedWallet.Phone);
                    if (dbWallet != null)
                    {
                        dbWallet.Balance += RechargeAmount;
                        dbWallet.CustomerName = SelectedWallet.CustomerName;
                        dbWallet.Email = SelectedWallet.Email;
                        
                        context.WalletTransactions.Add(new WalletTransaction
                        {
                            Phone = dbWallet.Phone,
                            TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                            Type = "Credit",
                            Amount = RechargeAmount,
                            CreatedAt = DateTime.Now
                        });
                        
                        context.SaveChanges();
                        MessageBox.Show($"Recharge de {RechargeAmount:N0} FCFA effectuée avec succès sur le compte {dbWallet.Phone}.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }

            IsRecharging = false;
            LoadDataFromDatabase();
        }

        private void CancelAction(object obj)
        {
            IsRecharging = false;
            SelectedWallet = null;
        }

        private void DeleteWallet(object obj)
        {
            if (obj is CustomerWallet wallet)
            {
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer définitivement le portefeuille {wallet.Phone} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new PosDbContext())
                    {
                        var walletToDelete = context.CustomerWallets.FirstOrDefault(w => w.Phone == wallet.Phone);
                        if (walletToDelete != null)
                        {
                            context.CustomerWallets.Remove(walletToDelete);
                            
                            var transactions = context.WalletTransactions.Where(t => t.Phone == wallet.Phone).ToList();
                            context.WalletTransactions.RemoveRange(transactions);
                            
                            context.SaveChanges();
                            LoadDataFromDatabase();
                            MessageBox.Show("Portefeuille supprimé avec succès.", "Suppression", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new PosDbContext())
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
