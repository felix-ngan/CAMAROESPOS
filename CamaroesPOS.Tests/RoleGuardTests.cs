using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

public class RoleGuardTests
{
    [Fact]
    public void CashierCannotOpenEditFormsInInventoryVendorOrExpense()
    {
        DatabaseInitializer.Initialize();
        SessionService.SetSession(new User { Username = "caissier1", Name = "Caissier 1", Role = "Cashier" });

        try
        {
            var invVm = new InventoryViewModel();
            invVm.AddCommand.Execute(null);
            Assert.False(invVm.IsEditing);
            invVm.EditCommand.Execute(new Product { ProductCode = "P001", Description = "Test" });
            Assert.False(invVm.IsEditing);

            var vendorVm = new VendorViewModel();
            vendorVm.CreateVendorCommand.Execute(null);
            Assert.False(vendorVm.IsEditing);
            vendorVm.EditVendorCommand.Execute(new Vendor { Id = 1, VendorName = "V1" });
            Assert.False(vendorVm.IsEditing);

            var expenseVm = new ExpenseViewModel();
            expenseVm.CreateCommand.Execute(null);
            Assert.False(expenseVm.IsEditing);
            expenseVm.EditCommand.Execute(new Expense { Id = 1, Category = "Autre", Amount = 1000 });
            Assert.False(expenseVm.IsEditing);
        }
        finally
        {
            SessionService.Clear();
        }
    }
}
