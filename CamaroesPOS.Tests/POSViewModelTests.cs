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

        vm.DiscountPercentage = 20;
        Assert.Equal(20, vm.DiscountPercentage);
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
}
