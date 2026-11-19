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

        vm.DiscountPercentage = -15;
        Assert.Equal(0, vm.DiscountPercentage);

        vm.DiscountPercentage = 150;
        Assert.Equal(100, vm.DiscountPercentage);

        vm.DiscountPercentage = double.NaN;
        Assert.Equal(0, vm.DiscountPercentage);

        vm.DiscountPercentage = 20;
        Assert.Equal(20, vm.DiscountPercentage);
    }

    [Fact]
    public void DiscountPercentageString_EmptyResetsDiscountToZero()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T001", Description = "Test", Price = 10000, CostPrice = 5000, Qty = 10 }, 1);

        vm.DiscountPercentageString = "10";
        Assert.Equal(10, vm.DiscountPercentage);
        Assert.Equal(9000, vm.TotalAmount);

        vm.DiscountPercentageString = "";
        Assert.Equal(0, vm.DiscountPercentage);
        Assert.Equal(10000, vm.TotalAmount);
    }

    [Fact]
    public void ExactAmountCommand_OnFractionalDiscountedTotal_EnablesConfirmCashPayment()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T002", Description = "Item", Price = 105, CostPrice = 50, Qty = 10 }, 1);
        vm.DiscountPercentageString = "15"; // 105 * 0.85 = 89.25

        vm.ExactAmountCommand.Execute(null);

        Assert.True(vm.ChangeAmount >= 0);
        Assert.True(vm.ConfirmCashPaymentCommand.CanExecute(null));
    }

    [Fact]
    public void ChangeAmount_CalculatesCorrectly_AndGuardsConfirmCashPayment()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();

        vm.AddProductToCart(new Product { ProductCode = "T001", Description = "Test Item", Price = 10000, CostPrice = 5000, Qty = 10 }, 1);
        Assert.Equal(10000, vm.SubTotalAmount);
        Assert.Equal(10000, vm.TotalAmount);

        vm.ReceivedAmountString = "5000";
        Assert.Equal(-5000, vm.ChangeAmount);
        Assert.False(vm.ConfirmCashPaymentCommand.CanExecute(null));

        vm.ReceivedAmountString = "15000";
        Assert.Equal(5000, vm.ChangeAmount);
        Assert.True(vm.ConfirmCashPaymentCommand.CanExecute(null));
    }

    [Fact]
    public void CheckoutCommand_DisabledWhenTotalAmountIsZero()
    {
        DatabaseInitializer.Initialize();
        var vm = new POSViewModel();
        vm.AddProductToCart(new Product { ProductCode = "T003", Description = "Free", Price = 1000, CostPrice = 500, Qty = 10 }, 1);
        vm.DiscountPercentageString = "100";

        Assert.Equal(0, vm.TotalAmount);
        Assert.False(vm.CheckoutCommand.CanExecute(null));
    }
}
