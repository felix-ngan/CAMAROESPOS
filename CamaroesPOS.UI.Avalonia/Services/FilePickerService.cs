using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace CamaroesPOS.UI.Avalonia.Services;

public static class FilePickerService
{
    public static async Task<string?> PickSavePdfPathAsync(string suggestedFileName, string title = "Exporter en PDF")
    {
        var window = GetActiveWindow();
        if (window?.StorageProvider == null) return null;

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "pdf",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Documents PDF") { Patterns = new[] { "*.pdf" } }
            }
        });

        return file?.TryGetLocalPath();
    }

    public static void OpenFileWithDefaultApp(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", new[] { path });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", new[] { path });
        }
        else
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }

    private static Window? GetActiveWindow()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var w in desktop.Windows)
            {
                if (w.IsActive) return w;
            }
            return desktop.MainWindow;
        }
        return null;
    }
}
