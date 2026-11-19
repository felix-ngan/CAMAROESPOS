using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Material.Icons;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class MessageDialogWindow : Window
{
    public MessageDialogWindow()
    {
        InitializeComponent();
    }

    public MessageDialogWindow(string title, string message, MaterialIconKind icon, string accentHex, bool isConfirm) : this()
    {
        Title = title;
        TxtTitle.Text = title;
        TxtMessage.Text = message;
        IconGlyph.Kind = icon;
        IconGlyph.Foreground = SolidColorBrush.Parse(accentHex);

        if (isConfirm)
        {
            BtnNo.IsVisible = true;
            BtnYes.Content = "Oui";
        }
        else
        {
            BtnNo.IsVisible = false;
            BtnYes.Content = "OK";
        }
    }

    private void BtnYes_Click(object? sender, RoutedEventArgs e) => Close(true);

    private void BtnNo_Click(object? sender, RoutedEventArgs e) => Close(false);
}
