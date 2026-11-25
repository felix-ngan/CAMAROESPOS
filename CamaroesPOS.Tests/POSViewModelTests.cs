using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

public class POSViewModelTests
{
    [Fact]
    public void DiscountPercentage_ClampsBetweenZeroAndHundred()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();

        vm.DiscountPercentage = -15m;
        Assert.Equal(0m, vm.DiscountPercentage);

        vm.DiscountPercentage = 150m;
        Assert.Equal(100m, vm.DiscountPercentage);

        vm.DiscountPercentage = 20m;
        Assert.Equal(20m, vm.DiscountPercentage);
    }

    [Fact]
    public void DiscountPercentageString_EmptyResetsDiscountToZero()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T001", Description = "Test", Price = 10000m, CostPrice = 5000m, Qty = 10 }, 1);

        vm.DiscountPercentageString = "10";
        Assert.Equal(10m, vm.DiscountPercentage);
        Assert.Equal(9000m, vm.TotalAmount);

        vm.DiscountPercentageString = "";
        Assert.Equal(0m, vm.DiscountPercentage);
        Assert.Equal(10000m, vm.TotalAmount);
    }

    [Fact]
    public void ExactAmountCommand_OnFractionalDiscountedTotal_EnablesConfirmCashPayment()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T002", Description = "Item", Price = 105m, CostPrice = 50m, Qty = 10 }, 1);
        vm.DiscountPercentageString = "15";

        vm.ExactAmountCommand.Execute(null);

        Assert.True(vm.ChangeAmount >= 0m);
        Assert.True(vm.ConfirmCashPaymentCommand.CanExecute(null));
    }

    [Fact]
    public void AddProductToCart_CapsAtAvailableStock_PreventingOversell()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        var scarce = new Product { ProductCode = "T_LOW", Description = "Scarce Item", Price = 5000m, CostPrice = 2000m, Qty = 3 };

        vm.AddProductToCart(scarce, 10);

        Assert.Single(vm.Cart);
        Assert.Equal(3, vm.Cart[0].Qty);
    }

    [Fact]
    public void ChangeAmount_CalculatesCorrectly_AndGuardsConfirmCashPayment()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();

        vm.AddProductToCart(new Product { ProductCode = "T001", Description = "Test Item", Price = 10000m, CostPrice = 5000m, Qty = 10 }, 1);
        Assert.Equal(10000m, vm.SubTotalAmount);
        Assert.Equal(10000m, vm.TotalAmount);

        vm.ReceivedAmountString = "5000";
        Assert.Equal(-5000m, vm.ChangeAmount);
        Assert.False(vm.ConfirmCashPaymentCommand.CanExecute(null));

        vm.ReceivedAmountString = "15000";
        Assert.Equal(5000m, vm.ChangeAmount);
        Assert.True(vm.ConfirmCashPaymentCommand.CanExecute(null));
    }

    [Fact]
    public void CheckoutCommand_DisabledWhenTotalAmountIsZero()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T003", Description = "Free", Price = 1000m, CostPrice = 500m, Qty = 10 }, 1);
        vm.DiscountPercentageString = "100";

        Assert.Equal(0m, vm.TotalAmount);
        Assert.False(vm.CheckoutCommand.CanExecute(null));
    }
}
