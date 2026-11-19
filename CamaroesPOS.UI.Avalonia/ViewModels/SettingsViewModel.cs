using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.UI.Avalonia.Services;
using CamaroesPOS.UI.Avalonia.Views;
using Microsoft.EntityFrameworkCore;

namespace CamaroesPOS.UI.Avalonia.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<string> Roles { get; } = new() { "Admin", "Cashier" };

    private User? _selectedUser;
    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
            if (_selectedUser != null)
            {
                NewUsername = _selectedUser.Username;
                NewName = _selectedUser.Name;
                NewRole = _selectedUser.Role;
                NewPassword = _selectedUser.Password;
                IsActiveUser = _selectedUser.IsActive;
            }
        }
    }

    private string _newUsername = string.Empty;
    public string NewUsername
    {
        get => _newUsername;
        set { _newUsername = value; OnPropertyChanged(); SaveUserCommand.RaiseCanExecuteChanged(); }
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); SaveUserCommand.RaiseCanExecuteChanged(); }
    }

    private string _newName = string.Empty;
    public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

    private string _newRole = "Cashier";
    public string NewRole { get => _newRole; set { _newRole = value; OnPropertyChanged(); } }

    private bool _isActiveUser = true;
    public bool IsActiveUser { get => _isActiveUser; set { _isActiveUser = value; OnPropertyChanged(); } }

    private string _storeName = "CAMAROES POS";
    public string StoreName
    {
        get => _storeName;
        set { _storeName = value; OnPropertyChanged(); SaveStoreCommand.RaiseCanExecuteChanged(); }
    }

    private string _storeAddress = "Douala, Cameroun";
    public string StoreAddress { get => _storeAddress; set { _storeAddress = value; OnPropertyChanged(); } }

    private string _momoCode = string.Empty;
    public string MomoCode { get => _momoCode; set { _momoCode = value; OnPropertyChanged(); } }

    private string _orangeCode = string.Empty;
    public string OrangeCode { get => _orangeCode; set { _orangeCode = value; OnPropertyChanged(); } }

    private string _oldPassword = string.Empty;
    public string OldPassword { get => _oldPassword; set { _oldPassword = value; OnPropertyChanged(); } }

    private string _profileNewPassword = string.Empty;
    public string ProfileNewPassword { get => _profileNewPassword; set { _profileNewPassword = value; OnPropertyChanged(); } }

    private string _profileConfirmPassword = string.Empty;
    public string ProfileConfirmPassword { get => _profileConfirmPassword; set { _profileConfirmPassword = value; OnPropertyChanged(); } }

    public RelayCommand<object?> SaveUserCommand { get; }
    public RelayCommand<object?> ClearUserCommand { get; }
    public RelayCommand<object?> SaveStoreCommand { get; }
    public RelayCommand<object?> ChangePasswordCommand { get; }
    public RelayCommand<object?> OpenChangePasswordWindowCommand { get; }

    public SettingsViewModel()
    {
        SaveUserCommand = new RelayCommand<object?>(_ => SaveUserAsync(), _ => !string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewPassword));
        ClearUserCommand = new RelayCommand<object?>(_ => ClearUser());
        SaveStoreCommand = new RelayCommand<object?>(_ => SaveStoreAsync(), _ => !string.IsNullOrWhiteSpace(StoreName));
        ChangePasswordCommand = new RelayCommand<object?>(_ => ChangePasswordAsync());
        OpenChangePasswordWindowCommand = new RelayCommand<object?>(_ => OpenChangePasswordWindowAsync());

        LoadData();
    }

    public void LoadData()
    {
        using var context = new PosDbContext();
        Users.Clear();
        foreach (var u in context.Users.ToList()) Users.Add(u);

        try
        {
            var store = context.Stores.FirstOrDefault();
            if (store != null)
            {
                StoreName = store.StoreName ?? "CAMAROES POS";
                StoreAddress = store.Address ?? "Douala, Cameroun";
                MomoCode = store.MomoCode ?? string.Empty;
                OrangeCode = store.OrangeCode ?? string.Empty;
            }
        }
        catch
        {
            // Stores table may be empty or not initialized
        }
    }

    private async void SaveUserAsync()
    {
        using (var context = new PosDbContext())
        {
            var existing = context.Users.FirstOrDefault(u => u.Username == NewUsername);
            if (existing != null)
            {
                existing.Name = string.IsNullOrWhiteSpace(NewName) ? NewUsername : NewName;
                existing.Role = NewRole;
                existing.Password = NewPassword;
                existing.IsActive = IsActiveUser;
            }
            else
            {
                context.Users.Add(new User
                {
                    Username = NewUsername,
                    Name = string.IsNullOrWhiteSpace(NewName) ? NewUsername : NewName,
                    Role = NewRole,
                    Password = NewPassword,
                    IsActive = IsActiveUser
                });
            }
            context.SaveChanges();
        }
        await DialogService.ShowInfoAsync("Utilisateur enregistré avec succès !", "Paramètres");
        ClearUser();
        LoadData();
    }

    private void ClearUser()
    {
        SelectedUser = null;
        NewUsername = string.Empty;
        NewPassword = string.Empty;
        NewName = string.Empty;
        NewRole = "Cashier";
        IsActiveUser = true;
    }

    private async void SaveStoreAsync()
    {
        using (var context = new PosDbContext())
        {
            context.Database.ExecuteSqlRaw("DELETE FROM Stores;");
            context.Database.ExecuteSqlRaw(
                "INSERT INTO Stores (StoreName, Address, Logo, MomoCode, OrangeCode) VALUES ({0}, {1}, X'', {2}, {3});",
                StoreName ?? "",
                StoreAddress ?? "",
                MomoCode ?? "",
                OrangeCode ?? "");
        }
        await DialogService.ShowInfoAsync("Paramètres système enregistrés !", "Paramètres");
    }

    private async void ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(ProfileNewPassword) || string.IsNullOrWhiteSpace(ProfileConfirmPassword))
        {
            await DialogService.ShowWarningAsync("Veuillez remplir tous les champs de mot de passe.", "Erreur");
            return;
        }
        if (ProfileNewPassword != ProfileConfirmPassword)
        {
            await DialogService.ShowWarningAsync("Le nouveau mot de passe et la confirmation ne correspondent pas.", "Erreur");
            return;
        }

        var username = SessionService.CurrentUser;
        if (string.IsNullOrWhiteSpace(username)) return;

        using (var context = new PosDbContext())
        {
            var user = context.Users.FirstOrDefault(u => u.Username == username && u.Password == OldPassword);
            if (user == null)
            {
                await DialogService.ShowErrorAsync("L'ancien mot de passe est incorrect.", "Erreur");
                return;
            }
            user.Password = ProfileNewPassword;
            context.SaveChanges();
        }

        await DialogService.ShowInfoAsync("Votre mot de passe a été modifié avec succès !", "Succès");
        OldPassword = string.Empty;
        ProfileNewPassword = string.Empty;
        ProfileConfirmPassword = string.Empty;
    }

    private async void OpenChangePasswordWindowAsync()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var win = new ChangePasswordWindow(SessionService.CurrentUser ?? string.Empty);
            await win.ShowDialog(desktop.MainWindow);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
