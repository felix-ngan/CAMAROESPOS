using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CamaroesPOS.Application.Security;
using CamaroesPOS.Data;
using CamaroesPOS.UI.Avalonia.Services;

namespace CamaroesPOS.UI.Avalonia.Views;

public partial class ChangePasswordWindow : Window
{
    private readonly string _username;

    public ChangePasswordWindow() : this(SessionService.CurrentUser ?? string.Empty)
    {
    }

    public ChangePasswordWindow(string username)
    {
        InitializeComponent();
        _username = username;
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e) => Close();

    private async void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        string oldPass = TxtOldPassword.Text ?? string.Empty;
        string newPass = TxtNewPassword.Text ?? string.Empty;
        string confPass = TxtConfirmPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
        {
            TxtError.Text = "Tous les champs sont requis.";
            TxtError.IsVisible = true;
            return;
        }

        if (newPass != confPass)
        {
            TxtError.Text = "Le nouveau mot de passe et la confirmation ne correspondent pas.";
            TxtError.IsVisible = true;
            return;
        }

        using (var context = new PosDbContext())
        {
            var user = context.Users.FirstOrDefault(u => u.Username == _username);
            if (user == null || !PasswordHasher.Verify(oldPass, user.Password))
            {
                TxtError.Text = "L'ancien mot de passe est incorrect.";
                TxtError.IsVisible = true;
                return;
            }

            user.Password = PasswordHasher.Hash(newPass);
            context.SaveChanges();
        }

        await DialogService.ShowInfoAsync("Mot de passe modifié avec succès !", "Succès");
        Close();
    }
}
