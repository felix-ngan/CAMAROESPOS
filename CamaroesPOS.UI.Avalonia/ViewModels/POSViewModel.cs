using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.Views;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class POSViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Product> Products { get; set; } = new();
    public ObservableCollection<CartItem> Cart { get; set; } = new();

    public RelayCommand<Product> AddToCartCommand { get; }
    public RelayCommand<object?> CheckoutCommand { get; }
    public RelayCommand<string> NumpadCommand { get; }
    public RelayCommand<object?> ClearNumpadCommand { get; }
    public RelayCommand<object?> CancelCheckoutCommand { get; }
    public RelayCommand<object?> ConfirmCashPaymentCommand { get; }
    public RelayCommand<object?> ExactAmountCommand { get; }
    public RelayCommand<string> QuickCashCommand { get; }
    public RelayCommand<object?> ClearCartCommand { get; }
    public RelayCommand<CartItem> RemoveFromCartCommand { get; }
    public RelayCommand<object?> OpenCatalogCommand { get; }
    public RelayCommand<object?> CloseCatalogCommand { get; }
    public RelayCommand<object?> SearchBarcodeCommand { get; }
    public RelayCommand<object?> ConfirmQuantityCommand { get; }
    public RelayCommand<object?> CancelQuantityCommand { get; }
    public RelayCommand<object?> IncreaseQtyCommand { get; }
    public RelayCommand<object?> DecreaseQtyCommand { get; }

    private double _subTotalAmount;
    public double SubTotalAmount { get => _subTotalAmount; set { _subTotalAmount = value; OnPropertyChanged(); } }

    private double _discountPercentage;
    public double DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0) value = 0;
            if (value > 100) value = 100;
            _discountPercentage = value;
            OnPropertyChanged();
            UpdateTotalAmount();
            RaiseCommandsChanged();
        }
    }

    private string _discountPercentageString = string.Empty;
    public string DiscountPercentageString
    {
        get => _discountPercentageString;
        set
        {
            _discountPercentageString = value ?? string.Empty;
            OnPropertyChanged();
            if (string.IsNullOrWhiteSpace(_discountPercentageString))
            {
                DiscountPercentage = 0;
            }
            else if (double.TryParse(_discountPercentageString, out double parsed) && !double.IsNaN(parsed) && !double.IsInfinity(parsed))
            {
                DiscountPercentage = parsed;
            }
        }
    }

    private double _totalAmount;
    public double TotalAmount { get => _totalAmount; set { _totalAmount = value; OnPropertyChanged(); } }

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
    public bool IsWalletPayment { get => _isWalletPayment; set { _isWalletPayment = value; OnPropertyChanged(); } }

    private string _customerPhone = string.Empty;
    public string CustomerPhone { get => _customerPhone; set { _customerPhone = value; OnPropertyChanged(); } }

    private string _barcodeSearch = string.Empty;
    public string BarcodeSearch { get => _barcodeSearch; set { _barcodeSearch = value; OnPropertyChanged(); } }

    private int _selectedQuantityToAdd = 1;
    public int SelectedQuantityToAdd { get => _selectedQuantityToAdd; set { _selectedQuantityToAdd = value; OnPropertyChanged(); } }

    private Product? _selectedProductToAdd;

    private bool _isCheckoutDialogOpen;
    public bool IsCheckoutDialogOpen
    {
        get => _isCheckoutDialogOpen;
        set { _isCheckoutDialogOpen = value; OnPropertyChanged(); UpdateAnyDialogOpen(); }
    }

    private bool _isCatalogDialogOpen;
    public bool IsCatalogDialogOpen
    {
        get => _isCatalogDialogOpen;
        set { _isCatalogDialogOpen = value; OnPropertyChanged(); UpdateAnyDialogOpen(); }
    }

    private bool _isQuantityDialogOpen;
    public bool IsQuantityDialogOpen
    {
        get => _isQuantityDialogOpen;
        set { _isQuantityDialogOpen = value; OnPropertyChanged(); UpdateAnyDialogOpen(); }
    }

    private bool _isAnyDialogOpen;
    public bool IsAnyDialogOpen { get => _isAnyDialogOpen; set { _isAnyDialogOpen = value; OnPropertyChanged(); } }

    private void UpdateAnyDialogOpen() => IsAnyDialogOpen = IsCheckoutDialogOpen || IsCatalogDialogOpen || IsQuantityDialogOpen;

    private string _receivedAmountString = string.Empty;
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
    public double ReceivedAmount { get => _receivedAmount; set { _receivedAmount = value; OnPropertyChanged(); } }

    private double _changeAmount;
    public double ChangeAmount { get => _changeAmount; set { _changeAmount = value; OnPropertyChanged(); } }

    public POSViewModel()
    {
        AddToCartCommand = new RelayCommand<Product>(AddToCart);
        CheckoutCommand = new RelayCommand<object?>(_ => CheckoutAsync(), _ => Cart.Count > 0 && TotalAmount > 0);
        NumpadCommand = new RelayCommand<string>(NumpadPress);
        ClearNumpadCommand = new RelayCommand<object?>(_ => ReceivedAmountString = string.Empty);
        CancelCheckoutCommand = new RelayCommand<object?>(_ => IsCheckoutDialogOpen = false);
        ConfirmCashPaymentCommand = new RelayCommand<object?>(_ => ConfirmCashPaymentAsync(), _ => ReceivedAmount >= TotalAmount && TotalAmount > 0);
        ExactAmountCommand = new RelayCommand<object?>(_ => ReceivedAmountString = Math.Ceiling(TotalAmount).ToString("0"));
        QuickCashCommand = new RelayCommand<string>(amount => ReceivedAmountString = amount);
        ClearCartCommand = new RelayCommand<object?>(_ => ClearCart(), _ => Cart.Count > 0);
        RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
        OpenCatalogCommand = new RelayCommand<object?>(_ => IsCatalogDialogOpen = true);
        CloseCatalogCommand = new RelayCommand<object?>(_ => IsCatalogDialogOpen = false);
        SearchBarcodeCommand = new RelayCommand<object?>(_ => SearchBarcodeAsync());
        ConfirmQuantityCommand = new RelayCommand<object?>(_ => ConfirmQuantity());
        CancelQuantityCommand = new RelayCommand<object?>(_ => IsQuantityDialogOpen = false);
        IncreaseQtyCommand = new RelayCommand<object?>(_ => SelectedQuantityToAdd++);
        DecreaseQtyCommand = new RelayCommand<object?>(_ => { if (SelectedQuantityToAdd > 1) SelectedQuantityToAdd--; });

        LoadDataFromDatabase();
    }

    private void RaiseCommandsChanged()
    {
        CheckoutCommand.RaiseCanExecuteChanged();
        ClearCartCommand.RaiseCanExecuteChanged();
        ConfirmCashPaymentCommand.RaiseCanExecuteChanged();
    }

    private void AddToCart(Product? product)
    {
        if (product == null) return;
        _selectedProductToAdd = product;
        SelectedQuantityToAdd = 1;
        IsQuantityDialogOpen = true;
    }

    private void ConfirmQuantity()
    {
        if (_selectedProductToAdd == null || SelectedQuantityToAdd <= 0) return;
        AddProductToCart(_selectedProductToAdd, SelectedQuantityToAdd);
        IsQuantityDialogOpen = false;
    }

    private async void SearchBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeSearch)) return;

        using var context = new PosDbContext();
        var product = context.Products.FirstOrDefault(p => p.Barcode == BarcodeSearch || p.ProductCode == BarcodeSearch);
        if (product != null)
        {
            AddProductToCart(product, 1);
        }
        else
        {
            await DialogService.ShowWarningAsync("Produit introuvable pour ce code-barres.", "Erreur");
        }
        BarcodeSearch = string.Empty;
    }

    public void AddProductToCart(Product product, int qty)
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
        RaiseCommandsChanged();
    }

    private void UpdateTotalAmount()
    {
        SubTotalAmount = Cart.Sum(c => c.Total);
        TotalAmount = SubTotalAmount - (SubTotalAmount * (DiscountPercentage / 100.0));
        UpdateChangeAmount();
    }

    private void ClearCart()
    {
        Cart.Clear();
        DiscountPercentageString = string.Empty;
        UpdateTotalAmount();
        RaiseCommandsChanged();
    }

    private void RemoveFromCart(CartItem? item)
    {
        if (item == null) return;
        Cart.Remove(item);
        UpdateTotalAmount();
        RaiseCommandsChanged();
    }

    private async void CheckoutAsync()
    {
        if (Cart.Count == 0 || TotalAmount <= 0) return;

        if (IsWalletPayment)
        {
            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                await DialogService.ShowErrorAsync("Veuillez entrer le numéro de téléphone du client pour payer par portefeuille.", "Erreur");
                return;
            }

            string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");
            using (var context = new PosDbContext())
            using (var tx = context.Database.BeginTransaction())
            {
                var wallet = context.CustomerWallets.FirstOrDefault(w => w.Phone == CustomerPhone);
                if (wallet == null)
                {
                    await DialogService.ShowErrorAsync("Ce portefeuille client n'existe pas.", "Erreur");
                    return;
                }

                if (wallet.Balance < TotalAmount)
                {
                    await DialogService.ShowErrorAsync($"Solde insuffisant dans le portefeuille de ce client (Solde: {wallet.Balance:N0} FCFA).", "Erreur");
                    return;
                }

                wallet.Balance -= TotalAmount;
                context.WalletTransactions.Add(new WalletTransaction
                {
                    Phone = wallet.Phone,
                    TransNo = transNo,
                    Type = "Debit",
                    Amount = TotalAmount,
                    CreatedAt = DateTime.Now
                });

                SaveTransactionItems(context, transNo, "Portefeuille");
                context.SaveChanges();
                tx.Commit();
            }

            double totalDiscountWallet = SubTotalAmount * (DiscountPercentage / 100.0);
            await ShowReceiptDialogAsync(transNo, TotalAmount, TotalAmount, 0, "Portefeuille (Wallet)", totalDiscountWallet);

            Cart.Clear();
            DiscountPercentageString = string.Empty;
            UpdateTotalAmount();
            CustomerPhone = string.Empty;
            LoadDataFromDatabase();
            RaiseCommandsChanged();
        }
        else
        {
            ReceivedAmountString = string.Empty;
            IsCheckoutDialogOpen = true;
        }
    }

    private string SaveTransaction(string paymentMode)
    {
        string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        using var context = new PosDbContext();
        SaveTransactionItems(context, transNo, paymentMode);
        context.SaveChanges();
        return transNo;
    }

    private void SaveTransactionItems(PosDbContext context, string transNo, string paymentMode)
    {
        foreach (var item in Cart)
        {
            double itemDiscount = item.Total * (DiscountPercentage / 100.0);
            double netItemTotal = item.Total - itemDiscount;
            string cashierName = SessionService.CurrentUserName ?? "Admin";

            context.SoldItems.Add(new SoldItem
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
                Cashier = cashierName,
                PaymentMode = paymentMode
            });

            var product = context.Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
            if (product != null)
            {
                product.Qty = Math.Max(0, product.Qty - item.Qty);
            }
        }

        if (!string.IsNullOrWhiteSpace(CustomerPhone))
        {
            var wallet = context.CustomerWallets.FirstOrDefault(w => w.Phone == CustomerPhone);
            if (wallet != null)
            {
                wallet.TotalSpent += TotalAmount;
                wallet.LoyaltyPoints += (int)(TotalAmount / 1000);
            }
        }
    }

    private void NumpadPress(string? digit)
    {
        if (string.IsNullOrEmpty(digit)) return;
        ReceivedAmountString = (ReceivedAmountString == "0" || string.IsNullOrEmpty(ReceivedAmountString))
            ? digit
            : ReceivedAmountString + digit;
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
        ConfirmCashPaymentCommand.RaiseCanExecuteChanged();
    }

    private async void ConfirmCashPaymentAsync()
    {
        string transNo = SaveTransaction("Espèces");
        double totalDiscountCash = SubTotalAmount * (DiscountPercentage / 100.0);
        await ShowReceiptDialogAsync(transNo, TotalAmount, ReceivedAmount, ChangeAmount, "Espèces", totalDiscountCash);

        IsCheckoutDialogOpen = false;
        Cart.Clear();
        DiscountPercentageString = string.Empty;
        UpdateTotalAmount();
        ReceivedAmountString = string.Empty;
        CustomerPhone = string.Empty;
        LoadDataFromDatabase();
        RaiseCommandsChanged();
    }

    private async System.Threading.Tasks.Task ShowReceiptDialogAsync(string transNo, double total, double received, double change, string paymentMode, double discount)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var receiptWin = new ReceiptWindow(transNo, new ObservableCollection<CartItem>(Cart), total, received, change, paymentMode, discount, CustomerPhone);
            await receiptWin.ShowDialog(desktop.MainWindow);
        }
    }

    public void LoadDataFromDatabase()
    {
        using var context = new PosDbContext();
        Products.Clear();
        foreach (var p in context.Products.ToList())
        {
            Products.Add(p);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
