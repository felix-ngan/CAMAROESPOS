using System;
using System.Collections.Generic;
using System.Linq;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Web.Services;
using Xunit;

namespace CamaroesPOS.Tests;

public class WebStoreStateTests
{
    [Fact]
    public void SeedDemoData_PopulatesProductsUsersAndSevenDaySales()
    {
        var store = new PosStoreState();
        store.SeedDemoData();

        Assert.True(store.Products.Count >= 5);
        Assert.Contains(store.Users, u => u.Username == "admin" && u.Role == "Admin");
        Assert.Contains(store.Users, u => u.Username == "caissier1" && u.Role == "Cashier");
        Assert.NotEmpty(store.SoldItems);
        Assert.NotEmpty(store.CustomerWallets);
        Assert.NotEmpty(store.Vendors);
        Assert.NotEmpty(store.Brands);
        Assert.NotEmpty(store.Categories);
        Assert.NotEmpty(store.Expenses);
    }

    [Fact]
    public void CheckoutCash_AppliesClampedDiscount_DecrementsStock_AndCreatesSoldItems()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var product = store.Products.First(p => p.ProductCode == "P002");
        int initialQty = product.Qty;

        var cart = new List<CartItem>
        {
            new() { ProductCode = product.ProductCode, ProductDescription = product.Description, CostPrice = product.CostPrice, Price = product.Price, Qty = 2, Total = product.Price * 2 }
        };

        // 10% discount on 30,000 = 27,000
        var failRes = store.CheckoutCash(cart, 10, 20000, null, "Admin");
        Assert.False(failRes.Success);
        Assert.Equal(initialQty, product.Qty);

        var okRes = store.CheckoutCash(cart, 10, 30000, "0601020304", "Admin");
        Assert.True(okRes.Success);
        Assert.Equal(27000, okRes.TotalAmount);
        Assert.Equal(3000, okRes.ChangeAmount);
        Assert.Equal(initialQty - 2, product.Qty);
        Assert.Contains(store.SoldItems, s => s.TransNo == okRes.TransNo && s.PaymentMode == "Espèces");
    }

    [Fact]
    public void CheckoutWallet_RejectsInsufficientBalance_AndZeroTotal_AndSharesTransNoOnSuccess()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var wallet = store.CustomerWallets.First(w => w.Phone == "0611223344"); // 50,000 FCFA
        double initialBalance = wallet.Balance;

        var expensiveCart = new List<CartItem>
        {
            new() { ProductCode = "P001", ProductDescription = "Laptop Gamer", CostPrice = 1000000, Price = 1500000, Qty = 1, Total = 1500000 }
        };
        var failBal = store.CheckoutWallet(expensiveCart, 0, wallet.Phone, "Caissier 1");
        Assert.False(failBal.Success);
        Assert.Equal(initialBalance, wallet.Balance);

        var zeroCart = new List<CartItem>
        {
            new() { ProductCode = "P002", ProductDescription = "Souris", CostPrice = 8000, Price = 15000, Qty = 1, Total = 15000 }
        };
        var failZero = store.CheckoutWallet(zeroCart, 100, wallet.Phone, "Caissier 1");
        Assert.False(failZero.Success);

        var okRes = store.CheckoutWallet(zeroCart, 0, wallet.Phone, "Caissier 1");
        Assert.True(okRes.Success);
        Assert.Equal(initialBalance - 15000, wallet.Balance);
        Assert.Contains(store.WalletTransactions, wt => wt.TransNo == okRes.TransNo && wt.Type == "Debit" && wt.Amount == 15000);
        Assert.Contains(store.SoldItems, s => s.TransNo == okRes.TransNo && s.PaymentMode == "Portefeuille");
    }

    [Fact]
    public void AdjustStock_RejectsNegativeQuantity_AndInsufficientStockOut()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        var product = store.Products.First(p => p.ProductCode == "P004"); // Qty 5
        int initialQty = product.Qty;

        var negRes = store.AdjustStock(product.ProductCode, "Ajustement (Inventaire)", -3, "Test", "admin", isAdmin: true);
        Assert.False(negRes.Success);
        Assert.Equal(initialQty, product.Qty);

        var overOutRes = store.AdjustStock(product.ProductCode, "Sortie", initialQty + 10, "Casse", "admin", isAdmin: true);
        Assert.False(overOutRes.Success);
        Assert.Equal(initialQty, product.Qty);

        var okIn = store.AdjustStock(product.ProductCode, "Entrée", 4, "Réappro", "admin", isAdmin: true);
        Assert.True(okIn.Success);
        Assert.Equal(initialQty + 4, product.Qty);
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
        Assert.False(store.SaveExpense(new Expense { Category = "Autre", Amount = 5000 }, isNew: true, "caissier1", isAdmin: false).Success);
        Assert.False(store.CancelSale(saleId, "caissier1", isAdmin: false).Success);
        Assert.False(store.DeleteWallet("0601020304", isAdmin: false).Success);

        // Cashier CAN create/recharge a customer wallet at the counter
        Assert.True(store.SaveWallet(new CustomerWallet { Phone = "0699887766", CustomerName = "Client Comptoir" }, 10000, isNew: true).Success);
    }

    [Fact]
    public void SnapshotJson_RoundTripsAllEntities_AndFallsBackToSeedOnCorruptJson()
    {
        var store = new PosStoreState();
        store.SeedDemoData();
        store.SaveProduct(new Product { ProductCode = "P777", Description = "Test Snapshot", Price = 5000, Qty = 3 }, isNew: true, isAdmin: true);

        string json = store.ExportSnapshotJson();

        var restored = new PosStoreState();
        restored.ImportSnapshotJson(json);
        Assert.Contains(restored.Products, p => p.ProductCode == "P777");

        var fallback = new PosStoreState();
        fallback.ImportSnapshotJson("{not valid json!!!");
        Assert.True(fallback.Products.Count >= 5);
        Assert.Contains(fallback.Users, u => u.Username == "admin");
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
}
