using System.Linq;
using System.Windows;
using CamaroesPOS.Data;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private string _username;

        public ChangePasswordWindow(string username)
        {
            InitializeComponent();
            _username = username;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            string oldPass = TxtOldPassword.Password;
            string newPass = TxtNewPassword.Password;
            string confPass = TxtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                TxtError.Text = "Tous les champs sont requis.";
                TxtError.Visibility = Visibility.Visible;
                return;
            }

            if (newPass != confPass)
            {
                TxtError.Text = "Le nouveau mot de passe et la confirmation ne correspondent pas.";
                TxtError.Visibility = Visibility.Visible;
                return;
            }

            using (var context = new PosDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == _username && u.Password == oldPass);
                
                if (user == null)
                {
                    TxtError.Text = "L'ancien mot de passe est incorrect.";
                    TxtError.Visibility = Visibility.Visible;
                    return;
                }

                user.Password = newPass;
                context.SaveChanges();
            }

            MessageBox.Show("Mot de passe modifié avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
