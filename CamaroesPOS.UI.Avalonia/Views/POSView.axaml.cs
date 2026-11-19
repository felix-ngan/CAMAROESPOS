using Avalonia.Controls;
using Avalonia.Input;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class POSView : UserControl
{
    public POSView()
    {
        InitializeComponent();
        DataContext = new POSViewModel();
    }

    private void BarcodeBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is POSViewModel vm)
        {
            vm.SearchBarcodeCommand.Execute(null);
        }
    }
}
