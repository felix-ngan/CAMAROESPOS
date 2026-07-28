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
        public ObservableCollection<Product> InventoryList { get; set; }

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

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public InventoryViewModel()
        {
            InventoryList = new ObservableCollection<Product>();
            
            AddCommand = new RelayCommand<object>(OpenAddForm);
            EditCommand = new RelayCommand<Product>(OpenEditForm);
            DeleteCommand = new RelayCommand<Product>(DeleteProduct);
            SaveCommand = new RelayCommand<object>(SaveProduct);
            CancelCommand = new RelayCommand<object>(CancelEdit);

            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new PosDbContext())
            {
                var products = context.Products.ToList();
                InventoryList.Clear();
                foreach (var p in products)
                {
                    InventoryList.Add(p);
                }
            }
        }

        private void OpenAddForm(object obj)
        {
            SelectedProduct = new Product
            {
                Reorder = 5
            };
            _isNewProduct = true;
            IsEditing = true;
        }

        private void OpenEditForm(Product product)
        {
            if (product == null) return;
            // On crée une copie pour l'édition afin de ne pas modifier la ligne en direct
            SelectedProduct = new Product
            {
                ProductCode = product.ProductCode,
                Description = product.Description,
                Price = product.Price,
                Qty = product.Qty,
                Reorder = product.Reorder,
                Barcode = product.Barcode,
                BrandName = product.BrandName,
                CategoryName = product.CategoryName
            };
            _isNewProduct = false;
            IsEditing = true;
        }

        private void DeleteProduct(Product product)
        {
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
            if (string.IsNullOrWhiteSpace(SelectedProduct.ProductCode) || string.IsNullOrWhiteSpace(SelectedProduct.Description))
            {
                MessageBox.Show("Le code produit et la description sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
