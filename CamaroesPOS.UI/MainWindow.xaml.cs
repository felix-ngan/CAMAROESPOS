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

        public MainWindow()
        {
            InitializeComponent();
            _posView = new POSView();
            _inventoryView = new InventoryView();
            _walletView = new WalletView();
            _dashboardView = new DashboardView();

            ShowDashboard();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void BtnPOS_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Caisse (Point of Sale)";
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