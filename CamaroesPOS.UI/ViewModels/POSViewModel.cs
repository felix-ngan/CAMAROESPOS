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
        public ICommand ClearCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand OpenCatalogCommand { get; }
        public ICommand CloseCatalogCommand { get; }
        public ICommand SearchBarcodeCommand { get; }
        public ICommand ConfirmQuantityCommand { get; }
        public ICommand CancelQuantityCommand { get; }
        public ICommand IncreaseQtyCommand { get; }
        public ICommand DecreaseQtyCommand { get; }

        private double _subTotalAmount;
        public double SubTotalAmount
        {
            get => _subTotalAmount;
            set { _subTotalAmount = value; OnPropertyChanged(); }
        }

        private double _discountPercentage;
        public double DiscountPercentage
        {
            get => _discountPercentage;
            set 
            { 
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                _discountPercentage = value; 
                OnPropertyChanged(); 
                UpdateTotalAmount(); 
            }
        }

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

        private string _barcodeSearch;
        public string BarcodeSearch
        {
            get => _barcodeSearch;
            set { _barcodeSearch = value; OnPropertyChanged(); }
        }

        private int _selectedQuantityToAdd = 1;
        public int SelectedQuantityToAdd
        {
            get => _selectedQuantityToAdd;
            set { _selectedQuantityToAdd = value; OnPropertyChanged(); }
        }

        private Product _selectedProductToAdd;

        // Tactile Calculator variables
        private bool _isCheckoutDialogOpen;
        public bool IsCheckoutDialogOpen
        {
            get => _isCheckoutDialogOpen;
            set 
            { 
                _isCheckoutDialogOpen = value; 
                OnPropertyChanged(); 
                UpdateAnyDialogOpen();
            }
        }

        private bool _isCatalogDialogOpen;
        public bool IsCatalogDialogOpen
        {
            get => _isCatalogDialogOpen;
            set
            {
                _isCatalogDialogOpen = value;
                OnPropertyChanged();
                UpdateAnyDialogOpen();
            }
        }

        private bool _isQuantityDialogOpen;
        public bool IsQuantityDialogOpen
        {
            get => _isQuantityDialogOpen;
            set
            {
                _isQuantityDialogOpen = value;
                OnPropertyChanged();
                UpdateAnyDialogOpen();
            }
        }

        private bool _isAnyDialogOpen;
        public bool IsAnyDialogOpen
        {
            get => _isAnyDialogOpen;
            set { _isAnyDialogOpen = value; OnPropertyChanged(); }
        }

        private void UpdateAnyDialogOpen()
        {
            IsAnyDialogOpen = IsCheckoutDialogOpen || IsCatalogDialogOpen || IsQuantityDialogOpen;
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
            ClearCartCommand = new RelayCommand<object>(ClearCart, CanClearCart);
            RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
            OpenCatalogCommand = new RelayCommand<object>(o => IsCatalogDialogOpen = true);
            CloseCatalogCommand = new RelayCommand<object>(o => IsCatalogDialogOpen = false);
            SearchBarcodeCommand = new RelayCommand<object>(SearchBarcode);
            ConfirmQuantityCommand = new RelayCommand<object>(ConfirmQuantity);
            CancelQuantityCommand = new RelayCommand<object>(o => IsQuantityDialogOpen = false);
            IncreaseQtyCommand = new RelayCommand<object>(o => SelectedQuantityToAdd++);
            DecreaseQtyCommand = new RelayCommand<object>(o => { if (SelectedQuantityToAdd > 1) SelectedQuantityToAdd--; });

            LoadDataFromDatabase();
        }

        private void AddToCart(Product product)
        {
            if (product == null) return;
            
            // Ouvrir le dialogue de quantité
            _selectedProductToAdd = product;
            SelectedQuantityToAdd = 1;
            IsQuantityDialogOpen = true;
        }

        private void ConfirmQuantity(object obj)
        {
            if (_selectedProductToAdd == null || SelectedQuantityToAdd <= 0) return;

            AddProductToCartLogic(_selectedProductToAdd, SelectedQuantityToAdd);
            IsQuantityDialogOpen = false;
        }

        private void SearchBarcode(object obj)
        {
            if (string.IsNullOrWhiteSpace(BarcodeSearch)) return;

            using (var context = new PosDbContext())
            {
                var product = context.Products.FirstOrDefault(p => p.Barcode == BarcodeSearch || p.ProductCode == BarcodeSearch);
                if (product != null)
                {
                    // Scan direct : on ajoute 1 sans demander la quantité
                    AddProductToCartLogic(product, 1);
                }
                else
                {
                    MessageBox.Show("Produit introuvable pour ce code-barres.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            BarcodeSearch = ""; // Clear for next scan
        }

        private void AddProductToCartLogic(Product product, int qty)
        {
            var existingItem = Cart.FirstOrDefault(c => c.ProductCode == product.ProductCode);
            if (existingItem != null)
            {
                existingItem.Qty += qty;
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
                    CostPrice = product.CostPrice,
                    Price = product.Price,
                    Qty = qty,
                    Total = product.Price * qty,
                    Date = DateTime.Now
                });
            }

            UpdateTotalAmount();
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateTotalAmount()
        {
            SubTotalAmount = Cart.Sum(c => c.Total);
            TotalAmount = SubTotalAmount - (SubTotalAmount * (DiscountPercentage / 100.0));
            UpdateChangeAmount(); // To update the popup if it's open (though you can't add to cart while it's open)
        }

        private bool CanClearCart(object obj)
        {
            return Cart.Count > 0;
        }

        private void ClearCart(object obj)
        {
            Cart.Clear();
            DiscountPercentage = 0;
            UpdateTotalAmount();
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemoveFromCart(CartItem item)
        {
            if (item != null)
            {
                Cart.Remove(item);
                UpdateTotalAmount();
                CommandManager.InvalidateRequerySuggested();
            }
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

                string transNo = SaveTransaction("Portefeuille");
                
                // Show receipt for Wallet
                var receiptWin = new CamaroesPOS.UI.Views.ReceiptWindow(transNo, new ObservableCollection<CartItem>(Cart), TotalAmount, TotalAmount, 0, "Portefeuille (Wallet)");
                receiptWin.ShowDialog();
                
                Cart.Clear();
                DiscountPercentage = 0;
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

        private string SaveTransaction(string paymentMode)
        {
            string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");

            using (var context = new PosDbContext())
            {
                foreach (var item in Cart)
                {
                    double itemDiscount = item.Total * (DiscountPercentage / 100.0);
                    double netItemTotal = item.Total - itemDiscount;

                    var soldItem = new SoldItem
                    {
                        TransNo = transNo,
                        ProductCode = item.ProductCode,
                        Description = item.ProductDescription,
                        CostPrice = item.CostPrice,
                        Price = item.Price,
                        Qty = item.Qty,
                        Discount = itemDiscount,
                        Total = netItemTotal,
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
            return transNo;
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
            string transNo = SaveTransaction("Espèces");
            
            // Show receipt for Cash Payment
            var receiptWin = new CamaroesPOS.UI.Views.ReceiptWindow(transNo, new ObservableCollection<CartItem>(Cart), TotalAmount, ReceivedAmount, ChangeAmount, "Espèces");
            receiptWin.ShowDialog();
            
            IsCheckoutDialogOpen = false;
            Cart.Clear();
            DiscountPercentage = 0;
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
