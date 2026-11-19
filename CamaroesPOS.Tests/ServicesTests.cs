using System;
using System.Collections.Generic;
using System.IO;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.ViewModels;
using Xunit;

namespace CamaroesPOS.Tests;

public class ServicesTests
{
    [Fact]
    public void SessionServiceSetsAndClearsUserRole()
    {
        SessionService.SetSession(new User { Username = "admin", Name = "Administrateur", Role = "Admin" });
        Assert.True(SessionService.IsAdmin);
        Assert.Equal("admin", SessionService.CurrentUser);
        Assert.Equal("Administrateur", SessionService.CurrentUserName);

        SessionService.SetSession(new User { Username = "caissier1", Name = "Caissier 1", Role = "Cashier" });
        Assert.False(SessionService.IsAdmin);
        Assert.Equal("Cashier", SessionService.CurrentUserRole);

        SessionService.Clear();
        Assert.Null(SessionService.CurrentUser);
        Assert.Null(SessionService.CurrentUserName);
        Assert.Null(SessionService.CurrentUserRole);
        Assert.False(SessionService.IsAdmin);
    }

    [Fact]
    public void RelayCommandRaisesCanExecuteChanged()
    {
        var raised = false;
        var cmd = new RelayCommand<object?>(_ => { });
        cmd.CanExecuteChanged += (_, _) => raised = true;

        cmd.RaiseCanExecuteChanged();

        Assert.True(raised);
    }

    [Fact]
    public void PdfServiceGeneratesNonEmptyReceiptPdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var pdfService = new PdfService();
        var tempPath = Path.Combine(Path.GetTempPath(), $"camaroes_test_{Guid.NewGuid():N}.pdf");

        try
        {
            var items = new List<CartItem>
            {
                new() { ProductCode = "P001", ProductDescription = "Laptop Gamer", Qty = 1, Price = 1500000, Total = 1500000 }
            };

            pdfService.GenerateReceiptPdf("T001", "Admin", 1500000, 1500000, 0, "0601020304", items, tempPath, 0);

            Assert.True(File.Exists(tempPath));
            var bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length > 100);
            Assert.Equal((byte)'%', bytes[0]);
            Assert.Equal((byte)'P', bytes[1]);
            Assert.Equal((byte)'D', bytes[2]);
            Assert.Equal((byte)'F', bytes[3]);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void PdfServiceGeneratesNonEmptySalesReportPdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var pdfService = new PdfService();
        var tempPath = Path.Combine(Path.GetTempPath(), $"camaroes_report_{Guid.NewGuid():N}.pdf");

        try
        {
            var sold = new List<SoldItem>
            {
                new() { TransNo = "T001", ProductCode = "P001", Description = "Laptop Gamer", Qty = 1, Price = 1500000, Total = 1500000, SoldDate = DateTime.Now, Cashier = "Admin", PaymentMode = "Espèces" }
            };

            pdfService.GenerateSalesReportPdf(DateTime.Today.AddDays(-7), DateTime.Today, 1500000, 0, 1, sold, tempPath);

            Assert.True(File.Exists(tempPath));
            var bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length > 100);
            Assert.Equal((byte)'%', bytes[0]);
            Assert.Equal((byte)'P', bytes[1]);
            Assert.Equal((byte)'D', bytes[2]);
            Assert.Equal((byte)'F', bytes[3]);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
