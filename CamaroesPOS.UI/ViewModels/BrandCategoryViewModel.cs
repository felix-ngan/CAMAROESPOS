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

        public ICommand AddBrandCommand { get; }
        public ICommand DeleteBrandCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public BrandCategoryViewModel()
        {
            Brands = new ObservableCollection<Brand>();
            Categories = new ObservableCollection<Category>();

            AddBrandCommand = new RelayCommand<object>(AddBrand, o => !string.IsNullOrWhiteSpace(NewBrandName));
            DeleteBrandCommand = new RelayCommand<Brand>(DeleteBrand);
            AddCategoryCommand = new RelayCommand<object>(AddCategory, o => !string.IsNullOrWhiteSpace(NewCategoryName));
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

        private void AddBrand(object obj)
        {
            using (var context = new PosDbContext())
            {
                if (context.Brands.Any(b => b.BrandName == NewBrandName))
                {
                    MessageBox.Show("Cette marque existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                context.Brands.Add(new Brand { BrandName = NewBrandName });
                context.SaveChanges();
            }
            NewBrandName = string.Empty;
            LoadData();
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

        private void AddCategory(object obj)
        {
            using (var context = new PosDbContext())
            {
                if (context.Categories.Any(c => c.CategoryName == NewCategoryName))
                {
                    MessageBox.Show("Cette catégorie existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                context.Categories.Add(new Category { CategoryName = NewCategoryName });
                context.SaveChanges();
            }
            NewCategoryName = string.Empty;
            LoadData();
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
