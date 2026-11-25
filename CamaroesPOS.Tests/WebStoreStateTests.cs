using System;
using System.Collections.Generic;
using System.Linq;
using CamaroesPOS.Application.Security;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Web.Services;
using Xunit;

namespace CamaroesPOS.Tests;

public class WebStoreStateTests
{
    [Fact]
    public void SeedDemoData_PopulatesProductsHashedUsersShiftsAndSevenDaySales()
    {
        var store = new PosStoreState();
        store.SeedDemoData();

        Assert.True(store.Products.Count >= 5);
        Assert.Contains(store.Users, u => u.Username == "admin" && u.Role == "Admin");
        Assert.Contains(store.Users, u => u.Username == "caissier1" && u.Role == "Cashier");
        Assert.All(store.Users, u => Assert.True(PasswordHasher.IsHashed(u.Password)));
        Assert.NotEmpty(store.SoldItems);
        Assert.NotEmpty(store.CustomerWallets);
        Assert.NotEmpty(store.Vendors);
        Assert.NotEmpty(store.Brands);
        Assert.NotEmpty(store.Categories);
        Assert.NotEmpty(store.Expenses);
        Assert.NotEmpty(store.CashShifts);
        Assert.NotEmpty(store.AuditLogs);

        // Snapshot JSON must NEVER expose plaintext password "123"
        string json = store.ExportSnapshotJson();
        Assert.DoesNotContain("\"Password\":\"123\"", json);
    }

    [Fact]
    public void CheckoutCash_AppliesClampedDiscount_DecrementsStock_ComputesVat_AndBlocksOversell()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var product = store.Products.First(p => p.ProductCode == "P006"); // Qty = 3
        int initialQty = product.Qty;

        // Oversell attempt (10 units when only 3 available) MUST be rejected!
        var oversellCart = new List<CartItem>
        {
            new() { ProductCode = product.ProductCode, ProductDescription = product.Description, CostPrice = product.CostPrice, Price = product.Price, Qty = 10, Total = product.Price * 10, IsTaxable = true }
        };
        var overRes = store.CheckoutCash(oversellCart, 0m, 1000000m, null, "Admin");
        Assert.False(overRes.Success);
        Assert.Equal(initialQty, product.Qty);

        // Multi-line oversell attempt (2 + 2 = 4 > 3) MUST also be rejected!
        var multiLineOversell = new List<CartItem>
        {
            new() { ProductCode = product.ProductCode, ProductDescription = product.Description, CostPrice = product.CostPrice, Price = product.Price, Qty = 2, Total = product.Price * 2, IsTaxable = true },
            new() { ProductCode = product.ProductCode, ProductDescription = product.Description, CostPrice = product.CostPrice, Price = product.Price, Qty = 2, Total = product.Price * 2, IsTaxable = true }
        };
        Assert.False(store.CheckoutCash(multiLineOversell, 0m, 1000000m, null, "Admin").Success);
        Assert.Equal(initialQty, product.Qty);

        // Valid sale: 2 units of P002 (15,000 * 2 = 30,000), 10% discount => 27,000 TTC
        var p2 = store.Products.First(p => p.ProductCode == "P002");
        int p2Initial = p2.Qty;
        var cart = new List<CartItem>
        {
            new() { ProductCode = p2.ProductCode, ProductDescription = p2.Description, CostPrice = p2.CostPrice, Price = p2.Price, Qty = 2, Total = p2.Price * 2, IsTaxable = true }
        };

        var failRes = store.CheckoutCash(cart, 10m, 20000m, null, "Admin");
        Assert.False(failRes.Success);
        Assert.Equal(p2Initial, p2.Qty);

        var okRes = store.CheckoutCash(cart, 10m, 30000m, "0601020304", "Admin");
        Assert.True(okRes.Success);
        Assert.Equal(27000m, okRes.TotalAmount);
        Assert.Equal(3000m, okRes.ChangeAmount);
        Assert.Equal(okRes.TotalAmount, okRes.SubTotalHT + okRes.TaxAmount);
        Assert.True(okRes.TaxAmount > 0m);
        Assert.Equal(p2Initial - 2, p2.Qty);
        Assert.Contains(store.SoldItems, s => s.TransNo == okRes.TransNo && s.PaymentMode == "Espèces");
    }

    [Fact]
    public void CheckoutWallet_AndRefundTransaction_RestoresStockWalletBalanceAndLoyaltyPoints()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var wallet = store.CustomerWallets.First(w => w.Phone == "0601020304"); // 150,000 FCFA
        decimal initialBalance = wallet.Balance;
        decimal initialSpent = wallet.TotalSpent;
        int initialPoints = wallet.LoyaltyPoints;

        var p2 = store.Products.First(p => p.ProductCode == "P002");
        int initialQty = p2.Qty;

        var cart = new List<CartItem>
        {
            new() { ProductCode = p2.ProductCode, ProductDescription = p2.Description, CostPrice = p2.CostPrice, Price = p2.Price, Qty = 2, Total = p2.Price * 2, IsTaxable = true }
        };

        var payRes = store.CheckoutWallet(cart, 0m, wallet.Phone, "Caissier 1");
        Assert.True(payRes.Success);
        Assert.Equal(initialBalance - 30000m, wallet.Balance);
        Assert.Equal(initialSpent + 30000m, wallet.TotalSpent);
        Assert.Equal(initialPoints + 30, wallet.LoyaltyPoints);
        Assert.Equal(initialQty - 2, p2.Qty);

        // Refund the whole ticket -> stock, wallet balance, totalSpent & loyaltyPoints MUST all be restored!
        var refundRes = store.RefundTransaction(payRes.TransNo, "Administrateur", isAdmin: true, reason: "Retour client");
        Assert.True(refundRes.Success);
        Assert.Equal(initialBalance, wallet.Balance);
        Assert.Equal(initialSpent, wallet.TotalSpent);
        Assert.Equal(initialPoints, wallet.LoyaltyPoints);
        Assert.Equal(initialQty, p2.Qty);
        Assert.Contains(store.WalletTransactions, wt => wt.TransNo == payRes.TransNo && wt.Type == "Refund" && wt.Amount == 30000m);
        Assert.Contains(store.CancelledOrders, c => c.TransNo == payRes.TransNo && c.WalletRefunded);
        Assert.All(store.SoldItems.Where(s => s.TransNo == payRes.TransNo), s => Assert.Equal("Refunded", s.Status));

        // Double refund MUST be rejected
        Assert.False(store.RefundTransaction(payRes.TransNo, "Administrateur", isAdmin: true, reason: "Doublon").Success);
    }

    [Fact]
    public void CheckoutMobileMoney_AndSplitPayment_RecordReferencesAndUpdateActiveShift()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var shift = store.ActiveShift!;
        decimal initialMomo = shift.MobileMoneySales;
        decimal initialCash = shift.CashSales;

        var cart = new List<CartItem>
        {
            new() { ProductCode = "P002", ProductDescription = "Souris Sans Fil", CostPrice = 9500m, Price = 15000m, Qty = 2, Total = 30000m, IsTaxable = true }
        };

        var momoRes = store.CheckoutMobileMoney(cart, 0m, "MTN MoMo", "+237 677112233", null, "Caissier 1");
        Assert.True(momoRes.Success);
        Assert.StartsWith("MOMO-", momoRes.PaymentReference);
        Assert.Equal(initialMomo + 30000m, shift.MobileMoneySales);

        var splitRes = store.CheckoutSplit(cart, 0m, cashPortion: 10000m, secondaryMode: "Orange Money", secondaryPortion: 20000m, customerPhone: null, cashier: "Caissier 1");
        Assert.True(splitRes.Success);
        Assert.Contains("Mixte", splitRes.PaymentMode);
        Assert.Equal(initialCash + 10000m, shift.CashSales);
        Assert.Equal(initialMomo + 50000m, shift.MobileMoneySales);
    }

    [Fact]
    public void CashShift_CloseAndReopen_ComputesDiscrepancyAndRecordsAudit()
    {
        var store = new PosStoreState();
        store.SeedDemoData();

        var active = store.ActiveShift!;
        decimal expected = active.ExpectedCash;
        var closeRes = store.CloseShift("Administrateur", expected - 2000m, "Manquant caisse");
        Assert.True(closeRes.Success);
        Assert.Null(store.ActiveShift);
        Assert.Equal(-2000m, active.Discrepancy);

        var openRes = store.OpenShift("Caissier 1", 75000m);
        Assert.True(openRes.Success);
        Assert.NotNull(store.ActiveShift);
        Assert.Equal(75000m, store.ActiveShift!.ExpectedCash);
    }

    [Fact]
    public void AdminGuards_BlockCashierFromMutatingInventoryVendorsExpensesOrdersAndDeletingWallets()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        int saleId = store.SoldItems.First().Id;

        Assert.False(store.SaveProduct(new Product { ProductCode = "P999", Description = "Blocked" }, isNew: true, isAdmin: false).Success);
        Assert.False(store.DeleteProduct("P001", isAdmin: false).Success);
        Assert.False(store.AdjustStock("P001", "Entrée", 1, "Test", "caissier1", isAdmin: false).Success);
        Assert.False(store.SaveVendor(new Vendor { VendorName = "Blocked" }, isNew: true, isAdmin: false).Success);
        Assert.False(store.DeleteVendor(store.Vendors.First().Id, isAdmin: false).Success);
        Assert.False(store.SaveExpense(new Expense { Category = "Autre", Amount = 5000m }, isNew: true, "caissier1", isAdmin: false).Success);
        Assert.False(store.CancelSale(saleId, "caissier1", isAdmin: false).Success);
        Assert.False(store.DeleteWallet("0601020304", isAdmin: false).Success);

        Assert.True(store.SaveWallet(new CustomerWallet { Phone = "0699887766", CustomerName = "Client Comptoir" }, 10000m, isNew: true).Success);
    }

    [Fact]
    public void SessionState_RestoresOnlyFromValidSignedToken_AndRejectsSpoofedUsername()
    {
        var store = new PosStoreState();
        store.SeedDemoData();

        var session = new SessionState();
        var cashier = store.Authenticate("caissier1", "123");
        Assert.NotNull(cashier);

        string signedToken = session.Login(cashier!);
        session.Logout();

        // Spoofed raw username "admin" MUST be rejected
        Assert.False(session.TryRestoreFromToken("admin", store));
        Assert.False(session.IsAuthenticated);

        // Valid signed token restores exact user and role
        Assert.True(session.TryRestoreFromToken(signedToken, store));
        Assert.True(session.IsAuthenticated);
        Assert.False(session.IsAdmin);
    }

    [Fact]
    public void LocalizationService_FrenchAndEnglishHaveIdenticalKeys()
    {
        var loc = new LocalizationService();
        Assert.NotEmpty(loc.AllFrenchKeys);
        Assert.Equal(loc.AllFrenchKeys.ToHashSet(), loc.AllEnglishKeys.ToHashSet());

        Assert.Equal("fr", loc.CurrentLang);
        string frTitle = loc.T("Nav_Dashboard");
        loc.Toggle();
        Assert.Equal("en", loc.CurrentLang);
        string enTitle = loc.T("Nav_Dashboard");
        Assert.NotEqual(frTitle, enTitle);
    }

    [Fact]
    public void SnapshotJson_RoundTripsAllEntities_AndUpgradesLegacyPlaintextPasswords()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        store.SaveProduct(new Product { ProductCode = "P777", Description = "Test Snapshot", Price = 5000m, Qty = 3 }, isNew: true, isAdmin: true);

        string json = store.ExportSnapshotJson();

        var restored = new PosStoreState();
        restored.ImportSnapshotJson(json);
        Assert.Contains(restored.Products, p => p.ProductCode == "P777");
        Assert.NotEmpty(restored.CashShifts);

        var fallback = new PosStoreState();
        fallback.ImportSnapshotJson("{not valid json!!!");
        Assert.True(fallback.Products.Count >= 5);
        Assert.Contains(fallback.Users, u => u.Username == "admin");
    }
}
