using System;
using System.IO;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

[Collection("SharedState")]
public class DashboardViewModelTests
{
    [Fact]
    public void LoadStats_DoesNotThrow_AndPopulatesAllCharts()
    {
        var tempDataHome = Path.Combine(Path.GetTempPath(), $"camaroes_test_db_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDataHome);
        var prevXdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", tempDataHome);

        try
        {
            DatabaseInitializer.Initialize();

            var vm = new DashboardViewModel();
            vm.RefreshCommand.Execute(null);

            Assert.NotNull(vm.SalesChart);
            Assert.NotNull(vm.TodaySalesChart);
            Assert.NotNull(vm.TotalProductsChart);
            Assert.NotNull(vm.StockMovementsChart);
            Assert.NotNull(vm.LowStockChart);
            Assert.NotNull(vm.WalletsChart);
            Assert.NotNull(vm.ItemsSoldTodayChart);
            Assert.NotNull(vm.CancelledChart);
            Assert.NotNull(vm.UsersChart);
            Assert.NotNull(vm.DiscountChart);
            Assert.NotNull(vm.VendorsChart);
            Assert.NotNull(vm.ProfitChart);
            Assert.NotNull(vm.PaymentModesChart);
            Assert.NotNull(vm.UsersList);
            Assert.NotNull(vm.CriticalStockList);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", prevXdg);
            if (Directory.Exists(tempDataHome)) Directory.Delete(tempDataHome, recursive: true);
        }
    }
}
