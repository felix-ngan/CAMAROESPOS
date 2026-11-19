using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

public class DashboardViewModelTests
{
    [Fact]
    public void LoadStats_DoesNotThrow_AndPopulatesAllCharts()
    {
        DatabaseInitializer.Initialize();

        var vm = new DashboardViewModel();
        vm.LoadStats();

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
}
