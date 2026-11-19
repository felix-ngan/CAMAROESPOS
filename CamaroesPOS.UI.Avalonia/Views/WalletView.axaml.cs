using Avalonia.Controls;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class WalletView : UserControl
{
    public WalletView()
    {
        InitializeComponent();
        DataContext = new WalletViewModel();
    }
}
