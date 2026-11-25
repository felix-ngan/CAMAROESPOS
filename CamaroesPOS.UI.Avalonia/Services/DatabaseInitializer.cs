using System.Linq;
using CamaroesPOS.Application.Security;
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
            "ALTER TABLE Products ADD COLUMN IsTaxable INTEGER NOT NULL DEFAULT 1;",
            "ALTER TABLE SoldItems ADD COLUMN CostPrice REAL NOT NULL DEFAULT 0;",
            "ALTER TABLE SoldItems ADD COLUMN PaymentMode TEXT;",
            "ALTER TABLE SoldItems ADD COLUMN SubTotalHT REAL NOT NULL DEFAULT 0;",
            "ALTER TABLE SoldItems ADD COLUMN TaxAmount REAL NOT NULL DEFAULT 0;",
            "ALTER TABLE SoldItems ADD COLUMN PaymentReference TEXT DEFAULT '';",
            "ALTER TABLE SoldItems ADD COLUMN CustomerPhone TEXT DEFAULT '';",
            "ALTER TABLE SoldItems ADD COLUMN Status TEXT DEFAULT 'Completed';",
            "ALTER TABLE CancelledOrders ADD COLUMN CancelledAt TEXT DEFAULT '';",
            "ALTER TABLE CancelledOrders ADD COLUMN WalletRefunded INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE Stores ADD COLUMN Id INTEGER NOT NULL DEFAULT 1;",
            "ALTER TABLE Stores ADD COLUMN Phone TEXT DEFAULT '';",
            "ALTER TABLE Stores ADD COLUMN Niu TEXT DEFAULT '';",
            "ALTER TABLE Stores ADD COLUMN Rccm TEXT DEFAULT '';",
            "ALTER TABLE Stores ADD COLUMN TaxRate REAL NOT NULL DEFAULT 19.25;",
            "ALTER TABLE Stores ADD COLUMN Currency TEXT DEFAULT 'FCFA';",
            "ALTER TABLE CustomerWallets ADD COLUMN CustomerName TEXT;",
            "ALTER TABLE CustomerWallets ADD COLUMN Email TEXT;",
            "ALTER TABLE CustomerWallets ADD COLUMN LoyaltyPoints INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE CustomerWallets ADD COLUMN TotalSpent REAL NOT NULL DEFAULT 0;",
            "CREATE TABLE IF NOT EXISTS Expenses (Id INTEGER NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY AUTOINCREMENT, Date TEXT NOT NULL, Category TEXT, Description TEXT, Amount REAL NOT NULL, AddedBy TEXT);",
            "CREATE TABLE IF NOT EXISTS Vendors (Id INTEGER NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY AUTOINCREMENT, VendorName TEXT, Address TEXT, ContactPerson TEXT, Phone TEXT, Email TEXT);",
            "CREATE TABLE IF NOT EXISTS WalletTransactions (Id INTEGER NOT NULL CONSTRAINT PK_WalletTransactions PRIMARY KEY AUTOINCREMENT, Phone TEXT, TransNo TEXT, Type TEXT, Amount REAL NOT NULL, CreatedAt TEXT NOT NULL);",
            "CREATE TABLE IF NOT EXISTS Brands (Id INTEGER NOT NULL CONSTRAINT PK_Brands PRIMARY KEY AUTOINCREMENT, BrandName TEXT);",
            "CREATE TABLE IF NOT EXISTS Categories (Id INTEGER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY AUTOINCREMENT, CategoryName TEXT);",
            "CREATE TABLE IF NOT EXISTS CashShifts (Id INTEGER NOT NULL CONSTRAINT PK_CashShifts PRIMARY KEY AUTOINCREMENT, OpenedBy TEXT, OpenedAt TEXT NOT NULL, OpeningFloat REAL NOT NULL, ClosedAt TEXT, ClosedBy TEXT, ExpectedCash REAL NOT NULL, CountedCash REAL, Discrepancy REAL, TotalSales REAL NOT NULL, CashSales REAL NOT NULL, MobileMoneySales REAL NOT NULL, WalletSales REAL NOT NULL, TransactionCount INTEGER NOT NULL, Notes TEXT);",
            "CREATE TABLE IF NOT EXISTS AuditEntries (Id INTEGER NOT NULL CONSTRAINT PK_AuditEntries PRIMARY KEY AUTOINCREMENT, Timestamp TEXT NOT NULL, Actor TEXT, Role TEXT, Action TEXT, Target TEXT, Details TEXT, Severity TEXT);",
            "UPDATE Products SET VendorName = '' WHERE VendorName IS NULL;",
            "UPDATE Products SET BrandName = '' WHERE BrandName IS NULL;",
            "UPDATE Products SET CategoryName = '' WHERE CategoryName IS NULL;",
            "UPDATE Products SET Barcode = '' WHERE Barcode IS NULL;",
            "UPDATE CustomerWallets SET CustomerName = '' WHERE CustomerName IS NULL;",
            "UPDATE CustomerWallets SET Email = '' WHERE Email IS NULL;",
            "UPDATE SoldItems SET PaymentMode = 'Espèces' WHERE PaymentMode IS NULL;",
            "UPDATE SoldItems SET Status = 'Completed' WHERE Status IS NULL OR Status = '';",
            "UPDATE SoldItems SET CustomerPhone = '' WHERE CustomerPhone IS NULL;",
            "UPDATE SoldItems SET PaymentReference = '' WHERE PaymentReference IS NULL;"
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
            context.Products.Add(new Product { ProductCode = "P001", Description = "Laptop Gamer", Price = 1500000m, CostPrice = 1150000m, Qty = 10, Reorder = 4, Barcode = "12345", BrandName = "Asus", CategoryName = "Informatique", VendorName = "" });
            context.Products.Add(new Product { ProductCode = "P002", Description = "Souris Sans Fil", Price = 15000m, CostPrice = 9500m, Qty = 50, Reorder = 10, Barcode = "12346", BrandName = "Logitech", CategoryName = "Accessoires", VendorName = "" });
            context.Products.Add(new Product { ProductCode = "P003", Description = "Clavier Mécanique", Price = 45000m, CostPrice = 30000m, Qty = 15, Reorder = 5, Barcode = "12347", BrandName = "Corsair", CategoryName = "Accessoires", VendorName = "" });
            context.Products.Add(new Product { ProductCode = "P004", Description = "Écran 27 pouces", Price = 120000m, CostPrice = 90000m, Qty = 5, Reorder = 5, Barcode = "12348", BrandName = "Samsung", CategoryName = "Informatique", VendorName = "" });
            context.Products.Add(new Product { ProductCode = "P005", Description = "Casque Audio", Price = 35000m, CostPrice = 22000m, Qty = 20, Reorder = 6, Barcode = "12349", BrandName = "Sony", CategoryName = "Audio", VendorName = "" });
            context.SaveChanges();
        }

        if (!context.CustomerWallets.Any())
        {
            context.CustomerWallets.Add(new CustomerWallet { Phone = "0601020304", CustomerName = "Client Démo 1", Email = "", Balance = 150000m });
            context.CustomerWallets.Add(new CustomerWallet { Phone = "0611223344", CustomerName = "Client Démo 2", Email = "", Balance = 50000m });
            context.SaveChanges();
        }

        if (!context.Users.Any())
        {
            context.Users.Add(new User { Username = "admin", Password = PasswordHasher.Hash("123"), Role = "Admin", Name = "Administrateur", IsActive = true });
            context.Users.Add(new User { Username = "caissier1", Password = PasswordHasher.Hash("123"), Role = "Cashier", Name = "Caissier 1", IsActive = true });
            context.SaveChanges();
        }
        else
        {
            // Transparently upgrade any legacy plaintext passwords in SQLite to PBKDF2-SHA256
            bool migrated = false;
            foreach (var u in context.Users.ToList())
            {
                if (!string.IsNullOrEmpty(u.Password) && !PasswordHasher.IsHashed(u.Password))
                {
                    u.Password = PasswordHasher.Hash(u.Password);
                    migrated = true;
                }
            }
            if (migrated) context.SaveChanges();
        }
    }
}
