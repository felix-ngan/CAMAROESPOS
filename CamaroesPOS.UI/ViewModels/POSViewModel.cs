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

            LoadDummyData();
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
            // Logique d'encaissement et de sauvegarde en BDD (Phase 7)
            Cart.Clear();
            UpdateTotalAmount();
            // Optionnel : Afficher un message de succès
        }

        private void LoadDummyData()
        {
            Products.Add(new Product { ProductCode = "P001", Description = "Laptop Gamer", Price = 1500000 });
            Products.Add(new Product { ProductCode = "P002", Description = "Souris Sans Fil", Price = 15000 });
            Products.Add(new Product { ProductCode = "P003", Description = "Clavier Mécanique", Price = 45000 });
            Products.Add(new Product { ProductCode = "P004", Description = "Écran 27 pouces", Price = 120000 });
            Products.Add(new Product { ProductCode = "P005", Description = "Casque Audio", Price = 35000 });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
