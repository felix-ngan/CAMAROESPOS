using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.ViewModels
{
    public class POSViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<CartItem> Cart { get; set; }

        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        private double _totalAmount;
        public double TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        public POSViewModel()
        {
            Products = new ObservableCollection<Product>();
            Cart = new ObservableCollection<CartItem>();
            
            AddToCartCommand = new RelayCommand<Product>(AddToCart);
            CheckoutCommand = new RelayCommand<object>(Checkout, CanCheckout);

            LoadDataFromDatabase();
        }

        private void AddToCart(Product product)
        {
            if (product == null) return;

            var existingItem = Cart.FirstOrDefault(c => c.ProductCode == product.ProductCode);
            if (existingItem != null)
            {
                existingItem.Qty++;
                existingItem.Total = existingItem.Qty * existingItem.Price;
                
                // Rafraîchir l'interface (car CartItem n'implémente pas INotifyPropertyChanged par défaut)
                int index = Cart.IndexOf(existingItem);
                Cart[index] = existingItem;
            }
            else
            {
                Cart.Add(new CartItem
                {
                    ProductCode = product.ProductCode,
                    ProductDescription = product.Description,
                    Price = product.Price,
                    Qty = 1,
                    Total = product.Price,
                    Date = DateTime.Now
                });
            }

            UpdateTotalAmount();
            // Force l'évaluation de CanCheckout
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateTotalAmount()
        {
            TotalAmount = Cart.Sum(c => c.Total);
        }

        private bool CanCheckout(object obj)
        {
            return Cart.Count > 0;
        }

        private void Checkout(object obj)
        {
            if (Cart.Count == 0) return;

            string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");

            using (var context = new CamaroesPOS.Data.PosDbContext())
            {
                foreach (var item in Cart)
                {
                    // Enregistrer la vente dans la table SoldItems
                    var soldItem = new SoldItem
                    {
                        TransNo = transNo,
                        ProductCode = item.ProductCode,
                        Description = item.ProductDescription,
                        Price = item.Price,
                        Qty = item.Qty,
                        Total = item.Total,
                        SoldDate = DateTime.Now,
                        Cashier = "Admin"
                    };
                    context.SoldItems.Add(soldItem);

                    // Déduire la quantité du stock
                    var product = context.Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
                    if (product != null)
                    {
                        product.Qty -= item.Qty;
                        if (product.Qty < 0) product.Qty = 0; // Sécurité
                    }
                }

                context.SaveChanges(); // Tout sauvegarder en une seule transaction
            }

            Cart.Clear();
            UpdateTotalAmount();
            
            // Rafraîchir la liste locale pour avoir les stocks à jour
            LoadDataFromDatabase();

            System.Windows.MessageBox.Show($"Encaissement réussi !\nN° Transaction : {transNo}", "Caisse", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void LoadDataFromDatabase()
        {
            using (var context = new CamaroesPOS.Data.PosDbContext())
            {
                var products = context.Products.ToList();
                Products.Clear();
                foreach (var p in products)
                {
                    Products.Add(p);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
