using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.Views;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class InventoryViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Product> _allProducts = new();
    public ObservableCollection<Product> InventoryList { get; } = new();
    public ObservableCollection<Vendor> Vendors { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<string> AdjustmentTypes { get; } = new() { "Entrée", "Sortie", "Ajustement (Inventaire)" };

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); FilterProducts(); }
    }

    private Product? _selectedProduct;
    public Product? SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged(); } }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    private bool _isNewProduct;

    private bool _isAdjustingStock;
    public bool IsAdjustingStock { get => _isAdjustingStock; set { _isAdjustingStock = value; OnPropertyChanged(); } }

    private int _adjustmentQuantity;
    public int AdjustmentQuantity { get => _adjustmentQuantity; set { _adjustmentQuantity = value; OnPropertyChanged(); } }

    private string _adjustmentJustification = string.Empty;
    public string AdjustmentJustification { get => _adjustmentJustification; set { _adjustmentJustification = value; OnPropertyChanged(); } }

    private string _adjustmentType = "Entrée";
    public string AdjustmentType { get => _adjustmentType; set { _adjustmentType = value; OnPropertyChanged(); } }

    public bool IsAdmin => SessionService.IsAdmin;

    public RelayCommand<object?> AddCommand { get; }
    public RelayCommand<Product> EditCommand { get; }
    public RelayCommand<Product> DeleteCommand { get; }
    public RelayCommand<object?> SaveCommand { get; }
    public RelayCommand<object?> CancelCommand { get; }
    public RelayCommand<object?> ManageCatalogCommand { get; }
    public RelayCommand<Product> AdjustStockCommand { get; }
    public RelayCommand<object?> SaveAdjustmentCommand { get; }
    public RelayCommand<object?> CancelAdjustmentCommand { get; }

    public InventoryViewModel()
    {
        AddCommand = new RelayCommand<object?>(_ => OpenAddForm());
        EditCommand = new RelayCommand<Product>(OpenEditForm);
        DeleteCommand = new RelayCommand<Product>(DeleteProductAsync);
        SaveCommand = new RelayCommand<object?>(_ => SaveProductAsync());
        CancelCommand = new RelayCommand<object?>(_ => { IsEditing = false; SelectedProduct = null; });
        ManageCatalogCommand = new RelayCommand<object?>(_ => OpenCatalogManagerAsync());
        AdjustStockCommand = new RelayCommand<Product>(OpenAdjustFormAsync);
        SaveAdjustmentCommand = new RelayCommand<object?>(_ => SaveAdjustmentAsync());
        CancelAdjustmentCommand = new RelayCommand<object?>(_ => { IsAdjustingStock = false; SelectedProduct = null; });

        LoadDataFromDatabase();
    }

    private void FilterProducts()
    {
        InventoryList.Clear();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var p in _allProducts) InventoryList.Add(p);
            return;
        }

        var lower = SearchText.ToLowerInvariant();
        foreach (var p in _allProducts.Where(p =>
            (p.ProductCode != null && p.ProductCode.ToLowerInvariant().Contains(lower)) ||
            (p.Description != null && p.Description.ToLowerInvariant().Contains(lower)) ||
            (p.CategoryName != null && p.CategoryName.ToLowerInvariant().Contains(lower)) ||
            (p.BrandName != null && p.BrandName.ToLowerInvariant().Contains(lower))))
        {
            InventoryList.Add(p);
        }
    }

    public void LoadDataFromDatabase()
    {
        using var context = new PosDbContext();
        _allProducts.Clear();
        foreach (var p in context.Products.ToList()) _allProducts.Add(p);

        Vendors.Clear();
        foreach (var v in context.Vendors.ToList()) Vendors.Add(v);

        Brands.Clear();
        foreach (var b in context.Brands.ToList()) Brands.Add(b);

        Categories.Clear();
        foreach (var c in context.Categories.ToList()) Categories.Add(c);

        FilterProducts();
    }

    private async void OpenCatalogManagerAsync()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var win = new BrandCategoryWindow();
            await win.ShowDialog(desktop.MainWindow);
            LoadDataFromDatabase();
        }
    }

    private void OpenAddForm()
    {
        if (!IsAdmin) return;
        SelectedProduct = new Product
        {
            ProductCode = string.Empty,
            Description = string.Empty,
            Barcode = string.Empty,
            BrandName = string.Empty,
            CategoryName = string.Empty,
            VendorName = string.Empty,
            Reorder = 5
        };
        _isNewProduct = true;
        IsAdjustingStock = false;
        IsEditing = true;
    }

    private void OpenEditForm(Product? product)
    {
        if (!IsAdmin || product == null) return;
        SelectedProduct = new Product
        {
            ProductCode = product.ProductCode,
            Description = product.Description,
            CostPrice = product.CostPrice,
            Price = product.Price,
            Qty = product.Qty,
            Reorder = product.Reorder,
            Barcode = product.Barcode ?? string.Empty,
            BrandName = product.BrandName ?? string.Empty,
            CategoryName = product.CategoryName ?? string.Empty,
            VendorId = product.VendorId,
            VendorName = product.VendorName ?? string.Empty
        };
        _isNewProduct = false;
        IsAdjustingStock = false;
        IsEditing = true;
    }

    private async void DeleteProductAsync(Product? product)
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut supprimer un produit.", "Accès Refusé");
            return;
        }
        if (product == null) return;

        if (!await DialogService.ConfirmAsync($"Êtes-vous sûr de vouloir supprimer définitivement le produit '{product.Description}' ?", "Confirmation"))
            return;

        using (var context = new PosDbContext())
        {
            var dbProduct = context.Products.FirstOrDefault(p => p.ProductCode == product.ProductCode);
            if (dbProduct != null)
            {
                context.Products.Remove(dbProduct);
                context.SaveChanges();
            }
        }
        LoadDataFromDatabase();
    }

    private async void SaveProductAsync()
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut modifier ou ajouter des produits.", "Accès Refusé");
            return;
        }
        if (SelectedProduct == null || string.IsNullOrWhiteSpace(SelectedProduct.ProductCode) || string.IsNullOrWhiteSpace(SelectedProduct.Description))
        {
            await DialogService.ShowErrorAsync("Le code produit et la description sont obligatoires.", "Erreur");
            return;
        }

        SelectedProduct.Barcode ??= string.Empty;
        SelectedProduct.BrandName ??= string.Empty;
        SelectedProduct.CategoryName ??= string.Empty;
        var vendor = Vendors.FirstOrDefault(v => v.Id == SelectedProduct.VendorId);
        SelectedProduct.VendorName = vendor?.VendorName ?? SelectedProduct.VendorName ?? string.Empty;

        using (var context = new PosDbContext())
        {
            if (_isNewProduct)
            {
                if (context.Products.Any(p => p.ProductCode == SelectedProduct.ProductCode))
                {
                    await DialogService.ShowErrorAsync("Ce code produit existe déjà dans la base !", "Erreur");
                    return;
                }
                context.Products.Add(SelectedProduct);
            }
            else
            {
                var dbProduct = context.Products.FirstOrDefault(p => p.ProductCode == SelectedProduct.ProductCode);
                if (dbProduct != null)
                {
                    dbProduct.Description = SelectedProduct.Description;
                    dbProduct.CostPrice = SelectedProduct.CostPrice;
                    dbProduct.Price = SelectedProduct.Price;
                    dbProduct.Qty = SelectedProduct.Qty;
                    dbProduct.Reorder = SelectedProduct.Reorder;
                    dbProduct.Barcode = SelectedProduct.Barcode;
                    dbProduct.BrandName = SelectedProduct.BrandName;
                    dbProduct.CategoryName = SelectedProduct.CategoryName;
                    dbProduct.VendorId = SelectedProduct.VendorId;
                    dbProduct.VendorName = SelectedProduct.VendorName;
                }
            }
            context.SaveChanges();
        }

        IsEditing = false;
        LoadDataFromDatabase();
    }

    private async void OpenAdjustFormAsync(Product? product)
    {
        if (product == null) return;
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut ajuster les stocks.", "Accès Refusé");
            return;
        }

        SelectedProduct = product;
        AdjustmentQuantity = 0;
        AdjustmentJustification = string.Empty;
        AdjustmentType = "Entrée";
        IsAdjustingStock = true;
        IsEditing = false;
    }

    private async void SaveAdjustmentAsync()
    {
        if (SelectedProduct == null) return;

        if (AdjustmentQuantity < 0 || (AdjustmentQuantity == 0 && AdjustmentType != "Ajustement (Inventaire)"))
        {
            await DialogService.ShowErrorAsync("La quantité doit être valide et non négative.", "Erreur");
            return;
        }
        if (string.IsNullOrWhiteSpace(AdjustmentJustification))
        {
            await DialogService.ShowErrorAsync("Une justification est requise pour cette opération.", "Erreur");
            return;
        }

        using (var context = new PosDbContext())
        {
            var dbProduct = context.Products.FirstOrDefault(p => p.ProductCode == SelectedProduct.ProductCode);
            if (dbProduct != null)
            {
                int diff;
                if (AdjustmentType == "Entrée")
                {
                    diff = AdjustmentQuantity;
                    dbProduct.Qty += diff;
                }
                else if (AdjustmentType == "Sortie")
                {
                    diff = -AdjustmentQuantity;
                    if (dbProduct.Qty < AdjustmentQuantity)
                    {
                        await DialogService.ShowErrorAsync("Stock insuffisant pour cette sortie.", "Erreur");
                        return;
                    }
                    dbProduct.Qty -= AdjustmentQuantity;
                }
                else
                {
                    diff = AdjustmentQuantity - dbProduct.Qty;
                    dbProduct.Qty = AdjustmentQuantity;
                }

                context.Stocks.Add(new Stock
                {
                    ProductCode = dbProduct.ProductCode,
                    Description = dbProduct.Description,
                    Qty = diff,
                    StockInDate = DateTime.Now,
                    StockInBy = SessionService.CurrentUser ?? "Admin",
                    Status = $"{AdjustmentType} - {AdjustmentJustification}",
                    RefNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    VendorId = dbProduct.VendorId,
                    VendorName = dbProduct.VendorName ?? string.Empty
                });
                context.SaveChanges();
            }
        }

        await DialogService.ShowInfoAsync("Le stock a été ajusté avec succès.", "Succès");
        IsAdjustingStock = false;
        LoadDataFromDatabase();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
