using System.Windows;
using System.Windows.Controls;
using CamaroesPOS.UI.Views;

namespace CamaroesPOS.UI
{
    public partial class MainWindow : Window
    {
        private POSView _posView;
        private InventoryView _inventoryView;
        private WalletView _walletView;
        private DashboardView _dashboardView;
        private ReportsView _reportsView;
        private SettingsView _settingsView;
        private VendorView _vendorView;

        public MainWindow()
        {
            InitializeComponent();
            _posView = new POSView();
            _inventoryView = new InventoryView();
            _walletView = new WalletView();
            _dashboardView = new DashboardView();
            _reportsView = new ReportsView();
            _settingsView = new SettingsView();
            _vendorView = new VendorView();

            if (Application.Current.Properties.Contains("CurrentUserName"))
            {
                TxtUsername.Text = $"Connecté : {Application.Current.Properties["CurrentUserName"]}";
            }

            if (Application.Current.Properties.Contains("CurrentUserRole"))
            {
                var role = Application.Current.Properties["CurrentUserRole"].ToString();
                if (role == "Cashier")
                {
                    BtnDashboard.Visibility = System.Windows.Visibility.Collapsed;
                    BtnReports.Visibility = System.Windows.Visibility.Collapsed;
                    BtnInventory.Visibility = System.Windows.Visibility.Collapsed;
                    BtnSettings.Visibility = System.Windows.Visibility.Collapsed;
                    BtnVendors.Visibility = System.Windows.Visibility.Collapsed;
                    BtnPOS_Click(null, null); // Start directly on POS
                }
                else
                {
                    ShowDashboard(); // Admins start on Dashboard
                }
            }
            else
            {
                ShowDashboard();
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void BtnPOS_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Point de Vente";
            MainContentControl.Content = _posView;
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Gestion des Stocks";
            MainContentControl.Content = _inventoryView;
        }

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Portefeuilles Clients";
            MainContentControl.Content = _walletView;
        }

        private void BtnVendors_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Gestion des Fournisseurs";
            MainContentControl.Content = _vendorView;
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Rapports & Ventes";
            
            if (_reportsView.DataContext is CamaroesPOS.UI.ViewModels.ReportsViewModel vm)
            {
                vm.LoadTransactions(); // Recharger à l'ouverture
            }

            MainContentControl.Content = _reportsView;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Paramètres Généraux";
            MainContentControl.Content = _settingsView;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties.Remove("CurrentUser");
            Application.Current.Properties.Remove("CurrentUserRole");
            Application.Current.Properties.Remove("CurrentUserName");
            
            var loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
        }

        private void ShowDashboard()
        {
            TxtHeader.Text = "Tableau de Bord";
            
            if (_dashboardView.DataContext is CamaroesPOS.UI.ViewModels.DashboardViewModel vm)
            {
                vm.LoadStats(); // Force la mise à jour à chaque affichage
            }
            
            MainContentControl.Content = _dashboardView;
        }
    }
}