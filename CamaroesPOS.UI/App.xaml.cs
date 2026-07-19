using System.Windows;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using System.Linq;

namespace CamaroesPOS.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new PosDbContext())
            {
                // Créer la base de données SQLite si elle n'existe pas
                context.Database.EnsureCreated();

                // Injecter des données de démonstration dans SQLite si la base est vide
                if (!context.Products.Any())
                {
                    context.Products.Add(new Product { ProductCode = "P001", Description = "Laptop Gamer", Price = 1500000, Qty = 10, Barcode = "12345", BrandName = "Asus", CategoryName = "Informatique" });
                    context.Products.Add(new Product { ProductCode = "P002", Description = "Souris Sans Fil", Price = 15000, Qty = 50, Barcode = "12346", BrandName = "Logitech", CategoryName = "Accessoires" });
                    context.Products.Add(new Product { ProductCode = "P003", Description = "Clavier Mécanique", Price = 45000, Qty = 15, Barcode = "12347", BrandName = "Corsair", CategoryName = "Accessoires" });
                    context.Products.Add(new Product { ProductCode = "P004", Description = "Écran 27 pouces", Price = 120000, Qty = 5, Barcode = "12348", BrandName = "Samsung", CategoryName = "Informatique" });
                    context.Products.Add(new Product { ProductCode = "P005", Description = "Casque Audio", Price = 35000, Qty = 20, Barcode = "12349", BrandName = "Sony", CategoryName = "Audio" });
                    context.SaveChanges();
                }

                if (!context.CustomerWallets.Any())
                {
                    context.CustomerWallets.Add(new CustomerWallet { Phone = "0601020304", Balance = 150000 });
                    context.CustomerWallets.Add(new CustomerWallet { Phone = "0611223344", Balance = 50000 });
                    context.SaveChanges();
                }
            }
        }
    }
}
