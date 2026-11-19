using Avalonia.Controls;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }
}
