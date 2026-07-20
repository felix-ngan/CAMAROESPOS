using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Data;
using System.Windows;

namespace CamaroesPOS.UI.ViewModels
{
    public class POSViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<CartItem> Cart { get; set; }

        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        private double _totalAmount;
        public double TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        private int _paymentMethodIndex;
        public int PaymentMethodIndex
        {
            get => _paymentMethodIndex;
            set 
            { 
                _paymentMethodIndex = value; 
                OnPropertyChanged(); 
                IsWalletPayment = (_paymentMethodIndex == 1); 
            }
        }

        private bool _isWalletPayment;
        public bool IsWalletPayment
        {
            get => _isWalletPayment;
            set { _isWalletPayment = value; OnPropertyChanged(); }
        }

        private string _customerPhone;
        public string CustomerPhone
        {
            get => _customerPhone;
            set { _customerPhone = value; OnPropertyChanged(); }
        }

        public POSViewModel()
        {
            Products = new ObservableCollection<Product>();
            Cart = new ObservableCollection<CartItem>();
            
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            CheckoutCommand = new RelayCommand<object>(Checkout, CanCheckout);

            LoadDataFromDatabase();
        }

        private void AddToCart(Product product)
        {
            if (product == null) return;

            var existingItem = Cart.FirstOrDefault(c => c.ProductCode == product.ProductCode);
            if (existingItem != null)
            {
                existingItem.Qty++;
                existingItem.Total = existingItem.Qty * existingItem.Price;
                
                int index = Cart.IndexOf(existingItem);
                Cart[index] = existingItem;
            }
            else
            {
                Cart.Add(new CartItem
                {
                    ProductCode = product.ProductCode,
                    ProductDescription = product.Description,
                    Price = product.Price,
                    Qty = 1,
                    Total = product.Price,
                    Date = DateTime.Now
                });
            }

            UpdateTotalAmount();
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateTotalAmount()
        {
            TotalAmount = Cart.Sum(c => c.Total);
        }

        private bool CanCheckout(object obj)
        {
            return Cart.Count > 0;
        }

        private void Checkout(object obj)
        {
            if (Cart.Count == 0) return;

            using (var context = new PosDbContext())
            {
                // Vérification du portefeuille si c'est le mode de paiement
                if (IsWalletPayment)
                {
                    if (string.IsNullOrWhiteSpace(CustomerPhone))
                    {
                        MessageBox.Show("Veuillez entrer le numéro de téléphone du client pour payer par portefeuille.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var wallet = context.CustomerWallets.FirstOrDefault(w => w.Phone == CustomerPhone);
                    if (wallet == null)
                    {
                        MessageBox.Show("Ce portefeuille client n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (wallet.Balance < TotalAmount)
                    {
                        MessageBox.Show($"Solde insuffisant dans le portefeuille de ce client (Solde: {wallet.Balance} FCFA).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Déduire le solde
                    wallet.Balance -= TotalAmount;
                    
                    // Ajouter la transaction Wallet
                    context.WalletTransactions.Add(new WalletTransaction
                    {
                        Phone = wallet.Phone,
                        TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Type = "Debit",
                        Amount = TotalAmount,
                        CreatedAt = DateTime.Now
                    });
                }

                string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");

                foreach (var item in Cart)
                {
                    var soldItem = new SoldItem
                    {
                        TransNo = transNo,
                        ProductCode = item.ProductCode,
                        Description = item.ProductDescription,
                        Price = item.Price,
                        Qty = item.Qty,
                        Total = item.Total,
                        SoldDate = DateTime.Now,
                        Cashier = "Admin"
                    };
                    context.SoldItems.Add(soldItem);

                    var product = context.Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
                    if (product != null)
                    {
                        product.Qty -= item.Qty;
                        if (product.Qty < 0) product.Qty = 0;
                    }
                }

                context.SaveChanges(); 
                
                string mode = IsWalletPayment ? "Portefeuille" : "Espèces";
                MessageBox.Show($"Encaissement réussi par {mode} !\nN° Transaction : {transNo}", "Caisse", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Cart.Clear();
            UpdateTotalAmount();
            CustomerPhone = string.Empty;
            
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new PosDbContext())
            {
                var products = context.Products.ToList();
                Products.Clear();
                foreach (var p in products)
                {
                    Products.Add(p);
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
