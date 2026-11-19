using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        DatabaseInitializer.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Window
            {
                Title = "CAMAROES POS - Avalonia",
                Width = 900,
                Height = 600
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
