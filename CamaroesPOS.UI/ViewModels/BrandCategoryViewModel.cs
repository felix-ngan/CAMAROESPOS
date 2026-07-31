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
    public class BrandCategoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Brand> Brands { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        private string _newBrandName;
        public string NewBrandName
        {
            get => _newBrandName;
            set { _newBrandName = value; OnPropertyChanged(); }
        }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set { _newCategoryName = value; OnPropertyChanged(); }
        }

        private Brand _selectedBrand;
        public Brand SelectedBrand
        {
            get => _selectedBrand;
            set { _selectedBrand = value; OnPropertyChanged(); }
        }

        private bool _isEditingBrand;
        public bool IsEditingBrand
        {
            get => _isEditingBrand;
            set { _isEditingBrand = value; OnPropertyChanged(); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        private bool _isEditingCategory;
        public bool IsEditingCategory
        {
            get => _isEditingCategory;
            set { _isEditingCategory = value; OnPropertyChanged(); }
        }

        public ICommand SaveBrandCommand { get; }
        public ICommand EditBrandCommand { get; }
        public ICommand CancelEditBrandCommand { get; }
        public ICommand DeleteBrandCommand { get; }
        
        public ICommand SaveCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand CancelEditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public BrandCategoryViewModel()
        {
            Brands = new ObservableCollection<Brand>();
            Categories = new ObservableCollection<Category>();

            SaveBrandCommand = new RelayCommand<object>(SaveBrand, o => !string.IsNullOrWhiteSpace(NewBrandName));
            EditBrandCommand = new RelayCommand<Brand>(EditBrand);
            CancelEditBrandCommand = new RelayCommand<object>(CancelEditBrand);
            DeleteBrandCommand = new RelayCommand<Brand>(DeleteBrand);

            SaveCategoryCommand = new RelayCommand<object>(SaveCategory, o => !string.IsNullOrWhiteSpace(NewCategoryName));
            EditCategoryCommand = new RelayCommand<Category>(EditCategory);
            CancelEditCategoryCommand = new RelayCommand<object>(CancelEditCategory);
            DeleteCategoryCommand = new RelayCommand<Category>(DeleteCategory);

            LoadData();
        }

        private void LoadData()
        {
            using (var context = new PosDbContext())
            {
                var brands = context.Brands.ToList();
                Brands.Clear();
                foreach (var b in brands) Brands.Add(b);

                var categories = context.Categories.ToList();
                Categories.Clear();
                foreach (var c in categories) Categories.Add(c);
            }
        }

        private void SaveBrand(object obj)
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
                            MessageBox.Show("Cette marque existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        b.BrandName = NewBrandName;
                        context.SaveChanges();
                        
                        // Also update associated products
                        var products = context.Products.Where(p => p.BrandName == SelectedBrand.BrandName).ToList();
                        foreach(var p in products) p.BrandName = NewBrandName;
                        context.SaveChanges();
                    }
                    CancelEditBrand(null);
                }
                else
                {
                    if (context.Brands.Any(b => b.BrandName == NewBrandName))
                    {
                        MessageBox.Show("Cette marque existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    context.Brands.Add(new Brand { BrandName = NewBrandName });
                    context.SaveChanges();
                    NewBrandName = string.Empty;
                }
            }
            LoadData();
        }

        private void EditBrand(Brand brand)
        {
            if (brand == null) return;
            SelectedBrand = brand;
            NewBrandName = brand.BrandName;
            IsEditingBrand = true;
        }

        private void CancelEditBrand(object obj)
        {
            SelectedBrand = null;
            NewBrandName = string.Empty;
            IsEditingBrand = false;
        }

        private void DeleteBrand(Brand brand)
        {
            if (brand == null) return;
            var result = MessageBox.Show($"Supprimer la marque {brand.BrandName} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
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
        }

        private void SaveCategory(object obj)
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
                            MessageBox.Show("Cette catégorie existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        c.CategoryName = NewCategoryName;
                        context.SaveChanges();
                        
                        // Also update associated products
                        var products = context.Products.Where(p => p.CategoryName == SelectedCategory.CategoryName).ToList();
                        foreach(var p in products) p.CategoryName = NewCategoryName;
                        context.SaveChanges();
                    }
                    CancelEditCategory(null);
                }
                else
                {
                    if (context.Categories.Any(c => c.CategoryName == NewCategoryName))
                    {
                        MessageBox.Show("Cette catégorie existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    context.Categories.Add(new Category { CategoryName = NewCategoryName });
                    context.SaveChanges();
                    NewCategoryName = string.Empty;
                }
            }
            LoadData();
        }

        private void EditCategory(Category category)
        {
            if (category == null) return;
            SelectedCategory = category;
            NewCategoryName = category.CategoryName;
            IsEditingCategory = true;
        }

        private void CancelEditCategory(object obj)
        {
            SelectedCategory = null;
            NewCategoryName = string.Empty;
            IsEditingCategory = false;
        }

        private void DeleteCategory(Category category)
        {
            if (category == null) return;
            var result = MessageBox.Show($"Supprimer la catégorie {category.CategoryName} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
