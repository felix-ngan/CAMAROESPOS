using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class VendorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Vendor> Vendors { get; set; }
        public ICommand CreateVendorCommand { get; }
        public ICommand EditVendorCommand { get; }
        public ICommand DeleteVendorCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private Vendor _selectedVendor;
        public Vendor SelectedVendor
        {
            get => _selectedVendor;
            set { _selectedVendor = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private bool _isNewVendor;
        public bool IsNewVendor
        {
            get => _isNewVendor;
            set { _isNewVendor = value; OnPropertyChanged(); }
        }

        public bool IsAdmin => System.Windows.Application.Current.Properties["CurrentUserRole"]?.ToString() == "Admin";

        public VendorViewModel()
        {
            Vendors = new ObservableCollection<Vendor>();
            
            CreateVendorCommand = new RelayCommand<object>(o => {
                SelectedVendor = new Vendor();
                IsEditing = true;
                IsNewVendor = true;
            });

            EditVendorCommand = new RelayCommand<Vendor>(v => {
                if (v != null)
                {
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
                }
            });

            DeleteVendorCommand = new RelayCommand<Vendor>(DeleteVendor);
            SaveCommand = new RelayCommand<object>(SaveVendor, CanSave);
            CancelCommand = new RelayCommand<object>(o => { IsEditing = false; SelectedVendor = null; });

            LoadVendors();
        }

        private void LoadVendors()
        {
            using (var context = new PosDbContext())
            {
                var list = context.Vendors.ToList();
                Vendors.Clear();
                foreach(var v in list)
                    Vendors.Add(v);
            }
        }

        private bool CanSave(object obj)
        {
            return SelectedVendor != null && !string.IsNullOrWhiteSpace(SelectedVendor.VendorName);
        }

        private void SaveVendor(object obj)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut modifier ou ajouter des fournisseurs.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private void DeleteVendor(Vendor vendor)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut supprimer un fournisseur.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (vendor == null) return;

            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le fournisseur {vendor.VendorName} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var context = new PosDbContext())
                {
                    // Optionnel : Vérifier si le fournisseur est lié à des produits avant de supprimer
                    var isLinked = context.Products.Any(p => p.VendorId == vendor.Id);
                    if (isLinked)
                    {
                        MessageBox.Show("Impossible de supprimer ce fournisseur car il est associé à des produits.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
