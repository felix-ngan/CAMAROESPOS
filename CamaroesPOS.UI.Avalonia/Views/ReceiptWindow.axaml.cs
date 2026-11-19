using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class ReceiptWindow : Window
{
    private readonly string _transNo;
    private readonly IEnumerable<CartItem> _items;
    private readonly double _total;
    private readonly double _received;
    private readonly double _change;
    private readonly double _discount;
    private readonly string _customerPhone;

    public ReceiptWindow()
    {
        InitializeComponent();
        _transNo = string.Empty;
        _items = Array.Empty<CartItem>();
        _customerPhone = string.Empty;
    }

    public ReceiptWindow(string transNo, IEnumerable<CartItem> items, double total, double received, double change, string paymentMode, double discount = 0, string customerPhone = "") : this()
    {
        _transNo = transNo;
        _items = items;
        _total = total;
        _received = received;
        _change = change;
        _discount = discount;
        _customerPhone = customerPhone;

        TxtTransNo.Text = transNo;
        TxtDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        TxtPaymentMode.Text = paymentMode;
        ItemsList.ItemsSource = items;

        double subTotal = total + discount;
        bool hasDiscount = discount > 0;
        LblSubTotal.IsVisible = hasDiscount;
        TxtSubTotal.IsVisible = hasDiscount;
        TxtSubTotal.Text = $"{subTotal:N0} FCFA";
        LblDiscount.IsVisible = hasDiscount;
        TxtDiscount.IsVisible = hasDiscount;
        TxtDiscount.Text = $"-{discount:N0} FCFA";
        TxtTotal.Text = $"{total:N0} FCFA";

        bool isCash = string.Equals(paymentMode, "Espèces", StringComparison.OrdinalIgnoreCase);
        LblReceived.IsVisible = isCash;
        TxtReceived.IsVisible = isCash;
        LblChange.IsVisible = isCash;
        TxtChange.IsVisible = isCash;
        TxtReceived.Text = $"{received:N0} FCFA";
        TxtChange.Text = $"{change:N0} FCFA";
    }

    private async void BtnExportPdf_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var path = await FilePickerService.PickSavePdfPathAsync($"Recu_{_transNo}.pdf", "Exporter le reçu en PDF");
            if (string.IsNullOrWhiteSpace(path)) return;

            var cashier = SessionService.CurrentUserName ?? "Admin";
            new PdfService().GenerateReceiptPdf(_transNo, cashier, _total, _received, _change, _customerPhone, _items, path, _discount);
            await DialogService.ShowInfoAsync("Le reçu a été exporté avec succès !", "Succès");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Erreur lors de l'exportation PDF : {ex.Message}", "Erreur");
        }
    }

    private async void BtnPrint_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"CamaroesPOS_Recu_{_transNo}.pdf");
            var cashier = SessionService.CurrentUserName ?? "Admin";
            new PdfService().GenerateReceiptPdf(_transNo, cashier, _total, _received, _change, _customerPhone, _items, tempPath, _discount);
            FilePickerService.OpenFileWithDefaultApp(tempPath);
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Erreur lors de l'ouverture pour impression : {ex.Message}", "Erreur");
        }
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e) => Close();
}
