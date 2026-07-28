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
        public ICommand NumpadCommand { get; }
        public ICommand ClearNumpadCommand { get; }
        public ICommand CancelCheckoutCommand { get; }
        public ICommand ConfirmCashPaymentCommand { get; }
        public ICommand ExactAmountCommand { get; }
        public ICommand QuickCashCommand { get; }

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

        // Tactile Calculator variables
        private bool _isCheckoutDialogOpen;
        public bool IsCheckoutDialogOpen
        {
            get => _isCheckoutDialogOpen;
            set { _isCheckoutDialogOpen = value; OnPropertyChanged(); }
        }

        private string _receivedAmountString = "";
        public string ReceivedAmountString
        {
            get => _receivedAmountString;
            set 
            { 
                _receivedAmountString = value; 
                OnPropertyChanged(); 
                UpdateChangeAmount();
            }
        }

        private double _receivedAmount;
        public double ReceivedAmount
        {
            get => _receivedAmount;
            set { _receivedAmount = value; OnPropertyChanged(); }
        }

        private double _changeAmount;
        public double ChangeAmount
        {
            get => _changeAmount;
            set { _changeAmount = value; OnPropertyChanged(); }
        }

        public POSViewModel()
        {
            Products = new ObservableCollection<Product>();
            Cart = new ObservableCollection<CartItem>();
            
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            CheckoutCommand = new RelayCommand<object>(Checkout, CanCheckout);

            NumpadCommand = new RelayCommand<string>(NumpadPress);
            ClearNumpadCommand = new RelayCommand<object>(ClearNumpad);
            CancelCheckoutCommand = new RelayCommand<object>(CancelCheckout);
            ConfirmCashPaymentCommand = new RelayCommand<object>(ConfirmCashPayment, CanConfirmCashPayment);
            ExactAmountCommand = new RelayCommand<object>(ExactAmount);
            QuickCashCommand = new RelayCommand<string>(QuickCash);

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
                Cart.RemoveAt(index);
                Cart.Insert(index, existingItem);
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
            UpdateChangeAmount(); // To update the popup if it's open (though you can't add to cart while it's open)
        }

        private bool CanCheckout(object obj)
        {
            return Cart.Count > 0;
        }

        private void Checkout(object obj)
        {
            if (Cart.Count == 0) return;

            if (IsWalletPayment)
            {
                using (var context = new PosDbContext())
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

                    wallet.Balance -= TotalAmount;
                    
                    context.WalletTransactions.Add(new WalletTransaction
                    {
                        Phone = wallet.Phone,
                        TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Type = "Debit",
                        Amount = TotalAmount,
                        CreatedAt = DateTime.Now
                    });
                    context.SaveChanges();
                }

                SaveTransaction("Portefeuille");
                MessageBox.Show("Encaissement réussi par Portefeuille !", "Caisse", MessageBoxButton.OK, MessageBoxImage.Information);
                
                Cart.Clear();
                UpdateTotalAmount();
                CustomerPhone = string.Empty;
                LoadDataFromDatabase();
            }
            else
            {
                // Cash Payment: Open Numpad Popup
                ReceivedAmountString = "";
                IsCheckoutDialogOpen = true;
            }
        }

        private void SaveTransaction(string paymentMode)
        {
            using (var context = new PosDbContext())
            {
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
            }
        }

        private void NumpadPress(string digit)
        {
            if (ReceivedAmountString == "0" || string.IsNullOrEmpty(ReceivedAmountString)) 
                ReceivedAmountString = digit;
            else 
                ReceivedAmountString += digit;
        }

        private void ExactAmount(object obj)
        {
            ReceivedAmountString = TotalAmount.ToString();
        }

        private void QuickCash(string amount)
        {
            ReceivedAmountString = amount;
        }

        private void ClearNumpad(object obj)
        {
            ReceivedAmountString = "";
        }

        private void UpdateChangeAmount()
        {
            if (double.TryParse(ReceivedAmountString, out double parsed))
            {
                ReceivedAmount = parsed;
                ChangeAmount = ReceivedAmount - TotalAmount;
            }
            else
            {
                ReceivedAmount = 0;
                ChangeAmount = -TotalAmount;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void CancelCheckout(object obj)
        {
            IsCheckoutDialogOpen = false;
        }

        private bool CanConfirmCashPayment(object obj)
        {
            return ReceivedAmount >= TotalAmount && TotalAmount > 0;
        }

        private void ConfirmCashPayment(object obj)
        {
            SaveTransaction("Espèces");
            
            MessageBox.Show($"Encaissement réussi !\nRendu Monnaie : {ChangeAmount:N0} FCFA", "Caisse", MessageBoxButton.OK, MessageBoxImage.Information);
            
            IsCheckoutDialogOpen = false;
            Cart.Clear();
            UpdateTotalAmount();
            ReceivedAmountString = "";
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
