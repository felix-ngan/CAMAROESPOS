using Avalonia.Controls;
using CamaroesPOS.UI.Avalonia.ViewModels;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class BrandCategoryWindow : Window
{
    public BrandCategoryWindow()
    {
        InitializeComponent();
        DataContext = new BrandCategoryViewModel();
    }
}
