using Avalonia.Controls;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class ExpenseView : UserControl
{
    public ExpenseView()
    {
        InitializeComponent();
        DataContext = new ExpenseViewModel();
    }
}
