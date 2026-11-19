using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class BrandCategoryViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    private string _newBrandName = string.Empty;
    public string NewBrandName
    {
        get => _newBrandName;
        set { _newBrandName = value; OnPropertyChanged(); SaveBrandCommand.RaiseCanExecuteChanged(); }
    }

    private string _newCategoryName = string.Empty;
    public string NewCategoryName
    {
        get => _newCategoryName;
        set { _newCategoryName = value; OnPropertyChanged(); SaveCategoryCommand.RaiseCanExecuteChanged(); }
    }

    private Brand? _selectedBrand;
    public Brand? SelectedBrand { get => _selectedBrand; set { _selectedBrand = value; OnPropertyChanged(); } }

    private bool _isEditingBrand;
    public bool IsEditingBrand { get => _isEditingBrand; set { _isEditingBrand = value; OnPropertyChanged(); } }

    private Category? _selectedCategory;
    public Category? SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

    private bool _isEditingCategory;
    public bool IsEditingCategory { get => _isEditingCategory; set { _isEditingCategory = value; OnPropertyChanged(); } }

    public RelayCommand<object?> SaveBrandCommand { get; }
    public RelayCommand<Brand> EditBrandCommand { get; }
    public RelayCommand<object?> CancelEditBrandCommand { get; }
    public RelayCommand<Brand> DeleteBrandCommand { get; }

    public RelayCommand<object?> SaveCategoryCommand { get; }
    public RelayCommand<Category> EditCategoryCommand { get; }
    public RelayCommand<object?> CancelEditCategoryCommand { get; }
    public RelayCommand<Category> DeleteCategoryCommand { get; }

    public BrandCategoryViewModel()
    {
        SaveBrandCommand = new RelayCommand<object?>(_ => SaveBrandAsync(), _ => !string.IsNullOrWhiteSpace(NewBrandName));
        EditBrandCommand = new RelayCommand<Brand>(EditBrand);
        CancelEditBrandCommand = new RelayCommand<object?>(_ => CancelEditBrand());
        DeleteBrandCommand = new RelayCommand<Brand>(DeleteBrandAsync);

        SaveCategoryCommand = new RelayCommand<object?>(_ => SaveCategoryAsync(), _ => !string.IsNullOrWhiteSpace(NewCategoryName));
        EditCategoryCommand = new RelayCommand<Category>(EditCategory);
        CancelEditCategoryCommand = new RelayCommand<object?>(_ => CancelEditCategory());
        DeleteCategoryCommand = new RelayCommand<Category>(DeleteCategoryAsync);

        LoadData();
    }

    private void LoadData()
    {
        using var context = new PosDbContext();
        Brands.Clear();
        foreach (var b in context.Brands.ToList()) Brands.Add(b);
        Categories.Clear();
        foreach (var c in context.Categories.ToList()) Categories.Add(c);
    }

    private async void SaveBrandAsync()
    {
        using (var context = new PosDbContext())
        {
            if (IsEditingBrand && SelectedBrand != null)
            {
                var b = context.Brands.FirstOrDefault(x => x.Id == SelectedBrand.Id);
                if (b != null)
                {
                    if (context.Brands.Any(x => x.BrandName == NewBrandName && x.Id != SelectedBrand.Id))
                    {
                        await DialogService.ShowErrorAsync("Cette marque existe déjà.", "Erreur");
                        return;
                    }
                    var oldName = b.BrandName;
                    b.BrandName = NewBrandName;
                    foreach (var p in context.Products.Where(p => p.BrandName == oldName).ToList())
                        p.BrandName = NewBrandName;
                    context.SaveChanges();
                }
                CancelEditBrand();
            }
            else
            {
                if (context.Brands.Any(b => b.BrandName == NewBrandName))
                {
                    await DialogService.ShowErrorAsync("Cette marque existe déjà.", "Erreur");
                    return;
                }
                context.Brands.Add(new Brand { BrandName = NewBrandName });
                context.SaveChanges();
                NewBrandName = string.Empty;
            }
        }
        LoadData();
    }

    private void EditBrand(Brand? brand)
    {
        if (brand == null) return;
        SelectedBrand = brand;
        NewBrandName = brand.BrandName ?? string.Empty;
        IsEditingBrand = true;
    }

    private void CancelEditBrand()
    {
        SelectedBrand = null;
        NewBrandName = string.Empty;
        IsEditingBrand = false;
    }

    private async void DeleteBrandAsync(Brand? brand)
    {
        if (brand == null) return;
        if (!await DialogService.ConfirmAsync($"Supprimer la marque {brand.BrandName} ?", "Confirmation")) return;

        using (var context = new PosDbContext())
        {
            var b = context.Brands.FirstOrDefault(x => x.Id == brand.Id);
            if (b != null)
            {
                context.Brands.Remove(b);
                context.SaveChanges();
            }
        }
        LoadData();
    }

    private async void SaveCategoryAsync()
    {
        using (var context = new PosDbContext())
        {
            if (IsEditingCategory && SelectedCategory != null)
            {
                var c = context.Categories.FirstOrDefault(x => x.Id == SelectedCategory.Id);
                if (c != null)
                {
                    if (context.Categories.Any(x => x.CategoryName == NewCategoryName && x.Id != SelectedCategory.Id))
                    {
                        await DialogService.ShowErrorAsync("Cette catégorie existe déjà.", "Erreur");
                        return;
                    }
                    var oldName = c.CategoryName;
                    c.CategoryName = NewCategoryName;
                    foreach (var p in context.Products.Where(p => p.CategoryName == oldName).ToList())
                        p.CategoryName = NewCategoryName;
                    context.SaveChanges();
                }
                CancelEditCategory();
            }
            else
            {
                if (context.Categories.Any(c => c.CategoryName == NewCategoryName))
                {
                    await DialogService.ShowErrorAsync("Cette catégorie existe déjà.", "Erreur");
                    return;
                }
                context.Categories.Add(new Category { CategoryName = NewCategoryName });
                context.SaveChanges();
                NewCategoryName = string.Empty;
            }
        }
        LoadData();
    }

    private void EditCategory(Category? category)
    {
        if (category == null) return;
        SelectedCategory = category;
        NewCategoryName = category.CategoryName ?? string.Empty;
        IsEditingCategory = true;
    }

    private void CancelEditCategory()
    {
        SelectedCategory = null;
        NewCategoryName = string.Empty;
        IsEditingCategory = false;
    }

    private async void DeleteCategoryAsync(Category? category)
    {
        if (category == null) return;
        if (!await DialogService.ConfirmAsync($"Supprimer la catégorie {category.CategoryName} ?", "Confirmation")) return;

        using (var context = new PosDbContext())
        {
            var c = context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (c != null)
            {
                context.Categories.Remove(c);
                context.SaveChanges();
            }
        }
        LoadData();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
