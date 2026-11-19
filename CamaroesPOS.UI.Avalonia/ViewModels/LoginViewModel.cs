using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CamaroesPOS.Data;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }
    public ICommand CloseCommand { get; }

    public Action? OnLoginSucceeded { get; set; }

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand<object?>(_ => LoginAsync());
        CloseCommand = new RelayCommand<object?>(_ => CloseApp());
    }

    private async void LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await DialogService.ShowWarningAsync("Veuillez saisir un nom d'utilisateur et un mot de passe.", "Erreur");
            return;
        }

        using var context = new PosDbContext();
        var user = context.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password && u.IsActive);
        if (user != null)
        {
            SessionService.SetSession(user);
            OnLoginSucceeded?.Invoke();
        }
        else
        {
            await DialogService.ShowErrorAsync("Identifiants incorrects ou compte désactivé.", "Erreur");
        }
    }

    private static void CloseApp()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
