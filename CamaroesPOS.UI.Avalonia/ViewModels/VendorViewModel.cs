using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class VendorViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<Vendor> _allVendors = new();
    public ObservableCollection<Vendor> Vendors { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); FilterVendors(); }
    }

    private Vendor? _selectedVendor;
    public Vendor? SelectedVendor { get => _selectedVendor; set { _selectedVendor = value; OnPropertyChanged(); } }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    private bool _isNewVendor;
    public bool IsNewVendor { get => _isNewVendor; set { _isNewVendor = value; OnPropertyChanged(); } }

    public bool IsAdmin => SessionService.IsAdmin;

    public RelayCommand<object?> CreateVendorCommand { get; }
    public RelayCommand<Vendor> EditVendorCommand { get; }
    public RelayCommand<Vendor> DeleteVendorCommand { get; }
    public RelayCommand<object?> SaveCommand { get; }
    public RelayCommand<object?> CancelCommand { get; }

    public VendorViewModel()
    {
        CreateVendorCommand = new RelayCommand<object?>(_ =>
        {
            SelectedVendor = new Vendor { VendorName = string.Empty, ContactPerson = string.Empty, Phone = string.Empty, Email = string.Empty, Address = string.Empty };
            IsEditing = true;
            IsNewVendor = true;
        });

        EditVendorCommand = new RelayCommand<Vendor>(v =>
        {
            if (v == null) return;
            SelectedVendor = new Vendor
            {
                Id = v.Id,
                VendorName = v.VendorName,
                ContactPerson = v.ContactPerson,
                Phone = v.Phone,
                Email = v.Email,
                Address = v.Address
            };
            IsEditing = true;
            IsNewVendor = false;
        });

        DeleteVendorCommand = new RelayCommand<Vendor>(DeleteVendorAsync);
        SaveCommand = new RelayCommand<object?>(_ => SaveVendorAsync());
        CancelCommand = new RelayCommand<object?>(_ => { IsEditing = false; SelectedVendor = null; });

        LoadVendors();
    }

    private void FilterVendors()
    {
        Vendors.Clear();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var v in _allVendors) Vendors.Add(v);
            return;
        }
        var lower = SearchText.ToLowerInvariant();
        foreach (var v in _allVendors.Where(v =>
            (v.VendorName != null && v.VendorName.ToLowerInvariant().Contains(lower)) ||
            (v.ContactPerson != null && v.ContactPerson.ToLowerInvariant().Contains(lower)) ||
            (v.Email != null && v.Email.ToLowerInvariant().Contains(lower))))
        {
            Vendors.Add(v);
        }
    }

    public void LoadVendors()
    {
        using var context = new PosDbContext();
        _allVendors.Clear();
        foreach (var v in context.Vendors.ToList()) _allVendors.Add(v);
        FilterVendors();
    }

    private async void SaveVendorAsync()
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut modifier ou ajouter des fournisseurs.", "Accès Refusé");
            return;
        }
        if (SelectedVendor == null || string.IsNullOrWhiteSpace(SelectedVendor.VendorName))
        {
            await DialogService.ShowErrorAsync("Le nom du fournisseur est obligatoire.", "Erreur");
            return;
        }

        SelectedVendor.ContactPerson ??= string.Empty;
        SelectedVendor.Phone ??= string.Empty;
        SelectedVendor.Email ??= string.Empty;
        SelectedVendor.Address ??= string.Empty;

        using (var context = new PosDbContext())
        {
            if (IsNewVendor)
            {
                context.Vendors.Add(SelectedVendor);
            }
            else
            {
                var existing = context.Vendors.FirstOrDefault(v => v.Id == SelectedVendor.Id);
                if (existing != null)
                {
                    existing.VendorName = SelectedVendor.VendorName;
                    existing.ContactPerson = SelectedVendor.ContactPerson;
                    existing.Phone = SelectedVendor.Phone;
                    existing.Email = SelectedVendor.Email;
                    existing.Address = SelectedVendor.Address;
                }
            }
            context.SaveChanges();
        }
        IsEditing = false;
        LoadVendors();
    }

    private async void DeleteVendorAsync(Vendor? vendor)
    {
        if (!IsAdmin)
        {
            await DialogService.ShowErrorAsync("Seul l'administrateur peut supprimer un fournisseur.", "Accès Refusé");
            return;
        }
        if (vendor == null) return;
        if (!await DialogService.ConfirmAsync($"Êtes-vous sûr de vouloir supprimer le fournisseur {vendor.VendorName} ?", "Confirmation"))
            return;

        using (var context = new PosDbContext())
        {
            if (context.Products.Any(p => p.VendorId == vendor.Id))
            {
                await DialogService.ShowErrorAsync("Impossible de supprimer ce fournisseur car il est associé à des produits.", "Erreur");
                return;
            }
            var v = context.Vendors.FirstOrDefault(x => x.Id == vendor.Id);
            if (v != null)
            {
                context.Vendors.Remove(v);
                context.SaveChanges();
            }
        }
        LoadVendors();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
