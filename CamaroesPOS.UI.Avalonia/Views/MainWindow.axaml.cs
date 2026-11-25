using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly DashboardView _dashboardView;
    private readonly POSView _posView;
    private readonly InventoryView _inventoryView;
    private readonly WalletView _walletView;
    private readonly VendorView _vendorView;
    private readonly ReportsView _reportsView;
    private readonly ExpenseView _expenseView;
    private readonly SettingsView _settingsView;

    public MainWindow()
    {
        InitializeComponent();
        _dashboardView = new DashboardView();
        _posView = new POSView();
        _inventoryView = new InventoryView();
        _walletView = new WalletView();
        _vendorView = new VendorView();
        _reportsView = new ReportsView();
        _expenseView = new ExpenseView();
        _settingsView = new SettingsView();

        if (!string.IsNullOrEmpty(SessionService.CurrentUserName))
        {
            var role = SessionService.CurrentUserRole ?? "Inconnu";
            TxtUsername.Text = $"Connecté : {SessionService.CurrentUserName} ({role})";
        }

        SyncLanguageButton();

        if (string.Equals(SessionService.CurrentUserRole, "Cashier", StringComparison.OrdinalIgnoreCase))
        {
            BtnDashboard.IsVisible = false;
            BtnSettings.IsVisible = false;
            BtnExpenses.IsVisible = false;
            BtnPOS_Click(null, null);
        }
        else
        {
            ShowDashboard();
        }
    }

    private void SetActiveNav(Button active)
    {
        var buttons = new[] { BtnDashboard, BtnPOS, BtnInventory, BtnWallet, BtnVendors, BtnReports, BtnExpenses, BtnSettings };
        foreach (var b in buttons)
        {
            b.Classes.Set("Active", b == active);
        }
    }

    private void ShowDashboard()
    {
        SetActiveNav(BtnDashboard);
        TxtHeader.Text = "";
        if (_dashboardView.DataContext is ViewModels.DashboardViewModel vm)
        {
            vm.LoadStats();
        }
        MainContentControl.Content = _dashboardView;
    }

    private void BtnDashboard_Click(object? sender, RoutedEventArgs? e) => ShowDashboard();

    private void BtnPOS_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnPOS);
        TxtHeader.Text = "Point de Vente";
        if (_posView.DataContext is ViewModels.POSViewModel vm)
        {
            vm.LoadDataFromDatabase();
        }
        MainContentControl.Content = _posView;
    }

    private void BtnInventory_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnInventory);
        TxtHeader.Text = "Gestion des Stocks";
        if (_inventoryView.DataContext is ViewModels.InventoryViewModel vm)
        {
            vm.LoadDataFromDatabase();
        }
        MainContentControl.Content = _inventoryView;
    }

    private void BtnWallet_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnWallet);
        TxtHeader.Text = "Gestion des Clients (CRM)";
        if (_walletView.DataContext is ViewModels.WalletViewModel vm) vm.LoadDataFromDatabase();
        MainContentControl.Content = _walletView;
    }

    private void BtnVendors_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnVendors);
        TxtHeader.Text = "Gestion des Fournisseurs";
        if (_vendorView.DataContext is ViewModels.VendorViewModel vm) vm.LoadVendors();
        MainContentControl.Content = _vendorView;
    }

    private void BtnReports_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnReports);
        TxtHeader.Text = "Rapports & Ventes";
        if (_reportsView.DataContext is ViewModels.ReportsViewModel vm) vm.LoadTransactions();
        MainContentControl.Content = _reportsView;
    }

    private void BtnExpenses_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnExpenses);
        TxtHeader.Text = "Dépenses & Charges";
        if (_expenseView.DataContext is ViewModels.ExpenseViewModel vm) vm.LoadExpenses();
        MainContentControl.Content = _expenseView;
    }

    private void BtnSettings_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnSettings);
        TxtHeader.Text = "Paramètres Généraux";
        if (_settingsView.DataContext is ViewModels.SettingsViewModel vm) vm.LoadData();
        MainContentControl.Content = _settingsView;
    }

    private void BtnLogout_Click(object? sender, RoutedEventArgs e)
    {
        SessionService.Clear();
        var loginWin = new LoginWindow();
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = loginWin;
        }
        loginWin.Show();
        Close();
    }

    private void SyncLanguageButton()
    {
        if (global::Avalonia.Application.Current == null) return;
        var current = global::Avalonia.Application.Current.Resources.MergedDictionaries
            .OfType<ResourceInclude>()
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Resources/Langs/Strings."));

        bool isEnglish = current?.Source?.OriginalString.EndsWith("Strings.en.axaml", StringComparison.OrdinalIgnoreCase) == true;
        BtnLangToggle.Content = isEnglish ? "FR" : "EN";
    }

    private void BtnLangToggle_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || global::Avalonia.Application.Current == null) return;

        string targetLang = btn.Content?.ToString() == "EN" ? "en" : "fr";
        btn.Content = targetLang == "en" ? "FR" : "EN";

        var merged = global::Avalonia.Application.Current.Resources.MergedDictionaries;
        var existing = merged
            .OfType<ResourceInclude>()
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Resources/Langs/Strings."));

        if (existing != null)
        {
            merged.Remove(existing);
        }

        var uri = new Uri($"avares://CamaroesPOS.UI.Avalonia/Resources/Langs/Strings.{targetLang}.axaml");
        merged.Add(new ResourceInclude(uri) { Source = uri });
    }
}
