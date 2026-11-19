using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class ReportsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<SoldItem> Transactions { get; } = new();
    public ObservableCollection<Product> ProductsList { get; } = new();
    public ObservableCollection<Stock> StockHistoryList { get; } = new();
    public ObservableCollection<CancelledOrder> CancelledOrdersList { get; } = new();
    public ObservableCollection<WalletTransaction> WalletTransactionsList { get; } = new();

    private DateTime? _startDate;
    public DateTime? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

    private DateTime? _endDate;
    public DateTime? EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

    private string _searchText = string.Empty;
    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

    private double _totalSalesAmount;
    public double TotalSalesAmount { get => _totalSalesAmount; set { _totalSalesAmount = value; OnPropertyChanged(); } }

    private int _totalItemsSold;
    public int TotalItemsSold { get => _totalItemsSold; set { _totalItemsSold = value; OnPropertyChanged(); } }

    public bool IsAdmin => SessionService.IsAdmin;

    public RelayCommand<object?> FilterCommand { get; }
    public RelayCommand<object?> ResetFilterCommand { get; }
    public RelayCommand<SoldItem> CancelTransactionCommand { get; }
    public RelayCommand<object?> ExportPdfCommand { get; }

    public ReportsViewModel()
    {
        FilterCommand = new RelayCommand<object?>(_ => ApplyFilter());
        ResetFilterCommand = new RelayCommand<object?>(_ => ResetFilter());
        CancelTransactionCommand = new RelayCommand<SoldItem>(CancelTransactionAsync);
        ExportPdfCommand = new RelayCommand<object?>(_ => ExportPdfAsync());

        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now.Date;

        LoadTransactions();
    }

    public void LoadTransactions() => ApplyFilter();

    private async void ExportPdfAsync()
    {
        try
        {
            var path = await FilePickerService.PickSavePdfPathAsync($"RapportVentes_{DateTime.Now:yyyyMMdd}.pdf", "Exporter le rapport des ventes");
            if (string.IsNullOrWhiteSpace(path)) return;

            double totalDiscounts = Transactions.Sum(t => (double?)t.Discount) ?? 0;
            new PdfService().GenerateSalesReportPdf(
                StartDate ?? DateTime.Now,
                EndDate ?? DateTime.Now,
                TotalSalesAmount,
                totalDiscounts,
                Transactions.Count,
                Transactions,
                path);

            await DialogService.ShowInfoAsync("Le rapport a été exporté en PDF avec succès !", "Succès");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Erreur lors de l'exportation PDF : {ex.Message}", "Erreur");
        }
    }

    private void ApplyFilter()
    {
        using var context = new PosDbContext();

        // 1. Ventes
        var querySales = context.SoldItems.AsQueryable();
        if (StartDate.HasValue) querySales = querySales.Where(t => t.SoldDate >= StartDate.Value.Date);
        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            querySales = querySales.Where(t => t.SoldDate < end);
        }
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            querySales = querySales.Where(t =>
                t.TransNo.ToLower().Contains(search) ||
                t.ProductCode.ToLower().Contains(search) ||
                t.Description.ToLower().Contains(search) ||
                t.Cashier.ToLower().Contains(search));
        }

        var salesResults = querySales.OrderByDescending(t => t.SoldDate).ToList();
        Transactions.Clear();
        foreach (var item in salesResults) Transactions.Add(item);
        TotalSalesAmount = salesResults.Sum(t => (double?)t.Total) ?? 0;
        TotalItemsSold = salesResults.Sum(t => (int?)t.Qty) ?? 0;

        // 2. Produits
        var queryProducts = context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            queryProducts = queryProducts.Where(p => p.ProductCode.ToLower().Contains(search) || p.Description.ToLower().Contains(search));
        }
        ProductsList.Clear();
        foreach (var item in queryProducts.OrderBy(p => p.Description).ToList()) ProductsList.Add(item);

        // 3. Historique Stock
        var queryStock = context.Stocks.AsQueryable();
        if (StartDate.HasValue) queryStock = queryStock.Where(s => s.StockInDate >= StartDate.Value.Date);
        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            queryStock = queryStock.Where(s => s.StockInDate < end);
        }
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            queryStock = queryStock.Where(s => s.ProductCode.ToLower().Contains(search) || s.Description.ToLower().Contains(search) || s.RefNo.ToLower().Contains(search));
        }
        StockHistoryList.Clear();
        foreach (var item in queryStock.OrderByDescending(s => s.StockInDate).ToList()) StockHistoryList.Add(item);

        // 4. Annulations
        var queryCancelled = context.CancelledOrders.AsQueryable();
        if (StartDate.HasValue) queryCancelled = queryCancelled.Where(c => c.SoldDate >= StartDate.Value.Date);
        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            queryCancelled = queryCancelled.Where(c => c.SoldDate < end);
        }
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            queryCancelled = queryCancelled.Where(c => c.TransNo.ToLower().Contains(search) || c.ProductCode.ToLower().Contains(search));
        }
        CancelledOrdersList.Clear();
        foreach (var item in queryCancelled.OrderByDescending(c => c.SoldDate).ToList()) CancelledOrdersList.Add(item);

        // 5. Opérations Portefeuille
        var queryWallet = context.WalletTransactions.AsQueryable();
        if (StartDate.HasValue) queryWallet = queryWallet.Where(w => w.CreatedAt >= StartDate.Value.Date);
        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            queryWallet = queryWallet.Where(w => w.CreatedAt < end);
        }
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.ToLower();
            queryWallet = queryWallet.Where(w => w.TransNo.ToLower().Contains(search) || w.Phone.ToLower().Contains(search));
        }
        WalletTransactionsList.Clear();
        foreach (var item in queryWallet.OrderByDescending(w => w.CreatedAt).ToList()) WalletTransactionsList.Add(item);
    }

    private void ResetFilter()
    {
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now.Date;
        SearchText = string.Empty;
        ApplyFilter();
    }

    private async void CancelTransactionAsync(SoldItem? soldItem)
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut annuler une transaction.", "Accès Refusé");
            return;
        }
        if (soldItem == null) return;

        if (!await DialogService.ConfirmAsync($"Êtes-vous sûr de vouloir annuler la transaction {soldItem.TransNo} pour {soldItem.Description} ?", "Annulation de Commande"))
            return;

        string reason = "Annulation demandée par l'administrateur.";
        string currentUser = SessionService.CurrentUserName ?? "Admin";

        using (var context = new PosDbContext())
        {
            var itemToCancel = context.SoldItems.FirstOrDefault(s => s.Id == soldItem.Id);
            if (itemToCancel != null)
            {
                var product = context.Products.FirstOrDefault(p => p.ProductCode == itemToCancel.ProductCode);
                if (product != null) product.Qty += itemToCancel.Qty;

                context.CancelledOrders.Add(new CancelledOrder
                {
                    TransNo = itemToCancel.TransNo,
                    ProductCode = itemToCancel.ProductCode,
                    Description = itemToCancel.Description,
                    Price = itemToCancel.Price,
                    Qty = itemToCancel.Qty,
                    Total = itemToCancel.Total,
                    SoldDate = itemToCancel.SoldDate,
                    VoidBy = currentUser,
                    CancelledBy = currentUser,
                    Reason = reason,
                    Action = "Annuler"
                });

                context.SoldItems.Remove(itemToCancel);
                context.SaveChanges();
            }
        }

        await DialogService.ShowInfoAsync("Transaction annulée et stock restauré avec succès.", "Succès");
        ApplyFilter();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
