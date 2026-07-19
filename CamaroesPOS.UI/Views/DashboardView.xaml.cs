using System.Windows.Controls;
using CamaroesPOS.UI.ViewModels;

namespace CamaroesPOS.UI.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void BtnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.LoadStats();
            }
        }
    }
}
