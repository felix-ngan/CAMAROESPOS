using System.Globalization;
using System.Text.Json;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public record OperationResult(bool Success, string Message);

public record CheckoutResult(
    bool Success,
    string Message,
    string TransNo = "",
    double SubTotal = 0,
    double DiscountAmount = 0,
    double TotalAmount = 0,
    double ReceivedAmount = 0,
    double ChangeAmount = 0,
    string PaymentMode = "");

public class DashboardMetrics
{
    public double TodaySales { get; set; }
    public double TotalSalesAllTime { get; set; }
    public double YesterdaySales { get; set; }
    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public int LowStockItems { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalLoyaltyPoints { get; set; }
    public double TotalWalletsBalance { get; set; }
    public int TotalItemsSoldToday { get; set; }
    public int TotalItemsSoldAllTime { get; set; }
    public double TotalCancelledAmount { get; set; }
    public double TotalDiscountAmount { get; set; }
    public int TotalUsers { get; set; }
    public int AdminCount { get; set; }
    public int CashierCount { get; set; }
    public int TotalVendors { get; set; }
    public double TotalCOGS { get; set; }
    public double TotalExpenses { get; set; }
    public double NetProfit { get; set; }

    public string[] Last7DaysLabels { get; set; } = Array.Empty<string>();
    public double[] Last7DaysSales { get; set; } = Array.Empty<double>();
    public double[] Last7DaysStockIn { get; set; } = Array.Empty<double>();
    public double[] Last7DaysStockOut { get; set; } = Array.Empty<double>();
    public double[] Last7DaysDiscounts { get; set; } = Array.Empty<double>();
    public double[] Last7DaysCancelled { get; set; } = Array.Empty<double>();

    public List<(string Label, double Value)> TopProductsStock { get; set; } = new();
    public List<(string Label, double Value)> TopCustomers { get; set; } = new();
    public List<(string Label, double Value)> TopSoldItems { get; set; } = new();
    public List<(string Label, double Value)> PaymentModes { get; set; } = new();
    public List<(string Label, double Value)> UsersByRole { get; set; } = new();
    public List<(string Label, double Value)> VendorsByProducts { get; set; } = new();
    public List<Product> CriticalStockList { get; set; } = new();
}

public class PosStoreState
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public List<Product> Products { get; private set; } = new();
    public List<Brand> Brands { get; private set; } = new();
    public List<Category> Categories { get; private set; } = new();
    public List<Vendor> Vendors { get; private set; } = new();
    public List<CustomerWallet> CustomerWallets { get; private set; } = new();
    public List<WalletTransaction> WalletTransactions { get; private set; } = new();
    public List<SoldItem> SoldItems { get; private set; } = new();
    public List<Stock> Stocks { get; private set; } = new();
    public List<CancelledOrder> CancelledOrders { get; private set; } = new();
    public List<Expense> Expenses { get; private set; } = new();
    public List<User> Users { get; private set; } = new();
    public Store Store { get; private set; } = new()
    {
        StoreName = "CAMAROES POS",
        Address = "Douala, Cameroun",
        Logo = Array.Empty<byte>(),
        MomoCode = "MTN-237-001",
        OrangeCode = "OM-237-002"
    };

    public bool IsInitialized { get; private set; }
    public event Action? OnChanged;

    private void Notify() => OnChanged?.Invoke();

    public void SeedDemoData()
    {
        var today = DateTime.Today;

        Brands = new List<Brand>
        {
            new() { Id = 1, BrandName = "Asus" },
            new() { Id = 2, BrandName = "Logitech" },
            new() { Id = 3, BrandName = "Corsair" },
            new() { Id = 4, BrandName = "Samsung" },
            new() { Id = 5, BrandName = "Sony" }
        };

        Categories = new List<Category>
        {
            new() { Id = 1, CategoryName = "Informatique" },
            new() { Id = 2, CategoryName = "Accessoires" },
            new() { Id = 3, CategoryName = "Audio" }
        };

        Vendors = new List<Vendor>
        {
            new() { Id = 1, VendorName = "TechDistrib Douala", ContactPerson = "Jean Mbarga", Phone = "+237 690 11 22 33", Email = "contact@techdistrib.cm", Address = "Akwa, Douala" },
            new() { Id = 2, VendorName = "AudioVision SARL", ContactPerson = "Marie Ndi", Phone = "+237 677 44 55 66", Email = "ventes@audiovision.cm", Address = "Bonapriso, Douala" }
        };

        Products = new List<Product>
        {
            new() { ProductCode = "P001", Barcode = "12345", Description = "Laptop Gamer", BrandId = 1, BrandName = "Asus", CategoryId = 1, CategoryName = "Informatique", CostPrice = 1150000, Price = 1500000, Qty = 10, Reorder = 4, VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { ProductCode = "P002", Barcode = "12346", Description = "Souris Sans Fil", BrandId = 2, BrandName = "Logitech", CategoryId = 2, CategoryName = "Accessoires", CostPrice = 9500, Price = 15000, Qty = 50, Reorder = 10, VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { ProductCode = "P003", Barcode = "12347", Description = "Clavier Mécanique", BrandId = 3, BrandName = "Corsair", CategoryId = 2, CategoryName = "Accessoires", CostPrice = 30000, Price = 45000, Qty = 15, Reorder = 5, VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { ProductCode = "P004", Barcode = "12348", Description = "Écran 27 pouces", BrandId = 4, BrandName = "Samsung", CategoryId = 1, CategoryName = "Informatique", CostPrice = 90000, Price = 120000, Qty = 5, Reorder = 5, VendorId = 2, VendorName = "AudioVision SARL" },
            new() { ProductCode = "P005", Barcode = "12349", Description = "Casque Audio", BrandId = 5, BrandName = "Sony", CategoryId = 3, CategoryName = "Audio", CostPrice = 22000, Price = 35000, Qty = 20, Reorder = 6, VendorId = 2, VendorName = "AudioVision SARL" },
            new() { ProductCode = "P006", Barcode = "12350", Description = "SSD NVMe 1 To", BrandId = 4, BrandName = "Samsung", CategoryId = 1, CategoryName = "Informatique", CostPrice = 48000, Price = 68000, Qty = 3, Reorder = 5, VendorId = 1, VendorName = "TechDistrib Douala" }
        };

        CustomerWallets = new List<CustomerWallet>
        {
            new() { Phone = "0601020304", CustomerName = "Paul Kamga", Email = "paul.kamga@mail.cm", Balance = 150000, LoyaltyPoints = 240, TotalSpent = 240000 },
            new() { Phone = "0611223344", CustomerName = "Amina Bello", Email = "amina.bello@mail.cm", Balance = 50000, LoyaltyPoints = 155, TotalSpent = 155000 },
            new() { Phone = "0655443322", CustomerName = "Eric Fotso", Email = "eric.fotso@mail.cm", Balance = 85000, LoyaltyPoints = 90, TotalSpent = 90000 }
        };

        Users = new List<User>
        {
            new() { Username = "admin", Password = "123", Role = "Admin", Name = "Administrateur", IsActive = true },
            new() { Username = "caissier1", Password = "123", Role = "Cashier", Name = "Caissier 1", IsActive = true }
        };

        // 7-day sample sales so Dashboard charts look alive on first visit
        SoldItems = new List<SoldItem>
        {
            new() { Id = 1, TransNo = "20261114093000", ProductCode = "P002", Description = "Souris Sans Fil", CostPrice = 9500, Price = 15000, Qty = 3, Discount = 0, Total = 45000, SoldDate = today.AddDays(-5).AddHours(10), Cashier = "Caissier 1", PaymentMode = "Espèces" },
            new() { Id = 2, TransNo = "20261115111500", ProductCode = "P004", Description = "Écran 27 pouces", CostPrice = 90000, Price = 120000, Qty = 1, Discount = 6000, Total = 114000, SoldDate = today.AddDays(-4).AddHours(11), Cashier = "Administrateur", PaymentMode = "Portefeuille" },
            new() { Id = 3, TransNo = "20261116142000", ProductCode = "P003", Description = "Clavier Mécanique", CostPrice = 30000, Price = 45000, Qty = 2, Discount = 4500, Total = 85500, SoldDate = today.AddDays(-3).AddHours(14), Cashier = "Caissier 1", PaymentMode = "Espèces" },
            new() { Id = 4, TransNo = "20261117160000", ProductCode = "P005", Description = "Casque Audio", CostPrice = 22000, Price = 35000, Qty = 3, Discount = 0, Total = 105000, SoldDate = today.AddDays(-2).AddHours(16), Cashier = "Caissier 1", PaymentMode = "Portefeuille" },
            new() { Id = 5, TransNo = "20261118104500", ProductCode = "P006", Description = "SSD NVMe 1 To", CostPrice = 48000, Price = 68000, Qty = 2, Discount = 6800, Total = 129200, SoldDate = today.AddDays(-1).AddHours(10), Cashier = "Administrateur", PaymentMode = "Espèces" },
            new() { Id = 6, TransNo = "20261119091500", ProductCode = "P003", Description = "Clavier Mécanique", CostPrice = 30000, Price = 45000, Qty = 2, Discount = 0, Total = 90000, SoldDate = today.AddHours(9), Cashier = "Caissier 1", PaymentMode = "Espèces" },
            new() { Id = 7, TransNo = "20261119110500", ProductCode = "P005", Description = "Casque Audio", CostPrice = 22000, Price = 35000, Qty = 2, Discount = 3500, Total = 66500, SoldDate = today.AddHours(11), Cashier = "Caissier 1", PaymentMode = "Portefeuille" }
        };

        Stocks = new List<Stock>
        {
            new() { Id = 1, RefNo = "STK-001", ProductCode = "P001", Description = "Laptop Gamer", Qty = 10, StockInDate = today.AddDays(-5), StockInBy = "admin", Status = "Entrée - Stock initial", VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { Id = 2, RefNo = "STK-002", ProductCode = "P002", Description = "Souris Sans Fil", Qty = 25, StockInDate = today.AddDays(-3), StockInBy = "admin", Status = "Entrée - Réapprovisionnement", VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { Id = 3, RefNo = "STK-003", ProductCode = "P005", Description = "Casque Audio", Qty = 12, StockInDate = today.AddDays(-1), StockInBy = "admin", Status = "Entrée - Livraison fournisseur", VendorId = 2, VendorName = "AudioVision SARL" }
        };

        WalletTransactions = new List<WalletTransaction>
        {
            new() { Id = 1, Phone = "0601020304", TransNo = "WLT-001", Type = "Credit", Amount = 200000, CreatedAt = today.AddDays(-4) },
            new() { Id = 2, Phone = "0611223344", TransNo = "20261119110500", Type = "Debit", Amount = 66500, CreatedAt = today.AddHours(11) }
        };

        CancelledOrders = new List<CancelledOrder>
        {
            new() { Id = 1, TransNo = "20261117083000", ProductCode = "P002", Description = "Souris Sans Fil", Price = 15000, Qty = 1, Total = 15000, SoldDate = today.AddDays(-2), VoidBy = "Administrateur", CancelledBy = "Administrateur", Reason = "Erreur de caisse", Action = "Annuler" }
        };

        Expenses = new List<Expense>
        {
            new() { Id = 1, Date = today.AddDays(-4), Category = "Électricité", Description = "Facture ENEO Boutique", Amount = 45000, AddedBy = "Administrateur" },
            new() { Id = 2, Date = today.AddDays(-2), Category = "Transport", Description = "Livraison fournisseurs Akwa", Amount = 18000, AddedBy = "Administrateur" }
        };

        Store = new Store
        {
            StoreName = "CAMAROES POS",
            Address = "Douala, Cameroun",
            Logo = Array.Empty<byte>(),
            MomoCode = "MTN-237-001",
            OrangeCode = "OM-237-002"
        };

        IsInitialized = true;
        Notify();
    }

    public string ExportSnapshotJson()
    {
        var snapshot = new PosStoreSnapshot
        {
            Products = Products,
            Brands = Brands,
            Categories = Categories,
            Vendors = Vendors,
            CustomerWallets = CustomerWallets,
            WalletTransactions = WalletTransactions,
            SoldItems = SoldItems,
            Stocks = Stocks,
            CancelledOrders = CancelledOrders,
            Expenses = Expenses,
            Users = Users,
            Store = Store
        };
        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    public void ImportSnapshotJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            SeedDemoData();
            return;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<PosStoreSnapshot>(json, JsonOptions);
            if (snapshot == null || snapshot.Users == null || snapshot.Users.Count == 0)
            {
                SeedDemoData();
                return;
            }

            Products = snapshot.Products ?? new();
            Brands = snapshot.Brands ?? new();
            Categories = snapshot.Categories ?? new();
            Vendors = snapshot.Vendors ?? new();
            CustomerWallets = snapshot.CustomerWallets ?? new();
            WalletTransactions = snapshot.WalletTransactions ?? new();
            SoldItems = snapshot.SoldItems ?? new();
            Stocks = snapshot.Stocks ?? new();
            CancelledOrders = snapshot.CancelledOrders ?? new();
            Expenses = snapshot.Expenses ?? new();
            Users = snapshot.Users;
            Store = snapshot.Store ?? Store;
            IsInitialized = true;
            Notify();
        }
        catch
        {
            SeedDemoData();
        }
    }

    public User? Authenticate(string username, string password)
        => Users.FirstOrDefault(u => u.Username == username && u.Password == password && u.IsActive);

    public static double ClampDiscount(double discountPercent)
    {
        if (!double.IsFinite(discountPercent) || discountPercent < 0) return 0;
        if (discountPercent > 100) return 100;
        return discountPercent;
    }

    public static bool TryParseDecimalInput(string? raw, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var normalized = raw.Trim().Replace(" ", "").Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            return false;
        if (!double.IsFinite(parsed)) return false;
        value = parsed;
        return true;
    }

    public CheckoutResult CheckoutCash(List<CartItem> cart, double discountPercent, double receivedAmount, string? customerPhone, string cashier)
    {
        if (cart == null || cart.Count == 0)
            return new CheckoutResult(false, "Le panier est vide.");

        double discount = ClampDiscount(discountPercent);
        double subTotal = cart.Sum(c => c.Total);
        double discountAmount = subTotal * (discount / 100.0);
        double total = subTotal - discountAmount;

        if (!double.IsFinite(total) || total <= 0)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        if (!double.IsFinite(receivedAmount) || receivedAmount < total)
            return new CheckoutResult(false, "Le montant reçu est insuffisant ou invalide.");

        string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        RecordSaleItems(cart, transNo, discount, total, customerPhone, cashier, "Espèces");
        Notify();

        return new CheckoutResult(true, "Vente enregistrée.", transNo, subTotal, discountAmount, total, receivedAmount, receivedAmount - total, "Espèces");
    }

    public CheckoutResult CheckoutWallet(List<CartItem> cart, double discountPercent, string customerPhone, string cashier)
    {
        if (cart == null || cart.Count == 0)
            return new CheckoutResult(false, "Le panier est vide.");

        if (string.IsNullOrWhiteSpace(customerPhone))
            return new CheckoutResult(false, "Veuillez entrer le numéro de téléphone du client pour payer par portefeuille.");

        double discount = ClampDiscount(discountPercent);
        double subTotal = cart.Sum(c => c.Total);
        double discountAmount = subTotal * (discount / 100.0);
        double total = subTotal - discountAmount;

        if (total <= 0)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == customerPhone);
        if (wallet == null)
            return new CheckoutResult(false, "Ce portefeuille client n'existe pas.");

        if (wallet.Balance < total)
            return new CheckoutResult(false, $"Solde insuffisant dans le portefeuille de ce client (Solde: {wallet.Balance:N0} FCFA).");

        string transNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        wallet.Balance -= total;
        WalletTransactions.Add(new WalletTransaction
        {
            Id = NextId(WalletTransactions.Select(w => w.Id)),
            Phone = wallet.Phone,
            TransNo = transNo,
            Type = "Debit",
            Amount = total,
            CreatedAt = DateTime.Now
        });

        RecordSaleItems(cart, transNo, discount, total, customerPhone, cashier, "Portefeuille");
        Notify();

        return new CheckoutResult(true, "Paiement portefeuille effectué.", transNo, subTotal, discountAmount, total, total, 0, "Portefeuille (Wallet)");
    }

    private void RecordSaleItems(List<CartItem> cart, string transNo, double discountPercent, double totalAmount, string? customerPhone, string cashier, string paymentMode)
    {
        int nextSoldId = NextId(SoldItems.Select(s => s.Id));
        foreach (var item in cart)
        {
            double itemDiscount = item.Total * (discountPercent / 100.0);
            double netTotal = item.Total - itemDiscount;
            SoldItems.Add(new SoldItem
            {
                Id = nextSoldId++,
                TransNo = transNo,
                ProductCode = item.ProductCode,
                Description = item.ProductDescription,
                CostPrice = item.CostPrice,
                Price = item.Price,
                Qty = item.Qty,
                Discount = itemDiscount,
                Total = netTotal,
                SoldDate = DateTime.Now,
                Cashier = string.IsNullOrWhiteSpace(cashier) ? "Admin" : cashier,
                PaymentMode = paymentMode
            });

            var prod = Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
            if (prod != null)
            {
                prod.Qty = Math.Max(0, prod.Qty - item.Qty);
            }
        }

        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == customerPhone);
            if (wallet != null)
            {
                wallet.TotalSpent += totalAmount;
                wallet.LoyaltyPoints += (int)(totalAmount / 1000);
            }
        }
    }

    public OperationResult SaveProduct(Product product, bool isNew, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut modifier ou ajouter des produits.");
        if (string.IsNullOrWhiteSpace(product.ProductCode) || string.IsNullOrWhiteSpace(product.Description))
            return new OperationResult(false, "Le code produit et la description sont obligatoires.");

        var vendor = Vendors.FirstOrDefault(v => v.Id == product.VendorId);
        product.VendorName = vendor?.VendorName ?? product.VendorName ?? string.Empty;
        product.Barcode ??= string.Empty;
        product.BrandName ??= string.Empty;
        product.CategoryName ??= string.Empty;

        if (isNew)
        {
            if (Products.Any(p => p.ProductCode == product.ProductCode))
                return new OperationResult(false, "Ce code produit existe déjà dans la base !");
            Products.Add(product);
        }
        else
        {
            var existing = Products.FirstOrDefault(p => p.ProductCode == product.ProductCode);
            if (existing == null) return new OperationResult(false, "Produit introuvable.");
            existing.Description = product.Description;
            existing.CostPrice = product.CostPrice;
            existing.Price = product.Price;
            existing.Qty = Math.Max(0, product.Qty);
            existing.Reorder = product.Reorder;
            existing.Barcode = product.Barcode;
            existing.BrandName = product.BrandName;
            existing.CategoryName = product.CategoryName;
            existing.VendorId = product.VendorId;
            existing.VendorName = product.VendorName;
        }

        Notify();
        return new OperationResult(true, "Produit enregistré avec succès.");
    }

    public OperationResult DeleteProduct(string productCode, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer un produit.");
        var p = Products.FirstOrDefault(x => x.ProductCode == productCode);
        if (p == null) return new OperationResult(false, "Produit introuvable.");
        Products.Remove(p);
        Notify();
        return new OperationResult(true, "Produit supprimé.");
    }

    public OperationResult AdjustStock(string productCode, string adjustmentType, int qty, string justification, string user, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut ajuster les stocks.");
        if (qty < 0 || (qty == 0 && adjustmentType != "Ajustement (Inventaire)"))
            return new OperationResult(false, "La quantité doit être valide et non négative.");
        if (string.IsNullOrWhiteSpace(justification))
            return new OperationResult(false, "Une justification est requise pour cette opération.");

        var product = Products.FirstOrDefault(p => p.ProductCode == productCode);
        if (product == null) return new OperationResult(false, "Produit introuvable.");

        int diff;
        if (adjustmentType == "Entrée")
        {
            diff = qty;
            product.Qty += diff;
        }
        else if (adjustmentType == "Sortie")
        {
            if (product.Qty < qty) return new OperationResult(false, "Stock insuffisant pour cette sortie.");
            diff = -qty;
            product.Qty -= qty;
        }
        else
        {
            diff = qty - product.Qty;
            product.Qty = qty;
        }

        Stocks.Add(new Stock
        {
            Id = NextId(Stocks.Select(s => s.Id)),
            RefNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ProductCode = product.ProductCode,
            Description = product.Description,
            Qty = diff,
            StockInDate = DateTime.Now,
            StockInBy = string.IsNullOrWhiteSpace(user) ? "admin" : user,
            Status = $"{adjustmentType} - {justification}",
            VendorId = product.VendorId,
            VendorName = product.VendorName ?? string.Empty
        });

        Notify();
        return new OperationResult(true, "Le stock a été ajusté avec succès.");
    }

    public OperationResult SaveBrand(int? id, string name, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(name)) return new OperationResult(false, "Le nom de la marque est obligatoire.");

        if (id.HasValue)
        {
            var existing = Brands.FirstOrDefault(b => b.Id == id.Value);
            if (existing == null) return new OperationResult(false, "Marque introuvable.");
            if (Brands.Any(b => b.BrandName == name && b.Id != id.Value))
                return new OperationResult(false, "Cette marque existe déjà.");
            string oldName = existing.BrandName;
            existing.BrandName = name;
            foreach (var p in Products.Where(p => p.BrandName == oldName)) p.BrandName = name;
        }
        else
        {
            if (Brands.Any(b => b.BrandName == name))
                return new OperationResult(false, "Cette marque existe déjà.");
            Brands.Add(new Brand { Id = NextId(Brands.Select(b => b.Id)), BrandName = name });
        }

        Notify();
        return new OperationResult(true, "Marque enregistrée.");
    }

    public OperationResult DeleteBrand(int id, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        var b = Brands.FirstOrDefault(x => x.Id == id);
        if (b != null) Brands.Remove(b);
        Notify();
        return new OperationResult(true, "Marque supprimée.");
    }

    public OperationResult SaveCategory(int? id, string name, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(name)) return new OperationResult(false, "Le nom de la catégorie est obligatoire.");

        if (id.HasValue)
        {
            var existing = Categories.FirstOrDefault(c => c.Id == id.Value);
            if (existing == null) return new OperationResult(false, "Catégorie introuvable.");
            if (Categories.Any(c => c.CategoryName == name && c.Id != id.Value))
                return new OperationResult(false, "Cette catégorie existe déjà.");
            string oldName = existing.CategoryName;
            existing.CategoryName = name;
            foreach (var p in Products.Where(p => p.CategoryName == oldName)) p.CategoryName = name;
        }
        else
        {
            if (Categories.Any(c => c.CategoryName == name))
                return new OperationResult(false, "Cette catégorie existe déjà.");
            Categories.Add(new Category { Id = NextId(Categories.Select(c => c.Id)), CategoryName = name });
        }

        Notify();
        return new OperationResult(true, "Catégorie enregistrée.");
    }

    public OperationResult DeleteCategory(int id, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        var c = Categories.FirstOrDefault(x => x.Id == id);
        if (c != null) Categories.Remove(c);
        Notify();
        return new OperationResult(true, "Catégorie supprimée.");
    }

    public OperationResult SaveWallet(CustomerWallet wallet, double rechargeAmount, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(wallet.Phone))
            return new OperationResult(false, "Le numéro de téléphone est obligatoire.");
        if (!double.IsFinite(rechargeAmount) || rechargeAmount < 0)
            return new OperationResult(false, "Le montant est invalide ou négatif.");

        wallet.CustomerName ??= string.Empty;
        wallet.Email ??= string.Empty;

        if (isNew)
        {
            if (CustomerWallets.Any(w => w.Phone == wallet.Phone))
                return new OperationResult(false, "Ce portefeuille existe déjà.");
            wallet.Balance = rechargeAmount;
            CustomerWallets.Add(wallet);
            if (rechargeAmount > 0)
            {
                WalletTransactions.Add(new WalletTransaction
                {
                    Id = NextId(WalletTransactions.Select(w => w.Id)),
                    Phone = wallet.Phone,
                    TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Type = "Credit",
                    Amount = rechargeAmount,
                    CreatedAt = DateTime.Now
                });
            }
            Notify();
            return new OperationResult(true, "Portefeuille créé avec succès.");
        }

        if (rechargeAmount <= 0)
            return new OperationResult(false, "Le montant de la recharge doit être supérieur à zéro.");

        var existing = CustomerWallets.FirstOrDefault(w => w.Phone == wallet.Phone);
        if (existing == null) return new OperationResult(false, "Portefeuille introuvable.");

        existing.Balance += rechargeAmount;
        existing.CustomerName = wallet.CustomerName;
        existing.Email = wallet.Email;
        WalletTransactions.Add(new WalletTransaction
        {
            Id = NextId(WalletTransactions.Select(w => w.Id)),
            Phone = existing.Phone,
            TransNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
            Type = "Credit",
            Amount = rechargeAmount,
            CreatedAt = DateTime.Now
        });

        Notify();
        return new OperationResult(true, $"Recharge de {rechargeAmount:N0} FCFA effectuée sur {existing.Phone}.");
    }

    public OperationResult DeleteWallet(string phone, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer un portefeuille.");
        var w = CustomerWallets.FirstOrDefault(x => x.Phone == phone);
        if (w == null) return new OperationResult(false, "Portefeuille introuvable.");
        CustomerWallets.Remove(w);
        WalletTransactions.RemoveAll(t => t.Phone == phone);
        Notify();
        return new OperationResult(true, "Portefeuille supprimé.");
    }

    public OperationResult SaveVendor(Vendor vendor, bool isNew, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut modifier ou ajouter des fournisseurs.");
        if (string.IsNullOrWhiteSpace(vendor.VendorName))
            return new OperationResult(false, "Le nom du fournisseur est obligatoire.");

        vendor.ContactPerson ??= string.Empty;
        vendor.Phone ??= string.Empty;
        vendor.Email ??= string.Empty;
        vendor.Address ??= string.Empty;

        if (isNew)
        {
            vendor.Id = NextId(Vendors.Select(v => v.Id));
            Vendors.Add(vendor);
        }
        else
        {
            var existing = Vendors.FirstOrDefault(v => v.Id == vendor.Id);
            if (existing == null) return new OperationResult(false, "Fournisseur introuvable.");
            existing.VendorName = vendor.VendorName;
            existing.ContactPerson = vendor.ContactPerson;
            existing.Phone = vendor.Phone;
            existing.Email = vendor.Email;
            existing.Address = vendor.Address;
        }

        Notify();
        return new OperationResult(true, "Fournisseur enregistré.");
    }

    public OperationResult DeleteVendor(int vendorId, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer un fournisseur.");
        if (Products.Any(p => p.VendorId == vendorId))
            return new OperationResult(false, "Impossible de supprimer ce fournisseur car il est associé à des produits.");
        var v = Vendors.FirstOrDefault(x => x.Id == vendorId);
        if (v != null) Vendors.Remove(v);
        Notify();
        return new OperationResult(true, "Fournisseur supprimé.");
    }

    public OperationResult CancelSale(int soldItemId, string cancelledBy, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut annuler une transaction.");
        var item = SoldItems.FirstOrDefault(s => s.Id == soldItemId);
        if (item == null) return new OperationResult(false, "Transaction introuvable.");

        var prod = Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
        if (prod != null) prod.Qty += item.Qty;

        CancelledOrders.Add(new CancelledOrder
        {
            Id = NextId(CancelledOrders.Select(c => c.Id)),
            TransNo = item.TransNo,
            ProductCode = item.ProductCode,
            Description = item.Description,
            Price = item.Price,
            Qty = item.Qty,
            Total = item.Total,
            SoldDate = item.SoldDate,
            VoidBy = cancelledBy,
            CancelledBy = cancelledBy,
            Reason = "Annulation demandée par l'administrateur.",
            Action = "Annuler"
        });

        SoldItems.Remove(item);
        Notify();
        return new OperationResult(true, "Transaction annulée et stock restauré avec succès.");
    }

    public OperationResult SaveExpense(Expense expense, bool isNew, string addedBy, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut modifier ou ajouter des dépenses.");
        if (string.IsNullOrWhiteSpace(expense.Category) || !double.IsFinite(expense.Amount) || expense.Amount <= 0)
            return new OperationResult(false, "Veuillez renseigner une catégorie et un montant supérieur à zéro.");

        expense.Description ??= string.Empty;
        if (isNew)
        {
            expense.Id = NextId(Expenses.Select(e => e.Id));
            expense.AddedBy = string.IsNullOrWhiteSpace(addedBy) ? "Admin" : addedBy;
            Expenses.Add(expense);
        }
        else
        {
            var existing = Expenses.FirstOrDefault(e => e.Id == expense.Id);
            if (existing == null) return new OperationResult(false, "Dépense introuvable.");
            existing.Date = expense.Date;
            existing.Category = expense.Category;
            existing.Description = expense.Description;
            existing.Amount = expense.Amount;
        }

        Notify();
        return new OperationResult(true, "Dépense enregistrée.");
    }

    public OperationResult DeleteExpense(int id, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer une dépense.");
        var e = Expenses.FirstOrDefault(x => x.Id == id);
        if (e != null) Expenses.Remove(e);
        Notify();
        return new OperationResult(true, "Dépense supprimée.");
    }

    public OperationResult SaveUser(User user, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            return new OperationResult(false, "Identifiant et mot de passe obligatoires.");

        var existing = Users.FirstOrDefault(u => u.Username == user.Username);
        if (existing != null)
        {
            existing.Name = string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name;
            existing.Role = user.Role;
            existing.Password = user.Password;
            existing.IsActive = user.IsActive;
        }
        else
        {
            Users.Add(new User
            {
                Username = user.Username,
                Name = string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name,
                Role = string.IsNullOrWhiteSpace(user.Role) ? "Cashier" : user.Role,
                Password = user.Password,
                IsActive = user.IsActive
            });
        }

        Notify();
        return new OperationResult(true, "Utilisateur enregistré avec succès !");
    }

    public OperationResult SaveStore(string storeName, string address, string momoCode, string orangeCode, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(storeName)) return new OperationResult(false, "Le nom de la boutique est obligatoire.");
        Store.StoreName = storeName;
        Store.Address = address ?? string.Empty;
        Store.MomoCode = momoCode ?? string.Empty;
        Store.OrangeCode = orangeCode ?? string.Empty;
        Notify();
        return new OperationResult(true, "Paramètres système enregistrés !");
    }

    public OperationResult ChangePassword(string username, string oldPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            return new OperationResult(false, "Veuillez remplir tous les champs de mot de passe.");
        if (newPassword != confirmPassword)
            return new OperationResult(false, "Le nouveau mot de passe et la confirmation ne correspondent pas.");

        var user = Users.FirstOrDefault(u => u.Username == username && u.Password == oldPassword);
        if (user == null)
            return new OperationResult(false, "L'ancien mot de passe est incorrect.");

        user.Password = newPassword;
        Notify();
        return new OperationResult(true, "Votre mot de passe a été modifié avec succès !");
    }

    public DashboardMetrics GetDashboardMetrics()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        var m = new DashboardMetrics
        {
            TodaySales = SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => s.Total),
            TotalSalesAllTime = SoldItems.Sum(s => s.Total),
            YesterdaySales = SoldItems.Where(s => s.SoldDate >= yesterday && s.SoldDate < today).Sum(s => s.Total),
            TotalProducts = Products.Count,
            TotalStockQuantity = Products.Sum(p => p.Qty),
            LowStockItems = Products.Count(p => p.Qty <= p.Reorder),
            CriticalStockList = Products.Where(p => p.Qty <= p.Reorder).ToList(),
            TotalCustomers = CustomerWallets.Count,
            TotalLoyaltyPoints = CustomerWallets.Sum(w => w.LoyaltyPoints),
            TotalWalletsBalance = CustomerWallets.Sum(w => w.Balance),
            TotalItemsSoldToday = SoldItems.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => s.Qty),
            TotalItemsSoldAllTime = SoldItems.Sum(s => s.Qty),
            TotalCancelledAmount = CancelledOrders.Sum(c => c.Total),
            TotalDiscountAmount = SoldItems.Sum(s => s.Discount),
            TotalUsers = Users.Count,
            AdminCount = Users.Count(u => u.Role == "Admin"),
            CashierCount = Users.Count(u => u.Role == "Cashier"),
            TotalVendors = Vendors.Count,
            TotalCOGS = SoldItems.Sum(s => s.CostPrice * s.Qty),
            TotalExpenses = Expenses.Sum(e => e.Amount),
            Last7DaysLabels = new string[7],
            Last7DaysSales = new double[7],
            Last7DaysStockIn = new double[7],
            Last7DaysStockOut = new double[7],
            Last7DaysDiscounts = new double[7],
            Last7DaysCancelled = new double[7]
        };

        m.NetProfit = m.TotalSalesAllTime - m.TotalCOGS - m.TotalExpenses;

        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            int idx = 6 - i;
            m.Last7DaysLabels[idx] = d.ToString("dd MMM");
            m.Last7DaysSales[idx] = SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Total);
            m.Last7DaysStockIn[idx] = Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty > 0).Sum(s => s.Qty);
            int manualOut = Math.Abs(Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty < 0).Sum(s => s.Qty));
            int salesOut = SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Qty);
            m.Last7DaysStockOut[idx] = manualOut + salesOut;
            m.Last7DaysDiscounts[idx] = SoldItems.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Discount);
            m.Last7DaysCancelled[idx] = CancelledOrders.Where(c => c.SoldDate >= d && c.SoldDate < nextD).Sum(c => c.Total);
        }

        m.TopProductsStock = Products.OrderByDescending(p => p.Qty).Take(5).Select(p => (p.ProductCode ?? "", (double)p.Qty)).ToList();
        m.TopCustomers = CustomerWallets.OrderByDescending(w => w.TotalSpent).Take(5).Select(w => (string.IsNullOrEmpty(w.CustomerName) ? w.Phone : w.CustomerName, w.TotalSpent)).ToList();
        m.TopSoldItems = SoldItems.GroupBy(s => s.Description ?? "").Select(g => (g.Key.Length > 14 ? g.Key[..14] + "…" : g.Key, (double)g.Sum(x => x.Qty))).OrderByDescending(x => x.Item2).Take(5).ToList();
        m.PaymentModes = SoldItems.GroupBy(s => s.PaymentMode ?? "Espèces").Select(g => (g.Key, g.Sum(x => x.Total))).ToList();
        m.UsersByRole = Users.GroupBy(u => u.Role ?? "Autre").Select(g => (g.Key, (double)g.Count())).ToList();
        m.VendorsByProducts = Products.Where(p => p.VendorId > 0).GroupBy(p => p.VendorName ?? "Inconnu").Select(g => (g.Key, (double)g.Count())).ToList();

        return m;
    }

    private static int NextId(IEnumerable<int> ids) => ids.Any() ? ids.Max() + 1 : 1;
}
