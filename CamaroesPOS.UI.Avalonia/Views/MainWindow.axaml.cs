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

    public MainWindow()
    {
        InitializeComponent();
        _dashboardView = new DashboardView();

        if (!string.IsNullOrEmpty(SessionService.CurrentUserName))
        {
            var role = SessionService.CurrentUserRole ?? "Inconnu";
            TxtUsername.Text = $"Connecté : {SessionService.CurrentUserName} ({role})";
        }

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

    private static Control MakePlaceholder(string title) => new Border
    {
        Classes = { "Card" },
        Child = new TextBlock
        {
            Text = $"{title} — en cours de chargement...",
            FontSize = 16,
            Foreground = SolidColorBrush.Parse("#8F9BBA"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        }
    };

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
        MainContentControl.Content = MakePlaceholder("Point de Vente");
    }

    private void BtnInventory_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnInventory);
        TxtHeader.Text = "Gestion des Stocks";
        MainContentControl.Content = MakePlaceholder("Gestion des Stocks");
    }

    private void BtnWallet_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnWallet);
        TxtHeader.Text = "Gestion des Clients (CRM)";
        MainContentControl.Content = MakePlaceholder("Clients CRM");
    }

    private void BtnVendors_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnVendors);
        TxtHeader.Text = "Gestion des Fournisseurs";
        MainContentControl.Content = MakePlaceholder("Fournisseurs");
    }

    private void BtnReports_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnReports);
        TxtHeader.Text = "Rapports & Ventes";
        MainContentControl.Content = MakePlaceholder("Rapports & Ventes");
    }

    private void BtnExpenses_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnExpenses);
        TxtHeader.Text = "Dépenses & Charges";
        MainContentControl.Content = MakePlaceholder("Dépenses & Charges");
    }

    private void BtnSettings_Click(object? sender, RoutedEventArgs? e)
    {
        SetActiveNav(BtnSettings);
        TxtHeader.Text = "Paramètres Généraux";
        MainContentControl.Content = MakePlaceholder("Paramètres Généraux");
    }

    private void BtnLogout_Click(object? sender, RoutedEventArgs e)
    {
        SessionService.Clear();
        var loginWin = new LoginWindow();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = loginWin;
        }
        loginWin.Show();
        Close();
    }

    private void BtnLangToggle_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || Application.Current == null) return;

        string targetLang = btn.Content?.ToString() == "EN" ? "en" : "fr";
        btn.Content = targetLang == "en" ? "FR" : "EN";

        var merged = Application.Current.Resources.MergedDictionaries;
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
