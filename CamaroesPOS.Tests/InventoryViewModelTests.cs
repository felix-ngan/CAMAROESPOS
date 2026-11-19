using System;
using System.IO;
using System.Linq;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

[Collection("SharedState")]
public class InventoryViewModelTests
{
    [Fact]
    public void SaveAdjustment_RejectsNegativeQuantityInInventoryMode()
    {
        var tempDataHome = Path.Combine(Path.GetTempPath(), $"camaroes_inv_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDataHome);
        var prevXdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", tempDataHome);

        try
        {
            DatabaseInitializer.Initialize();
            SessionService.SetSession(new User { Username = "admin", Name = "Admin", Role = "Admin" });

            var vm = new InventoryViewModel();
            var product = vm.InventoryList.First();
            int originalQty = product.Qty;

            vm.AdjustStockCommand.Execute(product);
            vm.AdjustmentType = "Ajustement (Inventaire)";
            vm.AdjustmentQuantity = -5;
            vm.AdjustmentJustification = "Test negatif";
            vm.SaveAdjustmentCommand.Execute(null);

            using var context = new PosDbContext();
            var dbProduct = context.Products.First(p => p.ProductCode == product.ProductCode);
            Assert.Equal(originalQty, dbProduct.Qty);
        }
        finally
        {
            SessionService.Clear();
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", prevXdg);
            if (Directory.Exists(tempDataHome)) Directory.Delete(tempDataHome, recursive: true);
        }
    }
}
