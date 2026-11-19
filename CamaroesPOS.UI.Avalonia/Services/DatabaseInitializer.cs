using System.Linq;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CamaroesPOS.UI.Avalonia.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var context = new PosDbContext();
        context.Database.EnsureCreated();

        var migrations = new[]
        {
            "ALTER TABLE Products ADD COLUMN CostPrice REAL NOT NULL DEFAULT 0;",
            "ALTER TABLE Products ADD COLUMN VendorId INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE Products ADD COLUMN VendorName TEXT;",
            "ALTER TABLE SoldItems ADD COLUMN CostPrice REAL NOT NULL DEFAULT 0;",
            "ALTER TABLE SoldItems ADD COLUMN PaymentMode TEXT;",
            "ALTER TABLE CustomerWallets ADD COLUMN CustomerName TEXT;",
            "ALTER TABLE CustomerWallets ADD COLUMN Email TEXT;",
            "ALTER TABLE CustomerWallets ADD COLUMN LoyaltyPoints INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE CustomerWallets ADD COLUMN TotalSpent REAL NOT NULL DEFAULT 0;",
            "CREATE TABLE IF NOT EXISTS Expenses (Id INTEGER NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY AUTOINCREMENT, Date TEXT NOT NULL, Category TEXT, Description TEXT, Amount REAL NOT NULL, AddedBy TEXT);",
            "CREATE TABLE IF NOT EXISTS Vendors (Id INTEGER NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY AUTOINCREMENT, VendorName TEXT, Address TEXT, ContactPerson TEXT, Phone TEXT, Email TEXT);",
            "CREATE TABLE IF NOT EXISTS WalletTransactions (Id INTEGER NOT NULL CONSTRAINT PK_WalletTransactions PRIMARY KEY AUTOINCREMENT, Phone TEXT, TransNo TEXT, Type TEXT, Amount REAL NOT NULL, CreatedAt TEXT NOT NULL);",
            "CREATE TABLE IF NOT EXISTS Brands (Id INTEGER NOT NULL CONSTRAINT PK_Brands PRIMARY KEY AUTOINCREMENT, BrandName TEXT);",
            "CREATE TABLE IF NOT EXISTS Categories (Id INTEGER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY AUTOINCREMENT, CategoryName TEXT);",
            "UPDATE Products SET VendorName = '' WHERE VendorName IS NULL;",
            "UPDATE Products SET BrandName = '' WHERE BrandName IS NULL;",
            "UPDATE Products SET CategoryName = '' WHERE CategoryName IS NULL;",
            "UPDATE Products SET Barcode = '' WHERE Barcode IS NULL;",
            "UPDATE CustomerWallets SET CustomerName = '' WHERE CustomerName IS NULL;",
            "UPDATE CustomerWallets SET Email = '' WHERE Email IS NULL;",
            "UPDATE SoldItems SET PaymentMode = 'Espèces' WHERE PaymentMode IS NULL;"
        };

        foreach (var sql in migrations)
        {
            try
            {
                context.Database.ExecuteSqlRaw(sql);
            }
            catch
            {
                // Column/table already exists
            }
        }

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

        if (!context.Users.Any())
        {
            context.Users.Add(new User { Username = "admin", Password = "123", Role = "Admin", Name = "Administrateur", IsActive = true });
            context.Users.Add(new User { Username = "caissier1", Password = "123", Role = "Cashier", Name = "Caissier 1", IsActive = true });
            context.SaveChanges();
        }
    }
}
