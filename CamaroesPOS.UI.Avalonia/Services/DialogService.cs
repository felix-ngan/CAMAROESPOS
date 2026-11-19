using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CamaroesPOS.UI.Avalonia.Views;
using Material.Icons;

namespace CamaroesPOS.UI.Avalonia.Services;

public static class DialogService
{
    public static Task ShowInfoAsync(string message, string title = "Information")
        => ShowInternalAsync(title, message, MaterialIconKind.InformationOutline, "#1E88E5", false);

    public static Task ShowWarningAsync(string message, string title = "Attention")
        => ShowInternalAsync(title, message, MaterialIconKind.AlertOutline, "#FB8C00", false);

    public static Task ShowErrorAsync(string message, string title = "Erreur")
        => ShowInternalAsync(title, message, MaterialIconKind.AlertCircleOutline, "#E53935", false);

    public static Task<bool> ConfirmAsync(string message, string title = "Confirmation")
        => ShowInternalAsync(title, message, MaterialIconKind.HelpCircleOutline, "#FB8C00", true);

    private static async Task<bool> ShowInternalAsync(string title, string message, MaterialIconKind icon, string accentHex, bool isConfirm)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return false;

        Window? owner = null;
        foreach (var w in desktop.Windows)
        {
            if (w.IsActive)
            {
                owner = w;
                break;
            }
        }
        owner ??= desktop.MainWindow;
        if (owner == null) return false;

        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dlg = new MessageDialogWindow(title, message, icon, accentHex, isConfirm);
            var result = await dlg.ShowDialog<bool?>(owner);
            return result == true;
        });
    }
}
