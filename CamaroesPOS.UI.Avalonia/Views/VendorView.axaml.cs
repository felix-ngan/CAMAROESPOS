using Avalonia.Controls;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class VendorView : UserControl
{
    public VendorView()
    {
        InitializeComponent();
        DataContext = new VendorViewModel();
    }
}
