using System.Globalization;
using System.Text;
using System.Text.Json;
using CamaroesPOS.Application.Security;
using CamaroesPOS.Application.Services;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public enum DashboardPeriod
{
    Today,
    Last7Days,
    Last30Days,
    AllTime
}

public record OperationResult(bool Success, string Message);

public record CheckoutResult(
    bool Success,
    string Message,
    string TransNo = "",
    DateTime Timestamp = default,
    decimal SubTotalHT = 0m,
    decimal TaxAmount = 0m,
    decimal TaxRatePercent = PosCalculator.DefaultCameroonVatRate,
    decimal SubTotal = 0m,
    decimal DiscountAmount = 0m,
    decimal TotalAmount = 0m,
    decimal ReceivedAmount = 0m,
    decimal ChangeAmount = 0m,
    string PaymentMode = "",
    string PaymentReference = "");

public record HeldCart(
    string Id,
    string Label,
    DateTime HeldAt,
    List<CartItem> Items,
    decimal DiscountPercent,
    string CustomerPhone);

public record SalesTicketSummary(
    string TransNo,
    DateTime SoldDate,
    string Cashier,
    string PaymentMode,
    string PaymentReference,
    string CustomerPhone,
    string Status,
    int TotalQty,
    decimal SubTotalHT,
    decimal TaxAmount,
    decimal Discount,
    decimal TotalTTC,
    List<SoldItem> Lines);

public class DashboardMetrics
{
    public DashboardPeriod Period { get; set; } = DashboardPeriod.Last7Days;
    public decimal TodaySales { get; set; }
    public decimal PeriodSales { get; set; }
    public decimal TotalSalesAllTime { get; set; }
    public decimal YesterdaySales { get; set; }
    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
    public int LowStockItems { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalLoyaltyPoints { get; set; }
    public decimal TotalWalletsBalance { get; set; }
    public int TotalItemsSoldToday { get; set; }
    public int PeriodItemsSold { get; set; }
    public int TotalItemsSoldAllTime { get; set; }
    public decimal TotalCancelledAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxCollected { get; set; }
    public int TotalUsers { get; set; }
    public int AdminCount { get; set; }
    public int CashierCount { get; set; }
    public int TotalVendors { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }

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
    private static readonly JsonSerializerOptions PrettyJsonOptions = new() { WriteIndented = true };

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
    public List<CashShift> CashShifts { get; private set; } = new();
    public List<AuditEntry> AuditLogs { get; private set; } = new();
    public List<HeldCart> HeldCarts { get; private set; } = new();
    public Store Store { get; private set; } = new();

    public bool IsInitialized { get; private set; }
    public CashShift? ActiveShift => CashShifts.LastOrDefault(s => s.IsOpen);
    public event Action? OnChanged;

    private void Notify() => OnChanged?.Invoke();

    public static decimal ClampDiscount(decimal discountPercent) => PosCalculator.ClampDiscount(discountPercent);
    public static bool TryParseDecimalInput(string? raw, out decimal value) => PosCalculator.TryParseDecimalInput(raw, out value);

    public void SeedDemoData()
    {
        var today = DateTime.Today;

        Store = new Store
        {
            Id = 1,
            StoreName = "CAMAROES POS",
            Address = "Boulevard de la Liberté, Akwa — Douala, Cameroun",
            Phone = "+237 690 12 34 56",
            Niu = "M012612345678A",
            Rccm = "RC/DLA/2026/B/0421",
            TaxRate = PosCalculator.DefaultCameroonVatRate,
            Currency = "FCFA",
            Logo = Array.Empty<byte>(),
            MomoCode = "MTN-237-8841",
            OrangeCode = "OM-237-4492"
        };

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
            new() { ProductCode = "P001", Barcode = "12345", Description = "Laptop Gamer", BrandId = 1, BrandName = "Asus", CategoryId = 1, CategoryName = "Informatique", CostPrice = 1150000m, Price = 1500000m, Qty = 10, Reorder = 4, VendorId = 1, VendorName = "TechDistrib Douala", IsTaxable = true },
            new() { ProductCode = "P002", Barcode = "12346", Description = "Souris Sans Fil", BrandId = 2, BrandName = "Logitech", CategoryId = 2, CategoryName = "Accessoires", CostPrice = 9500m, Price = 15000m, Qty = 50, Reorder = 10, VendorId = 1, VendorName = "TechDistrib Douala", IsTaxable = true },
            new() { ProductCode = "P003", Barcode = "12347", Description = "Clavier Mécanique", BrandId = 3, BrandName = "Corsair", CategoryId = 2, CategoryName = "Accessoires", CostPrice = 30000m, Price = 45000m, Qty = 15, Reorder = 5, VendorId = 1, VendorName = "TechDistrib Douala", IsTaxable = true },
            new() { ProductCode = "P004", Barcode = "12348", Description = "Écran 27 pouces", BrandId = 4, BrandName = "Samsung", CategoryId = 1, CategoryName = "Informatique", CostPrice = 90000m, Price = 120000m, Qty = 5, Reorder = 5, VendorId = 2, VendorName = "AudioVision SARL", IsTaxable = true },
            new() { ProductCode = "P005", Barcode = "12349", Description = "Casque Audio", BrandId = 5, BrandName = "Sony", CategoryId = 3, CategoryName = "Audio", CostPrice = 22000m, Price = 35000m, Qty = 20, Reorder = 6, VendorId = 2, VendorName = "AudioVision SARL", IsTaxable = true },
            new() { ProductCode = "P006", Barcode = "12350", Description = "SSD NVMe 1 To", BrandId = 4, BrandName = "Samsung", CategoryId = 1, CategoryName = "Informatique", CostPrice = 48000m, Price = 68000m, Qty = 3, Reorder = 5, VendorId = 1, VendorName = "TechDistrib Douala", IsTaxable = true }
        };

        CustomerWallets = new List<CustomerWallet>
        {
            new() { Phone = "0601020304", CustomerName = "Paul Kamga", Email = "paul.kamga@mail.cm", Balance = 150000m, LoyaltyPoints = 240, TotalSpent = 240000m },
            new() { Phone = "0611223344", CustomerName = "Amina Bello", Email = "amina.bello@mail.cm", Balance = 50000m, LoyaltyPoints = 155, TotalSpent = 155000m },
            new() { Phone = "0655443322", CustomerName = "Eric Fotso", Email = "eric.fotso@mail.cm", Balance = 85000m, LoyaltyPoints = 90, TotalSpent = 90000m }
        };

        // Passwords are ALWAYS stored hashed with PBKDF2-SHA256 (never plaintext)
        Users = new List<User>
        {
            new() { Username = "admin", Password = PasswordHasher.Hash("123"), Role = "Admin", Name = "Administrateur", IsActive = true },
            new() { Username = "caissier1", Password = PasswordHasher.Hash("123"), Role = "Cashier", Name = "Caissier 1", IsActive = true }
        };

        SoldItems = new List<SoldItem>
        {
            BuildSeedSale(1, today.AddDays(-5).AddHours(10), "A1B2", "P002", "Souris Sans Fil", 9500m, 15000m, 3, 0m, "Caissier 1", "Espèces", "", ""),
            BuildSeedSale(2, today.AddDays(-4).AddHours(11), "C3D4", "P004", "Écran 27 pouces", 90000m, 120000m, 1, 5m, "Administrateur", "Portefeuille", "WLT-2401", "0601020304"),
            BuildSeedSale(3, today.AddDays(-3).AddHours(14), "E5F6", "P003", "Clavier Mécanique", 30000m, 45000m, 2, 5m, "Caissier 1", "MTN MoMo", "MOMO-8841-901", "677112233"),
            BuildSeedSale(4, today.AddDays(-2).AddHours(16), "G7H8", "P005", "Casque Audio", 22000m, 35000m, 3, 0m, "Caissier 1", "Orange Money", "OM-4492-312", "699445566"),
            BuildSeedSale(5, today.AddDays(-1).AddHours(10), "J9K2", "P006", "SSD NVMe 1 To", 48000m, 68000m, 2, 5m, "Administrateur", "Espèces", "", ""),
            BuildSeedSale(6, today.AddHours(9), "M3N4", "P003", "Clavier Mécanique", 30000m, 45000m, 2, 0m, "Caissier 1", "Espèces", "", ""),
            BuildSeedSale(7, today.AddHours(11), "P5Q6", "P005", "Casque Audio", 22000m, 35000m, 2, 5m, "Caissier 1", "Portefeuille", "WLT-2502", "0611223344")
        };

        Stocks = new List<Stock>
        {
            new() { Id = 1, RefNo = $"STK-{today.AddDays(-5):yyyyMMdd}-0001", ProductCode = "P001", Description = "Laptop Gamer", Qty = 10, StockInDate = today.AddDays(-5), StockInBy = "admin", Status = "Entrée - Stock initial", VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { Id = 2, RefNo = $"STK-{today.AddDays(-3):yyyyMMdd}-0002", ProductCode = "P002", Description = "Souris Sans Fil", Qty = 25, StockInDate = today.AddDays(-3), StockInBy = "admin", Status = "Entrée - Réapprovisionnement", VendorId = 1, VendorName = "TechDistrib Douala" },
            new() { Id = 3, RefNo = $"STK-{today.AddDays(-1):yyyyMMdd}-0003", ProductCode = "P005", Description = "Casque Audio", Qty = 12, StockInDate = today.AddDays(-1), StockInBy = "admin", Status = "Entrée - Livraison fournisseur", VendorId = 2, VendorName = "AudioVision SARL" }
        };

        WalletTransactions = new List<WalletTransaction>
        {
            new() { Id = 1, Phone = "0601020304", TransNo = $"WLT-{today.AddDays(-4):yyyyMMdd}-0001", Type = "Credit", Amount = 200000m, CreatedAt = today.AddDays(-4) },
            new() { Id = 2, Phone = "0611223344", TransNo = $"TX-{today:yyyyMMdd}-110000-P5Q6", Type = "Debit", Amount = 66500m, CreatedAt = today.AddHours(11) }
        };

        CancelledOrders = new List<CancelledOrder>
        {
            new() { Id = 1, TransNo = $"TX-{today.AddDays(-2):yyyyMMdd}-083000-Z1Z2", ProductCode = "P002", Description = "Souris Sans Fil", Price = 15000m, Qty = 1, Total = 15000m, SoldDate = today.AddDays(-2), CancelledAt = today.AddDays(-2).AddMinutes(15), VoidBy = "Administrateur", CancelledBy = "Administrateur", Reason = "Erreur de caisse", Action = "Remboursement", WalletRefunded = false }
        };

        Expenses = new List<Expense>
        {
            new() { Id = 1, Date = today.AddDays(-4), Category = "Électricité", Description = "Facture ENEO Boutique", Amount = 45000m, AddedBy = "Administrateur" },
            new() { Id = 2, Date = today.AddDays(-2), Category = "Transport", Description = "Livraison fournisseurs Akwa", Amount = 18000m, AddedBy = "Administrateur" }
        };

        CashShifts = new List<CashShift>
        {
            new()
            {
                Id = 1,
                OpenedBy = "Caissier 1",
                OpenedAt = today.AddDays(-1).AddHours(8),
                OpeningFloat = 50000m,
                ClosedAt = today.AddDays(-1).AddHours(19),
                ClosedBy = "Administrateur",
                TotalSales = 129200m,
                CashSales = 129200m,
                MobileMoneySales = 0m,
                WalletSales = 0m,
                ExpectedCash = 179200m,
                CountedCash = 179200m,
                Discrepancy = 0m,
                TransactionCount = 1,
                Notes = "Clôture journalière conforme"
            },
            new()
            {
                Id = 2,
                OpenedBy = "Caissier 1",
                OpenedAt = today.AddHours(8),
                OpeningFloat = 50000m,
                TotalSales = 156500m,
                CashSales = 90000m,
                MobileMoneySales = 0m,
                WalletSales = 66500m,
                ExpectedCash = 140000m,
                TransactionCount = 2,
                Notes = "Session en cours"
            }
        };

        AuditLogs = new List<AuditEntry>
        {
            new() { Id = 1, Timestamp = today.AddHours(8), Actor = "Caissier 1", Role = "Cashier", Action = "SHIFT_OPEN", Target = "Shift #2", Details = "Fond de caisse initial : 50 000 FCFA", Severity = "Info" },
            new() { Id = 2, Timestamp = today.AddDays(-2).AddMinutes(15), Actor = "Administrateur", Role = "Admin", Action = "SALE_REFUND", Target = $"TX-{today.AddDays(-2):yyyyMMdd}-083000-Z1Z2", Details = "Remboursement 15 000 FCFA — Erreur de caisse", Severity = "Warning" }
        };

        HeldCarts = new List<HeldCart>();
        IsInitialized = true;
        Notify();
    }

    private SoldItem BuildSeedSale(int id, DateTime date, string suffix, string code, string desc, decimal cost, decimal price, int qty, decimal discountPct, string cashier, string mode, string payRef, string phone)
    {
        var temp = new CartItem { ProductCode = code, ProductDescription = desc, CostPrice = cost, Price = price, Qty = qty, IsTaxable = true };
        var (disc, netTtc, ht, tax) = PosCalculator.CalculateLineBreakdown(temp, discountPct, Store.TaxRate);
        return new SoldItem
        {
            Id = id,
            TransNo = $"TX-{date:yyyyMMdd-HHmmss}-{suffix}",
            ProductCode = code,
            Description = desc,
            CostPrice = cost,
            Price = price,
            Qty = qty,
            Discount = disc,
            SubTotalHT = ht,
            TaxAmount = tax,
            Total = netTtc,
            SoldDate = date,
            Cashier = cashier,
            PaymentMode = mode,
            PaymentReference = payRef,
            CustomerPhone = phone,
            Status = "Completed"
        };
    }

    public string ExportSnapshotJson(bool pretty = false)
    {
        var snapshot = new PosStoreSnapshot
        {
            SchemaVersion = 2,
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
            CashShifts = CashShifts,
            AuditLogs = AuditLogs,
            Store = Store
        };
        return JsonSerializer.Serialize(snapshot, pretty ? PrettyJsonOptions : JsonOptions);
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

            // Ensure any imported legacy plaintext passwords are upgraded to PBKDF2 hashes immediately
            foreach (var u in snapshot.Users)
            {
                if (!string.IsNullOrEmpty(u.Password) && !PasswordHasher.IsHashed(u.Password))
                {
                    u.Password = PasswordHasher.Hash(u.Password);
                }
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
            CashShifts = snapshot.CashShifts ?? new();
            AuditLogs = snapshot.AuditLogs ?? new();
            Store = snapshot.Store ?? new Store();
            IsInitialized = true;
            Notify();
        }
        catch
        {
            SeedDemoData();
        }
    }

    public OperationResult ImportBackupJson(string? json, string actor, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(json)) return new OperationResult(false, "Fichier JSON vide.");

        try
        {
            var snapshot = JsonSerializer.Deserialize<PosStoreSnapshot>(json, JsonOptions);
            if (snapshot == null || snapshot.Users == null || snapshot.Users.Count == 0)
                return new OperationResult(false, "Format de sauvegarde invalide (utilisateurs manquants).");

            ImportSnapshotJson(json);
            RecordAudit(actor, "Admin", "BACKUP_IMPORT", "Store", "Restauration complète depuis sauvegarde JSON", "Warning");
            return new OperationResult(true, "Sauvegarde restaurée avec succès.");
        }
        catch (Exception ex)
        {
            return new OperationResult(false, $"Erreur de lecture JSON : {ex.Message}");
        }
    }

    public User? Authenticate(string username, string password)
    {
        var user = Users.FirstOrDefault(u => u.Username == username && u.IsActive);
        if (user == null || !PasswordHasher.Verify(password, user.Password))
            return null;

        if (!PasswordHasher.IsHashed(user.Password))
        {
            user.Password = PasswordHasher.Hash(password);
        }

        RecordAudit(user.Name, user.Role, "AUTH_LOGIN", user.Username, "Connexion réussie", "Info");
        return user;
    }

    public OperationResult ValidateCartStock(IReadOnlyList<CartItem>? cart)
    {
        if (cart == null || cart.Count == 0)
            return new OperationResult(false, "Le panier est vide.");

        foreach (var group in cart.GroupBy(c => c.ProductCode))
        {
            int requestedQty = group.Sum(x => x.Qty);
            if (requestedQty <= 0)
                return new OperationResult(false, $"Quantité invalide pour l'article {group.Key}.");

            var product = Products.FirstOrDefault(p => p.ProductCode == group.Key);
            if (product == null)
                return new OperationResult(false, $"Produit introuvable ({group.Key}).");

            if (requestedQty > product.Qty)
                return new OperationResult(false, $"Stock insuffisant pour « {product.Description} » (Demandé : {requestedQty}, Disponible : {product.Qty}).");
        }

        return new OperationResult(true, "OK");
    }

    public CheckoutResult CheckoutCash(List<CartItem> cart, decimal discountPercent, decimal receivedAmount, string? customerPhone, string cashier)
    {
        var stockCheck = ValidateCartStock(cart);
        if (!stockCheck.Success)
            return new CheckoutResult(false, stockCheck.Message);

        var totals = PosCalculator.CalculateTotals(cart, discountPercent, Store.TaxRate);
        if (totals.TotalTTC <= 0m)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        if (receivedAmount < totals.TotalTTC)
            return new CheckoutResult(false, "Le montant reçu est insuffisant ou invalide.");

        string transNo = TransactionNumberGenerator.NextTransactionNo();
        DateTime now = DateTime.Now;
        RecordSaleItems(cart, transNo, now, totals.DiscountPercent, totals.TotalTTC, customerPhone, cashier, "Espèces", "");
        UpdateActiveShift(totals.TotalTTC, cashDelta: totals.TotalTTC, momoDelta: 0m, walletDelta: 0m);

        if (totals.DiscountPercent >= 15m)
            RecordAudit(cashier, "POS", "HIGH_DISCOUNT", transNo, $"Remise élevée de {totals.DiscountPercent:0.##}% ({totals.DiscountAmount:N0} FCFA)", "Warning");

        Notify();
        return new CheckoutResult(
            true,
            "Vente enregistrée.",
            transNo,
            now,
            totals.TotalHT,
            totals.TaxAmount,
            totals.TaxRatePercent,
            totals.SubTotalTTC,
            totals.DiscountAmount,
            totals.TotalTTC,
            receivedAmount,
            receivedAmount - totals.TotalTTC,
            "Espèces",
            "");
    }

    public CheckoutResult CheckoutWallet(List<CartItem> cart, decimal discountPercent, string customerPhone, string cashier)
    {
        var stockCheck = ValidateCartStock(cart);
        if (!stockCheck.Success)
            return new CheckoutResult(false, stockCheck.Message);

        if (string.IsNullOrWhiteSpace(customerPhone))
            return new CheckoutResult(false, "Veuillez entrer le numéro de téléphone du client pour payer par portefeuille.");

        var totals = PosCalculator.CalculateTotals(cart, discountPercent, Store.TaxRate);
        if (totals.TotalTTC <= 0m)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == customerPhone);
        if (wallet == null)
            return new CheckoutResult(false, "Ce portefeuille client n'existe pas.");

        if (wallet.Balance < totals.TotalTTC)
            return new CheckoutResult(false, $"Solde insuffisant dans le portefeuille (Solde : {wallet.Balance:N0} FCFA).");

        string transNo = TransactionNumberGenerator.NextTransactionNo();
        DateTime now = DateTime.Now;
        wallet.Balance -= totals.TotalTTC;
        WalletTransactions.Add(new WalletTransaction
        {
            Id = NextId(WalletTransactions.Select(w => w.Id)),
            Phone = wallet.Phone,
            TransNo = transNo,
            Type = "Debit",
            Amount = totals.TotalTTC,
            CreatedAt = now
        });

        RecordSaleItems(cart, transNo, now, totals.DiscountPercent, totals.TotalTTC, wallet.Phone, cashier, "Portefeuille", $"WLT-{wallet.Phone[^Math.Min(4, wallet.Phone.Length)..]}");
        UpdateActiveShift(totals.TotalTTC, cashDelta: 0m, momoDelta: 0m, walletDelta: totals.TotalTTC);

        Notify();
        return new CheckoutResult(
            true,
            "Paiement portefeuille effectué.",
            transNo,
            now,
            totals.TotalHT,
            totals.TaxAmount,
            totals.TaxRatePercent,
            totals.SubTotalTTC,
            totals.DiscountAmount,
            totals.TotalTTC,
            totals.TotalTTC,
            0m,
            "Portefeuille",
            $"WLT-{wallet.Phone[^Math.Min(4, wallet.Phone.Length)..]}");
    }

    public CheckoutResult CheckoutMobileMoney(List<CartItem> cart, decimal discountPercent, string provider, string payerPhone, string? loyaltyPhone, string cashier)
    {
        var stockCheck = ValidateCartStock(cart);
        if (!stockCheck.Success)
            return new CheckoutResult(false, stockCheck.Message);

        if (string.IsNullOrWhiteSpace(payerPhone) || payerPhone.Trim().Length < 8)
            return new CheckoutResult(false, "Veuillez saisir un numéro Mobile Money valide (min. 8 chiffres).");

        var totals = PosCalculator.CalculateTotals(cart, discountPercent, Store.TaxRate);
        if (totals.TotalTTC <= 0m)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        bool isOrange = provider.Contains("Orange", StringComparison.OrdinalIgnoreCase);
        string modeLabel = isOrange ? "Orange Money" : "MTN MoMo";
        string prefix = isOrange ? "OM" : "MOMO";
        string paymentRef = TransactionNumberGenerator.NextPaymentRef(prefix);
        string transNo = TransactionNumberGenerator.NextTransactionNo();
        DateTime now = DateTime.Now;

        string targetPhone = string.IsNullOrWhiteSpace(loyaltyPhone) ? payerPhone.Trim() : loyaltyPhone.Trim();
        RecordSaleItems(cart, transNo, now, totals.DiscountPercent, totals.TotalTTC, targetPhone, cashier, modeLabel, paymentRef);
        UpdateActiveShift(totals.TotalTTC, cashDelta: 0m, momoDelta: totals.TotalTTC, walletDelta: 0m);

        Notify();
        return new CheckoutResult(
            true,
            $"Paiement {modeLabel} confirmé ({paymentRef}).",
            transNo,
            now,
            totals.TotalHT,
            totals.TaxAmount,
            totals.TaxRatePercent,
            totals.SubTotalTTC,
            totals.DiscountAmount,
            totals.TotalTTC,
            totals.TotalTTC,
            0m,
            modeLabel,
            paymentRef);
    }

    public CheckoutResult CheckoutSplit(
        List<CartItem> cart,
        decimal discountPercent,
        decimal cashPortion,
        string secondaryMode,
        decimal secondaryPortion,
        string? customerPhone,
        string cashier)
    {
        var stockCheck = ValidateCartStock(cart);
        if (!stockCheck.Success)
            return new CheckoutResult(false, stockCheck.Message);

        var totals = PosCalculator.CalculateTotals(cart, discountPercent, Store.TaxRate);
        if (totals.TotalTTC <= 0m)
            return new CheckoutResult(false, "Le montant total doit être supérieur à zéro.");

        if (cashPortion < 0m || secondaryPortion <= 0m || (cashPortion + secondaryPortion) < totals.TotalTTC)
            return new CheckoutResult(false, "La somme des montants du paiement mixte est insuffisante.");

        string transNo = TransactionNumberGenerator.NextTransactionNo();
        DateTime now = DateTime.Now;
        decimal effectiveCash = totals.TotalTTC - secondaryPortion;
        if (effectiveCash < 0m) effectiveCash = 0m;
        decimal change = (cashPortion + secondaryPortion) - totals.TotalTTC;

        string paymentRef;
        decimal momoDelta = 0m;
        decimal walletDelta = 0m;

        if (secondaryMode == "Portefeuille")
        {
            if (string.IsNullOrWhiteSpace(customerPhone))
                return new CheckoutResult(false, "Numéro client requis pour la part Portefeuille.");
            var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == customerPhone.Trim());
            if (wallet == null)
                return new CheckoutResult(false, "Ce portefeuille client n'existe pas.");
            if (wallet.Balance < secondaryPortion)
                return new CheckoutResult(false, $"Solde portefeuille insuffisant ({wallet.Balance:N0} FCFA).");

            wallet.Balance -= secondaryPortion;
            WalletTransactions.Add(new WalletTransaction
            {
                Id = NextId(WalletTransactions.Select(w => w.Id)),
                Phone = wallet.Phone,
                TransNo = transNo,
                Type = "Debit",
                Amount = secondaryPortion,
                CreatedAt = now
            });
            paymentRef = $"SPLIT-WLT-{secondaryPortion:0}";
            walletDelta = secondaryPortion;
        }
        else
        {
            string prefix = secondaryMode.Contains("Orange", StringComparison.OrdinalIgnoreCase) ? "OM" : "MOMO";
            paymentRef = TransactionNumberGenerator.NextPaymentRef($"SPLIT-{prefix}");
            momoDelta = secondaryPortion;
        }

        string modeLabel = $"Mixte (Espèces + {secondaryMode})";
        RecordSaleItems(cart, transNo, now, totals.DiscountPercent, totals.TotalTTC, customerPhone?.Trim(), cashier, modeLabel, paymentRef);
        UpdateActiveShift(totals.TotalTTC, cashDelta: effectiveCash, momoDelta: momoDelta, walletDelta: walletDelta);

        Notify();
        return new CheckoutResult(
            true,
            "Paiement mixte enregistré.",
            transNo,
            now,
            totals.TotalHT,
            totals.TaxAmount,
            totals.TaxRatePercent,
            totals.SubTotalTTC,
            totals.DiscountAmount,
            totals.TotalTTC,
            cashPortion + secondaryPortion,
            change,
            modeLabel,
            paymentRef);
    }

    private void RecordSaleItems(
        List<CartItem> cart,
        string transNo,
        DateTime soldAt,
        decimal discountPercent,
        decimal totalAmount,
        string? customerPhone,
        string cashier,
        string paymentMode,
        string paymentReference)
    {
        int nextSoldId = NextId(SoldItems.Select(s => s.Id));
        foreach (var item in cart)
        {
            var (itemDiscount, netTotal, lineHt, lineTax) = PosCalculator.CalculateLineBreakdown(item, discountPercent, Store.TaxRate);
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
                SubTotalHT = lineHt,
                TaxAmount = lineTax,
                Total = netTotal,
                SoldDate = soldAt,
                Cashier = string.IsNullOrWhiteSpace(cashier) ? "Admin" : cashier,
                PaymentMode = paymentMode,
                PaymentReference = paymentReference,
                CustomerPhone = customerPhone ?? string.Empty,
                Status = "Completed"
            });

            var prod = Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
            if (prod != null)
            {
                prod.Qty -= item.Qty;
            }
        }

        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == customerPhone);
            if (wallet != null)
            {
                wallet.TotalSpent += totalAmount;
                wallet.LoyaltyPoints += PosCalculator.CalculateLoyaltyPoints(totalAmount);
            }
        }
    }

    private void UpdateActiveShift(decimal totalSale, decimal cashDelta, decimal momoDelta, decimal walletDelta)
    {
        var shift = ActiveShift;
        if (shift == null) return;
        shift.TotalSales += totalSale;
        shift.CashSales += cashDelta;
        shift.MobileMoneySales += momoDelta;
        shift.WalletSales += walletDelta;
        shift.ExpectedCash += cashDelta;
        shift.TransactionCount += 1;
    }

    public OperationResult OpenShift(string openedBy, decimal openingFloat)
    {
        if (ActiveShift != null)
            return new OperationResult(false, "Une session de caisse est déjà ouverte.");
        if (openingFloat < 0m)
            return new OperationResult(false, "Le fond de caisse ne peut pas être négatif.");

        var shift = new CashShift
        {
            Id = NextId(CashShifts.Select(s => s.Id)),
            OpenedBy = string.IsNullOrWhiteSpace(openedBy) ? "Admin" : openedBy,
            OpenedAt = DateTime.Now,
            OpeningFloat = openingFloat,
            ExpectedCash = openingFloat,
            Notes = "Session ouverte"
        };
        CashShifts.Add(shift);
        RecordAudit(shift.OpenedBy, "POS", "SHIFT_OPEN", $"Shift #{shift.Id}", $"Fond de caisse : {openingFloat:N0} FCFA", "Info");
        Notify();
        return new OperationResult(true, $"Session de caisse #{shift.Id} ouverte.");
    }

    public OperationResult CloseShift(string closedBy, decimal countedCash, string notes = "")
    {
        var shift = ActiveShift;
        if (shift == null)
            return new OperationResult(false, "Aucune session de caisse ouverte.");
        if (countedCash < 0m)
            return new OperationResult(false, "Le montant compté ne peut pas être négatif.");

        shift.ClosedAt = DateTime.Now;
        shift.ClosedBy = string.IsNullOrWhiteSpace(closedBy) ? "Admin" : closedBy;
        shift.CountedCash = countedCash;
        shift.Discrepancy = countedCash - shift.ExpectedCash;
        shift.Notes = string.IsNullOrWhiteSpace(notes) ? "Clôture Z effectuée" : notes.Trim();

        string sev = shift.Discrepancy == 0m ? "Info" : "Warning";
        RecordAudit(shift.ClosedBy, "POS", "SHIFT_CLOSE", $"Shift #{shift.Id}", $"Attendu : {shift.ExpectedCash:N0} | Compté : {countedCash:N0} | Écart : {shift.Discrepancy:N0} FCFA", sev);
        Notify();
        return new OperationResult(true, $"Rapport Z #{shift.Id} clôturé (Écart : {shift.Discrepancy:N0} FCFA).");
    }

    public void HoldCart(List<CartItem> items, decimal discountPercent, string customerPhone, string? label = null)
    {
        if (items == null || items.Count == 0) return;
        var clone = items.Select(c => new CartItem
        {
            ProductCode = c.ProductCode,
            ProductDescription = c.ProductDescription,
            CostPrice = c.CostPrice,
            Price = c.Price,
            Qty = c.Qty,
            Total = c.Total,
            IsTaxable = c.IsTaxable,
            Date = c.Date
        }).ToList();

        string cartLabel = string.IsNullOrWhiteSpace(label)
            ? $"Ticket #{HeldCarts.Count + 1} ({clone.Sum(x => x.Qty)} art.)"
            : label.Trim();

        HeldCarts.Add(new HeldCart(Guid.NewGuid().ToString("N")[..8], cartLabel, DateTime.Now, clone, discountPercent, customerPhone ?? string.Empty));
        Notify();
    }

    public HeldCart? PopHeldCart(string id)
    {
        var found = HeldCarts.FirstOrDefault(h => h.Id == id);
        if (found == null) return null;
        HeldCarts.Remove(found);
        Notify();
        return found;
    }

    public OperationResult SaveProduct(Product product, bool isNew, bool isAdmin, string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut modifier ou ajouter des produits.");
        if (string.IsNullOrWhiteSpace(product.ProductCode) || string.IsNullOrWhiteSpace(product.Description))
            return new OperationResult(false, "Le code produit et la description sont obligatoires.");
        if (product.Price < 0m || product.CostPrice < 0m || product.Qty < 0 || product.Reorder < 0)
            return new OperationResult(false, "Les prix et quantités ne peuvent pas être négatifs.");

        var vendor = Vendors.FirstOrDefault(v => v.Id == product.VendorId);
        product.VendorName = vendor?.VendorName ?? product.VendorName ?? string.Empty;
        product.Barcode ??= string.Empty;
        product.BrandName ??= string.Empty;
        product.CategoryName ??= string.Empty;

        if (isNew)
        {
            if (Products.Any(p => p.ProductCode.Equals(product.ProductCode, StringComparison.OrdinalIgnoreCase)))
                return new OperationResult(false, "Ce code produit existe déjà dans la base !");
            Products.Add(product);
            RecordAudit(actor, "Admin", "PRODUCT_CREATE", product.ProductCode, $"{product.Description} — {product.Price:N0} FCFA", "Info");
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
            existing.IsTaxable = product.IsTaxable;
            RecordAudit(actor, "Admin", "PRODUCT_UPDATE", product.ProductCode, $"{product.Description} — {product.Price:N0} FCFA", "Info");
        }

        Notify();
        return new OperationResult(true, "Produit enregistré avec succès.");
    }

    public OperationResult DeleteProduct(string productCode, bool isAdmin, string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer un produit.");
        var p = Products.FirstOrDefault(x => x.ProductCode == productCode);
        if (p == null) return new OperationResult(false, "Produit introuvable.");
        Products.Remove(p);
        RecordAudit(actor, "Admin", "PRODUCT_DELETE", productCode, $"Suppression de {p.Description}", "Warning");
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
            RefNo = TransactionNumberGenerator.NextStockRef(),
            ProductCode = product.ProductCode,
            Description = product.Description,
            Qty = diff,
            StockInDate = DateTime.Now,
            StockInBy = string.IsNullOrWhiteSpace(user) ? "admin" : user,
            Status = $"{adjustmentType} - {justification}",
            VendorId = product.VendorId,
            VendorName = product.VendorName ?? string.Empty
        });

        RecordAudit(user, "Admin", "STOCK_ADJUST", productCode, $"{adjustmentType} ({diff:+#;-#;0}) — {justification}", "Info");
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
            if (Brands.Any(b => b.BrandName.Equals(name, StringComparison.OrdinalIgnoreCase) && b.Id != id.Value))
                return new OperationResult(false, "Cette marque existe déjà.");
            string oldName = existing.BrandName;
            existing.BrandName = name;
            foreach (var p in Products.Where(p => p.BrandName == oldName)) p.BrandName = name;
        }
        else
        {
            if (Brands.Any(b => b.BrandName.Equals(name, StringComparison.OrdinalIgnoreCase)))
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
            if (Categories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase) && c.Id != id.Value))
                return new OperationResult(false, "Cette catégorie existe déjà.");
            string oldName = existing.CategoryName;
            existing.CategoryName = name;
            foreach (var p in Products.Where(p => p.CategoryName == oldName)) p.CategoryName = name;
        }
        else
        {
            if (Categories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase)))
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

    public OperationResult SaveWallet(CustomerWallet wallet, decimal rechargeAmount, bool isNew, string actor = "POS")
    {
        if (string.IsNullOrWhiteSpace(wallet.Phone))
            return new OperationResult(false, "Le numéro de téléphone est obligatoire.");
        if (rechargeAmount < 0m)
            return new OperationResult(false, "Le montant est invalide ou négatif.");

        wallet.CustomerName ??= string.Empty;
        wallet.Email ??= string.Empty;

        if (isNew)
        {
            if (CustomerWallets.Any(w => w.Phone == wallet.Phone))
                return new OperationResult(false, "Ce portefeuille existe déjà.");
            wallet.Balance = rechargeAmount;
            CustomerWallets.Add(wallet);
            if (rechargeAmount > 0m)
            {
                WalletTransactions.Add(new WalletTransaction
                {
                    Id = NextId(WalletTransactions.Select(w => w.Id)),
                    Phone = wallet.Phone,
                    TransNo = TransactionNumberGenerator.NextTransactionNo("WLT"),
                    Type = "Credit",
                    Amount = rechargeAmount,
                    CreatedAt = DateTime.Now
                });
            }
            RecordAudit(actor, "POS", "WALLET_CREATE", wallet.Phone, $"Solde initial : {rechargeAmount:N0} FCFA", "Info");
            Notify();
            return new OperationResult(true, "Portefeuille créé avec succès.");
        }

        if (rechargeAmount <= 0m)
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
            TransNo = TransactionNumberGenerator.NextTransactionNo("WLT"),
            Type = "Credit",
            Amount = rechargeAmount,
            CreatedAt = DateTime.Now
        });

        RecordAudit(actor, "POS", "WALLET_TOPUP", existing.Phone, $"Recharge : +{rechargeAmount:N0} FCFA", "Info");
        Notify();
        return new OperationResult(true, $"Recharge de {rechargeAmount:N0} FCFA effectuée sur {existing.Phone}.");
    }

    public OperationResult DeleteWallet(string phone, bool isAdmin, string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer un portefeuille.");
        var w = CustomerWallets.FirstOrDefault(x => x.Phone == phone);
        if (w == null) return new OperationResult(false, "Portefeuille introuvable.");
        CustomerWallets.Remove(w);
        WalletTransactions.RemoveAll(t => t.Phone == phone);
        RecordAudit(actor, "Admin", "WALLET_DELETE", phone, $"Suppression du portefeuille {w.CustomerName}", "Warning");
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

    public OperationResult CancelSale(int soldItemId, string cancelledBy, bool isAdmin, string reason = "Annulation demandée par l'administrateur.")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut annuler une transaction.");
        var item = SoldItems.FirstOrDefault(s => s.Id == soldItemId);
        if (item == null) return new OperationResult(false, "Transaction introuvable.");
        if (item.Status == "Refunded") return new OperationResult(false, "Cette ligne a déjà été remboursée.");

        return RefundLines(new List<SoldItem> { item }, cancelledBy, reason);
    }

    public OperationResult RefundTransaction(string transNo, string cancelledBy, bool isAdmin, string reason = "Remboursement ticket complet")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut rembourser une transaction.");
        var lines = SoldItems.Where(s => s.TransNo == transNo && s.Status != "Refunded").ToList();
        if (lines.Count == 0) return new OperationResult(false, "Ticket introuvable ou déjà remboursé.");

        return RefundLines(lines, cancelledBy, reason);
    }

    private OperationResult RefundLines(List<SoldItem> lines, string cancelledBy, string reason)
    {
        string finalReason = string.IsNullOrWhiteSpace(reason) ? "Remboursement administrateur" : reason.Trim();
        decimal totalRefund = lines.Sum(l => l.Total);
        var first = lines[0];
        bool walletRefunded = false;

        // 1. Restore product stock & mark lines as Refunded (preserve immutable ledger)
        foreach (var item in lines)
        {
            var prod = Products.FirstOrDefault(p => p.ProductCode == item.ProductCode);
            if (prod != null) prod.Qty += item.Qty;
            item.Status = "Refunded";
        }

        // 2. Refund customer wallet & reverse loyalty points if applicable
        if (!string.IsNullOrWhiteSpace(first.CustomerPhone))
        {
            var wallet = CustomerWallets.FirstOrDefault(w => w.Phone == first.CustomerPhone);
            if (wallet != null)
            {
                bool isWalletPayment = first.PaymentMode.Contains("Portefeuille", StringComparison.OrdinalIgnoreCase);
                if (isWalletPayment)
                {
                    // For split payments, refund up to the originally debited wallet amount
                    decimal debitAmount = WalletTransactions
                        .Where(t => t.TransNo == first.TransNo && t.Phone == wallet.Phone && t.Type == "Debit")
                        .Sum(t => t.Amount);
                    decimal alreadyRefunded = WalletTransactions
                        .Where(t => t.TransNo == first.TransNo && t.Phone == wallet.Phone && t.Type == "Refund")
                        .Sum(t => t.Amount);
                    decimal refundableWallet = debitAmount > 0m
                        ? Math.Min(totalRefund, Math.Max(0m, debitAmount - alreadyRefunded))
                        : totalRefund;

                    if (refundableWallet > 0m)
                    {
                        wallet.Balance += refundableWallet;
                        WalletTransactions.Add(new WalletTransaction
                        {
                            Id = NextId(WalletTransactions.Select(w => w.Id)),
                            Phone = wallet.Phone,
                            TransNo = first.TransNo,
                            Type = "Refund",
                            Amount = refundableWallet,
                            CreatedAt = DateTime.Now
                        });
                        walletRefunded = true;
                    }
                }

                wallet.TotalSpent = Math.Max(0m, wallet.TotalSpent - totalRefund);
                int pointsToDeduct = PosCalculator.CalculateLoyaltyPoints(totalRefund);
                wallet.LoyaltyPoints = Math.Max(0, wallet.LoyaltyPoints - pointsToDeduct);
            }
        }

        // 3. Adjust active cash shift expected cash if paid in cash during active shift
        var shift = ActiveShift;
        if (shift != null && first.SoldDate >= shift.OpenedAt && first.PaymentMode.Equals("Espèces", StringComparison.OrdinalIgnoreCase))
        {
            shift.ExpectedCash = Math.Max(0m, shift.ExpectedCash - totalRefund);
            shift.CashSales = Math.Max(0m, shift.CashSales - totalRefund);
            shift.TotalSales = Math.Max(0m, shift.TotalSales - totalRefund);
        }

        // 4. Record CancelledOrder entries + Audit trail
        int nextCancelId = NextId(CancelledOrders.Select(c => c.Id));
        foreach (var item in lines)
        {
            CancelledOrders.Add(new CancelledOrder
            {
                Id = nextCancelId++,
                TransNo = item.TransNo,
                ProductCode = item.ProductCode,
                Description = item.Description,
                Price = item.Price,
                Qty = item.Qty,
                Total = item.Total,
                SoldDate = item.SoldDate,
                CancelledAt = DateTime.Now,
                VoidBy = cancelledBy,
                CancelledBy = cancelledBy,
                Reason = finalReason,
                Action = walletRefunded ? "Remboursement Portefeuille" : "Remboursement",
                WalletRefunded = walletRefunded
            });
        }

        RecordAudit(
            cancelledBy,
            "Admin",
            "SALE_REFUND",
            first.TransNo,
            $"Remboursement {totalRefund:N0} FCFA (Stock restauré{(walletRefunded ? ", Wallet recrédité" : "")}) — {finalReason}",
            "Warning");

        Notify();
        string msg = walletRefunded
            ? $"Ticket {first.TransNo} remboursé ({totalRefund:N0} FCFA), stock restauré et portefeuille client recrédité."
            : $"Ticket {first.TransNo} remboursé ({totalRefund:N0} FCFA) et stock restauré avec succès.";
        return new OperationResult(true, msg);
    }

    public OperationResult SaveExpense(Expense expense, bool isNew, string addedBy, bool isAdmin)
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut modifier ou ajouter des dépenses.");
        if (string.IsNullOrWhiteSpace(expense.Category) || expense.Amount <= 0m)
            return new OperationResult(false, "Veuillez renseigner une catégorie et un montant supérieur à zéro.");

        expense.Description ??= string.Empty;
        if (isNew)
        {
            expense.Id = NextId(Expenses.Select(e => e.Id));
            expense.AddedBy = string.IsNullOrWhiteSpace(addedBy) ? "Admin" : addedBy;
            Expenses.Add(expense);
            RecordAudit(expense.AddedBy, "Admin", "EXPENSE_CREATE", expense.Category, $"{expense.Amount:N0} FCFA — {expense.Description}", "Info");
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

    public OperationResult DeleteExpense(int id, bool isAdmin, string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Seul l'administrateur peut supprimer une dépense.");
        var e = Expenses.FirstOrDefault(x => x.Id == id);
        if (e != null)
        {
            Expenses.Remove(e);
            RecordAudit(actor, "Admin", "EXPENSE_DELETE", e.Category, $"Suppression dépense {e.Amount:N0} FCFA", "Warning");
        }
        Notify();
        return new OperationResult(true, "Dépense supprimée.");
    }

    public OperationResult SaveUser(User user, bool isAdmin, string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(user.Username))
            return new OperationResult(false, "L'identifiant est obligatoire.");

        var existing = Users.FirstOrDefault(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Name = string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name;
            existing.Role = string.IsNullOrWhiteSpace(user.Role) ? existing.Role : user.Role;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = PasswordHasher.IsHashed(user.Password)
                    ? user.Password
                    : PasswordHasher.Hash(user.Password);
            }
            existing.IsActive = user.IsActive;
            RecordAudit(actor, "Admin", "USER_UPDATE", existing.Username, $"Rôle : {existing.Role} | Actif : {existing.IsActive}", "Info");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                return new OperationResult(false, "Le mot de passe est obligatoire pour un nouvel utilisateur.");

            Users.Add(new User
            {
                Username = user.Username.Trim(),
                Name = string.IsNullOrWhiteSpace(user.Name) ? user.Username.Trim() : user.Name.Trim(),
                Role = string.IsNullOrWhiteSpace(user.Role) ? "Cashier" : user.Role,
                Password = PasswordHasher.IsHashed(user.Password) ? user.Password : PasswordHasher.Hash(user.Password),
                IsActive = user.IsActive
            });
            RecordAudit(actor, "Admin", "USER_CREATE", user.Username, $"Rôle : {user.Role}", "Info");
        }

        Notify();
        return new OperationResult(true, "Utilisateur enregistré avec succès !");
    }

    public OperationResult SaveStore(
        string storeName,
        string address,
        string momoCode,
        string orangeCode,
        bool isAdmin,
        string? phone = null,
        string? niu = null,
        string? rccm = null,
        decimal? taxRate = null,
        string actor = "Admin")
    {
        if (!isAdmin) return new OperationResult(false, "Accès réservé à l'administrateur.");
        if (string.IsNullOrWhiteSpace(storeName)) return new OperationResult(false, "Le nom de la boutique est obligatoire.");
        if (taxRate.HasValue && (taxRate.Value < 0m || taxRate.Value > 50m))
            return new OperationResult(false, "Le taux de TVA doit être compris entre 0 % et 50 %.");

        Store.StoreName = storeName;
        Store.Address = address ?? string.Empty;
        Store.MomoCode = momoCode ?? string.Empty;
        Store.OrangeCode = orangeCode ?? string.Empty;
        if (phone != null) Store.Phone = phone;
        if (niu != null) Store.Niu = niu;
        if (rccm != null) Store.Rccm = rccm;
        if (taxRate.HasValue) Store.TaxRate = taxRate.Value;

        RecordAudit(actor, "Admin", "STORE_UPDATE", Store.StoreName, $"TVA : {Store.TaxRate:0.##}% | NIU : {Store.Niu}", "Info");
        Notify();
        return new OperationResult(true, "Paramètres système enregistrés !");
    }

    public OperationResult ChangePassword(string username, string oldPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            return new OperationResult(false, "Veuillez remplir tous les champs de mot de passe.");
        if (newPassword != confirmPassword)
            return new OperationResult(false, "Le nouveau mot de passe et la confirmation ne correspondent pas.");
        if (newPassword.Length < 4)
            return new OperationResult(false, "Le nouveau mot de passe doit contenir au moins 4 caractères.");

        var user = Users.FirstOrDefault(u => u.Username == username);
        if (user == null || !PasswordHasher.Verify(oldPassword, user.Password))
            return new OperationResult(false, "L'ancien mot de passe est incorrect.");

        user.Password = PasswordHasher.Hash(newPassword);
        RecordAudit(user.Name, user.Role, "AUTH_PASSWORD_CHANGE", user.Username, "Mot de passe mis à jour", "Warning");
        Notify();
        return new OperationResult(true, "Votre mot de passe a été modifié avec succès !");
    }

    public void RecordAudit(string actor, string role, string action, string target, string details, string severity = "Info")
    {
        AuditLogs.Add(new AuditEntry
        {
            Id = NextId(AuditLogs.Select(a => a.Id)),
            Timestamp = DateTime.Now,
            Actor = string.IsNullOrWhiteSpace(actor) ? "Système" : actor,
            Role = string.IsNullOrWhiteSpace(role) ? "System" : role,
            Action = action,
            Target = target,
            Details = details,
            Severity = severity
        });

        // Cap in-memory demo audit log to latest 500 entries
        if (AuditLogs.Count > 500)
            AuditLogs.RemoveRange(0, AuditLogs.Count - 500);
    }

    public List<SalesTicketSummary> GetGroupedSalesTickets(DateTime? startDate, DateTime? endDate, string? search)
    {
        var query = SoldItems.AsEnumerable();
        if (startDate.HasValue) query = query.Where(s => s.SoldDate >= startDate.Value.Date);
        if (endDate.HasValue) query = query.Where(s => s.SoldDate < endDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            query = query.Where(s =>
                s.TransNo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.ProductCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.Cashier.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.PaymentMode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.PaymentReference.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.CustomerPhone.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .GroupBy(s => s.TransNo)
            .Select(g =>
            {
                var lines = g.OrderBy(x => x.Id).ToList();
                var first = lines[0];
                string status = lines.All(x => x.Status == "Refunded") ? "Refunded" : "Completed";
                return new SalesTicketSummary(
                    TransNo: g.Key,
                    SoldDate: first.SoldDate,
                    Cashier: first.Cashier,
                    PaymentMode: first.PaymentMode,
                    PaymentReference: first.PaymentReference,
                    CustomerPhone: first.CustomerPhone,
                    Status: status,
                    TotalQty: lines.Sum(x => x.Qty),
                    SubTotalHT: lines.Sum(x => x.SubTotalHT),
                    TaxAmount: lines.Sum(x => x.TaxAmount),
                    Discount: lines.Sum(x => x.Discount),
                    TotalTTC: lines.Sum(x => x.Total),
                    Lines: lines);
            })
            .OrderByDescending(t => t.SoldDate)
            .ToList();
    }

    public DashboardMetrics GetDashboardMetrics(DashboardPeriod period = DashboardPeriod.AllTime)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        DateTime? periodStart = period switch
        {
            DashboardPeriod.Today => today,
            DashboardPeriod.Last7Days => today.AddDays(-6),
            DashboardPeriod.Last30Days => today.AddDays(-29),
            _ => null
        };

        var activeSalesAll = SoldItems.Where(s => s.Status != "Refunded").ToList();
        var periodSales = periodStart.HasValue
            ? activeSalesAll.Where(s => s.SoldDate >= periodStart.Value && s.SoldDate < tomorrow).ToList()
            : activeSalesAll;
        var periodExpenses = periodStart.HasValue
            ? Expenses.Where(e => e.Date >= periodStart.Value && e.Date < tomorrow).ToList()
            : Expenses;
        var periodCancelled = periodStart.HasValue
            ? CancelledOrders.Where(c => c.SoldDate >= periodStart.Value && c.SoldDate < tomorrow).ToList()
            : CancelledOrders;

        var m = new DashboardMetrics
        {
            Period = period,
            TodaySales = activeSalesAll.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => s.Total),
            PeriodSales = periodSales.Sum(s => s.Total),
            TotalSalesAllTime = activeSalesAll.Sum(s => s.Total),
            YesterdaySales = activeSalesAll.Where(s => s.SoldDate >= yesterday && s.SoldDate < today).Sum(s => s.Total),
            TotalProducts = Products.Count,
            TotalStockQuantity = Products.Sum(p => p.Qty),
            LowStockItems = Products.Count(p => p.Qty <= p.Reorder),
            CriticalStockList = Products.Where(p => p.Qty <= p.Reorder).ToList(),
            TotalCustomers = CustomerWallets.Count,
            TotalLoyaltyPoints = CustomerWallets.Sum(w => w.LoyaltyPoints),
            TotalWalletsBalance = CustomerWallets.Sum(w => w.Balance),
            TotalItemsSoldToday = activeSalesAll.Where(s => s.SoldDate >= today && s.SoldDate < tomorrow).Sum(s => s.Qty),
            PeriodItemsSold = periodSales.Sum(s => s.Qty),
            TotalItemsSoldAllTime = activeSalesAll.Sum(s => s.Qty),
            TotalCancelledAmount = periodCancelled.Sum(c => c.Total),
            TotalDiscountAmount = periodSales.Sum(s => s.Discount),
            TotalTaxCollected = periodSales.Sum(s => s.TaxAmount),
            TotalUsers = Users.Count,
            AdminCount = Users.Count(u => u.Role == "Admin"),
            CashierCount = Users.Count(u => u.Role == "Cashier"),
            TotalVendors = Vendors.Count,
            TotalCOGS = periodSales.Sum(s => s.CostPrice * s.Qty),
            TotalExpenses = periodExpenses.Sum(e => e.Amount),
            Last7DaysLabels = new string[7],
            Last7DaysSales = new double[7],
            Last7DaysStockIn = new double[7],
            Last7DaysStockOut = new double[7],
            Last7DaysDiscounts = new double[7],
            Last7DaysCancelled = new double[7]
        };

        m.NetProfit = m.PeriodSales - m.TotalCOGS - m.TotalExpenses;

        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var nextD = d.AddDays(1);
            int idx = 6 - i;
            m.Last7DaysLabels[idx] = d.ToString("dd MMM");
            m.Last7DaysSales[idx] = (double)activeSalesAll.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Total);
            m.Last7DaysStockIn[idx] = Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty > 0).Sum(s => s.Qty);
            int manualOut = Math.Abs(Stocks.Where(s => s.StockInDate >= d && s.StockInDate < nextD && s.Qty < 0).Sum(s => s.Qty));
            int salesOut = activeSalesAll.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Qty);
            m.Last7DaysStockOut[idx] = manualOut + salesOut;
            m.Last7DaysDiscounts[idx] = (double)activeSalesAll.Where(s => s.SoldDate >= d && s.SoldDate < nextD).Sum(s => s.Discount);
            m.Last7DaysCancelled[idx] = (double)CancelledOrders.Where(c => c.SoldDate >= d && c.SoldDate < nextD).Sum(c => c.Total);
        }

        m.TopProductsStock = Products.OrderByDescending(p => p.Qty).Take(5).Select(p => (p.ProductCode ?? "", (double)p.Qty)).ToList();
        m.TopCustomers = CustomerWallets.OrderByDescending(w => w.TotalSpent).Take(5).Select(w => (string.IsNullOrEmpty(w.CustomerName) ? w.Phone : w.CustomerName, (double)w.TotalSpent)).ToList();
        m.TopSoldItems = periodSales.GroupBy(s => s.Description ?? "").Select(g => (g.Key, (double)g.Sum(x => x.Qty))).OrderByDescending(x => x.Item2).Take(5).ToList();
        m.PaymentModes = periodSales.GroupBy(s => s.PaymentMode ?? "Espèces").Select(g => (g.Key, (double)g.Sum(x => x.Total))).ToList();
        m.UsersByRole = Users.GroupBy(u => u.Role ?? "Autre").Select(g => (g.Key, (double)g.Count())).ToList();
        m.VendorsByProducts = Products.Where(p => p.VendorId > 0).GroupBy(p => p.VendorName ?? "Inconnu").Select(g => (g.Key, (double)g.Count())).ToList();

        return m;
    }

    public static string ExportSalesCsv(IEnumerable<SoldItem> sales)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF'); // UTF-8 BOM for Excel
        sb.AppendLine("Date;TransNo;Code;Description;Qty;PriceTTC;Discount;SubTotalHT;TVA;TotalTTC;Cashier;PaymentMode;PaymentRef;CustomerPhone;Status");
        foreach (var s in sales)
        {
            sb.AppendLine(string.Join(";",
                Csv(s.SoldDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                Csv(s.TransNo),
                Csv(s.ProductCode),
                Csv(s.Description),
                s.Qty,
                s.Price.ToString("0.##", CultureInfo.InvariantCulture),
                s.Discount.ToString("0.##", CultureInfo.InvariantCulture),
                s.SubTotalHT.ToString("0.##", CultureInfo.InvariantCulture),
                s.TaxAmount.ToString("0.##", CultureInfo.InvariantCulture),
                s.Total.ToString("0.##", CultureInfo.InvariantCulture),
                Csv(s.Cashier),
                Csv(s.PaymentMode),
                Csv(s.PaymentReference),
                Csv(s.CustomerPhone),
                Csv(s.Status)));
        }
        return sb.ToString();
    }

    public static string ExportInventoryCsv(IEnumerable<Product> products)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        sb.AppendLine("ProductCode;Barcode;Description;Brand;Category;Vendor;CostPrice;SalePrice;Qty;Reorder;Taxable");
        foreach (var p in products)
        {
            sb.AppendLine(string.Join(";",
                Csv(p.ProductCode),
                Csv(p.Barcode),
                Csv(p.Description),
                Csv(p.BrandName),
                Csv(p.CategoryName),
                Csv(p.VendorName),
                p.CostPrice.ToString("0.##", CultureInfo.InvariantCulture),
                p.Price.ToString("0.##", CultureInfo.InvariantCulture),
                p.Qty,
                p.Reorder,
                p.IsTaxable ? "Oui" : "Non"));
        }
        return sb.ToString();
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        string escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static int NextId(IEnumerable<int> ids) => ids.Any() ? ids.Max() + 1 : 1;
}
