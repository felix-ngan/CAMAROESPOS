using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CamaroesPOS.Application.Services;
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

    private decimal _subTotalAmount;
    public decimal SubTotalAmount { get => _subTotalAmount; set { _subTotalAmount = value; OnPropertyChanged(); } }

    private decimal _discountPercentage;
    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            _discountPercentage = PosCalculator.ClampDiscount(value);
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
                DiscountPercentage = 0m;
            }
            else if (PosCalculator.TryParseDecimalInput(_discountPercentageString, out decimal parsed))
            {
                DiscountPercentage = parsed;
            }
        }
    }

    private decimal _totalAmount;
    public decimal TotalAmount { get => _totalAmount; set { _totalAmount = value; OnPropertyChanged(); } }

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

    private decimal _receivedAmount;
    public decimal ReceivedAmount { get => _receivedAmount; set { _receivedAmount = value; OnPropertyChanged(); } }

    private decimal _changeAmount;
    public decimal ChangeAmount { get => _changeAmount; set { _changeAmount = value; OnPropertyChanged(); } }

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
        if (qty <= 0) return;
        var existingItem = Cart.FirstOrDefault(c => c.ProductCode == product.ProductCode);
        int requestedTotal = (existingItem?.Qty ?? 0) + qty;
        if (product.Qty >= 0 && requestedTotal > product.Qty)
        {
            // Prevent overselling beyond available stock
            qty = Math.Max(0, product.Qty - (existingItem?.Qty ?? 0));
            if (qty <= 0) return;
        }

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
                IsTaxable = product.IsTaxable,
                Date = DateTime.Now
            });
        }

        UpdateTotalAmount();
        RaiseCommandsChanged();
    }

    private void UpdateTotalAmount()
    {
        var totals = PosCalculator.CalculateTotals(Cart, DiscountPercentage);
        SubTotalAmount = totals.SubTotalTTC;
        TotalAmount = totals.TotalTTC;
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

            string transNo = TransactionNumberGenerator.NextTransactionNo();
            using (var context = new PosDbContext())
            using (var tx = context.Database.BeginTransaction())
            {
                if (!ValidateDatabaseStock(context, out string stockErr))
                {
                    await DialogService.ShowErrorAsync(stockErr, "Stock insuffisant");
                    return;
                }

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

            decimal totalDiscountWallet = SubTotalAmount - TotalAmount;
            await ShowReceiptDialogAsync(transNo, TotalAmount, TotalAmount, 0m, "Portefeuille (Wallet)", totalDiscountWallet);

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

    private bool ValidateDatabaseStock(PosDbContext context, out string error)
    {
        error = string.Empty;
        foreach (var g in Cart.GroupBy(c => c.ProductCode))
        {
            var prod = context.Products.FirstOrDefault(p => p.ProductCode == g.Key);
            int needed = g.Sum(x => x.Qty);
            if (prod == null || needed > prod.Qty)
            {
                error = $"Stock insuffisant pour {g.Key} (Disponible : {prod?.Qty ?? 0}).";
                return false;
            }
        }
        return true;
    }

    private string SaveTransaction(string paymentMode)
    {
        string transNo = TransactionNumberGenerator.NextTransactionNo();
        using var context = new PosDbContext();
        SaveTransactionItems(context, transNo, paymentMode);
        context.SaveChanges();
        return transNo;
    }

    private void SaveTransactionItems(PosDbContext context, string transNo, string paymentMode)
    {
        foreach (var item in Cart)
        {
            var (itemDiscount, netItemTotal, lineHt, lineTax) = PosCalculator.CalculateLineBreakdown(item, DiscountPercentage);
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
                SubTotalHT = lineHt,
                TaxAmount = lineTax,
                Total = netItemTotal,
                SoldDate = DateTime.Now,
                Cashier = cashierName,
                PaymentMode = paymentMode,
                CustomerPhone = CustomerPhone ?? string.Empty,
                Status = "Completed"
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
                wallet.LoyaltyPoints += PosCalculator.CalculateLoyaltyPoints(TotalAmount);
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
        if (PosCalculator.TryParseDecimalInput(ReceivedAmountString, out decimal parsed))
        {
            ReceivedAmount = parsed;
            ChangeAmount = ReceivedAmount - TotalAmount;
        }
        else
        {
            ReceivedAmount = 0m;
            ChangeAmount = -TotalAmount;
        }
        ConfirmCashPaymentCommand.RaiseCanExecuteChanged();
    }

    private async void ConfirmCashPaymentAsync()
    {
        using (var checkCtx = new PosDbContext())
        {
            if (!ValidateDatabaseStock(checkCtx, out string stockErr))
            {
                await DialogService.ShowErrorAsync(stockErr, "Stock insuffisant");
                return;
            }
        }

        string transNo = SaveTransaction("Espèces");
        decimal totalDiscountCash = SubTotalAmount - TotalAmount;
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

    private async System.Threading.Tasks.Task ShowReceiptDialogAsync(string transNo, decimal total, decimal received, decimal change, string paymentMode, decimal discount)
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
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
