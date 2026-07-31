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
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _allProducts;
        public ObservableCollection<Product> InventoryList { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public ObservableCollection<Vendor> Vendors { get; set; }
        public ObservableCollection<Brand> Brands { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        private bool _isNewProduct;

        private bool _isAdjustingStock;
        public bool IsAdjustingStock
        {
            get => _isAdjustingStock;
            set { _isAdjustingStock = value; OnPropertyChanged(); }
        }

        private int _adjustmentQuantity;
        public int AdjustmentQuantity
        {
            get => _adjustmentQuantity;
            set { _adjustmentQuantity = value; OnPropertyChanged(); }
        }

        private string _adjustmentJustification;
        public string AdjustmentJustification
        {
            get => _adjustmentJustification;
            set { _adjustmentJustification = value; OnPropertyChanged(); }
        }

        private string _adjustmentType;
        public string AdjustmentType
        {
            get => _adjustmentType;
            set { _adjustmentType = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AdjustmentTypes { get; set; }

        public bool IsAdmin => System.Windows.Application.Current.Properties["CurrentUserRole"]?.ToString() == "Admin";

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand AdjustStockCommand { get; }
        public ICommand SaveAdjustmentCommand { get; }
        public ICommand CancelAdjustmentCommand { get; }

        public InventoryViewModel()
        {
            _allProducts = new ObservableCollection<Product>();
            InventoryList = new ObservableCollection<Product>();
            Vendors = new ObservableCollection<Vendor>();
            Brands = new ObservableCollection<Brand>();
            Categories = new ObservableCollection<Category>();
            
            AddCommand = new RelayCommand<object>(OpenAddForm);
            EditCommand = new RelayCommand<Product>(OpenEditForm);
            DeleteCommand = new RelayCommand<Product>(DeleteProduct);
            SaveCommand = new RelayCommand<object>(SaveProduct);
            CancelCommand = new RelayCommand<object>(CancelEdit);
            
            ManageCatalogCommand = new RelayCommand<object>(OpenCatalogManager);

            AdjustmentTypes = new ObservableCollection<string> { "Entrée", "Sortie", "Ajustement (Inventaire)" };
            AdjustStockCommand = new RelayCommand<Product>(OpenAdjustForm);
            SaveAdjustmentCommand = new RelayCommand<object>(SaveAdjustment);
            CancelAdjustmentCommand = new RelayCommand<object>(CancelAdjustment);

            LoadDataFromDatabase();
        }

        private void FilterProducts()
        {
            InventoryList.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var p in _allProducts) InventoryList.Add(p);
            }
            else
            {
                var lower = SearchText.ToLower();
                var filtered = _allProducts.Where(p => 
                    (p.ProductCode != null && p.ProductCode.ToLower().Contains(lower)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lower)) ||
                    (p.CategoryName != null && p.CategoryName.ToLower().Contains(lower)) ||
                    (p.BrandName != null && p.BrandName.ToLower().Contains(lower))
                ).ToList();
                foreach (var p in filtered) InventoryList.Add(p);
            }
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new PosDbContext())
            {
                var products = context.Products.ToList();
                _allProducts.Clear();
                foreach (var p in products)
                {
                    _allProducts.Add(p);
                }

                var vendors = context.Vendors.ToList();
                Vendors.Clear();
                foreach(var v in vendors)
                {
                    Vendors.Add(v);
                }
                
                var brands = context.Brands.ToList();
                Brands.Clear();
                foreach(var b in brands) Brands.Add(b);

                var categories = context.Categories.ToList();
                Categories.Clear();
                foreach(var c in categories) Categories.Add(c);
            }
            FilterProducts();
        }

        public ICommand ManageCatalogCommand { get; }

        private void OpenCatalogManager(object obj)
        {
            var win = new CamaroesPOS.UI.Views.BrandCategoryWindow();
            win.ShowDialog();
            LoadDataFromDatabase(); // Rafraîchir les listes après la fermeture
        }

        private void OpenAddForm(object obj)
        {
            SelectedProduct = new Product
            {
                Reorder = 5
            };
            _isNewProduct = true;
            IsAdjustingStock = false;
            IsEditing = true;
        }

        private void OpenEditForm(Product product)
        {
            if (product == null) return;
            SelectedProduct = new Product
            {
                ProductCode = product.ProductCode,
                Description = product.Description,
                Price = product.Price,
                Qty = product.Qty,
                Reorder = product.Reorder,
                Barcode = product.Barcode,
                BrandName = product.BrandName,
                CategoryName = product.CategoryName,
                VendorId = product.VendorId,
                VendorName = product.VendorName
            };
            _isNewProduct = false;
            IsAdjustingStock = false;
            IsEditing = true;
        }

        private void DeleteProduct(Product product)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut supprimer un produit.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (product == null) return;
            
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer définitivement le produit '{product.Description}' ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
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
        }

        private void SaveProduct(object obj)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut modifier ou ajouter des produits.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedProduct.ProductCode) || string.IsNullOrWhiteSpace(SelectedProduct.Description))
            {
                MessageBox.Show("Le code produit et la description sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var vendor = Vendors.FirstOrDefault(v => v.Id == SelectedProduct.VendorId);
            if(vendor != null)
            {
                SelectedProduct.VendorName = vendor.VendorName;
            }

            using (var context = new PosDbContext())
            {
                if (_isNewProduct)
                {
                    if (context.Products.Any(p => p.ProductCode == SelectedProduct.ProductCode))
                    {
                        MessageBox.Show("Ce code produit existe déjà dans la base !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CancelEdit(object obj)
        {
            IsEditing = false;
            SelectedProduct = null;
        }

        private void OpenAdjustForm(Product product)
        {
            if (product == null) return;
            if (!IsAdmin)
            {
                MessageBox.Show("Seul l'administrateur peut ajuster les stocks.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedProduct = product;
            AdjustmentQuantity = 0;
            AdjustmentJustification = "";
            AdjustmentType = "Entrée";
            IsAdjustingStock = true;
            IsEditing = false;
        }

        private void SaveAdjustment(object obj)
        {
            if (SelectedProduct == null) return;

            if (AdjustmentQuantity <= 0 && AdjustmentType != "Ajustement (Inventaire)")
            {
                MessageBox.Show("La quantité doit être supérieure à zéro.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(AdjustmentJustification))
            {
                MessageBox.Show("Une justification est requise pour cette opération.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new PosDbContext())
            {
                var dbProduct = context.Products.FirstOrDefault(p => p.ProductCode == SelectedProduct.ProductCode);
                if (dbProduct != null)
                {
                    int diff = 0;
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
                            MessageBox.Show("Stock insuffisant pour cette sortie.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        dbProduct.Qty -= AdjustmentQuantity;
                    }
                    else // Ajustement (Inventaire)
                    {
                        diff = AdjustmentQuantity - dbProduct.Qty; // Difference par rapport à la valeur entrée
                        dbProduct.Qty = AdjustmentQuantity; // La valeur entrée devient la nouvelle Qty absolue
                    }

                    // Enregistrer l'historique
                    context.Stocks.Add(new Stock
                    {
                        ProductCode = dbProduct.ProductCode,
                        Description = dbProduct.Description,
                        Qty = diff,
                        StockInDate = System.DateTime.Now,
                        StockInBy = Application.Current.Properties["CurrentUser"]?.ToString() ?? "Admin",
                        Status = $"{AdjustmentType} - {AdjustmentJustification}",
                        RefNo = System.DateTime.Now.ToString("yyyyMMddHHmmss"),
                        VendorId = dbProduct.VendorId,
                        VendorName = dbProduct.VendorName
                    });

                    context.SaveChanges();
                    MessageBox.Show("Le stock a été ajusté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            IsAdjustingStock = false;
            LoadDataFromDatabase();
        }

        private void CancelAdjustment(object obj)
        {
            IsAdjustingStock = false;
            SelectedProduct = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
