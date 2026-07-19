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

        public MainWindow()
        {
            InitializeComponent();
            _posView = new POSView();
            _inventoryView = new InventoryView();
            _walletView = new WalletView();

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
            
            var dashboardPlaceholder = new TextBlock
            {
                Text = "Résumé des ventes du jour à venir ici...",
                FontSize = 20,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            MainContentControl.Content = dashboardPlaceholder;
        }
    }
}