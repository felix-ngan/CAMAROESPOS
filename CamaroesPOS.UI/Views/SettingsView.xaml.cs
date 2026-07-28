using System.Windows;
using System.Windows.Controls;
using CamaroesPOS.UI.ViewModels;

namespace CamaroesPOS.UI.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void OldPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((SettingsViewModel)this.DataContext).OldPassword = ((PasswordBox)sender).Password; }
        }

        private void NewPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((SettingsViewModel)this.DataContext).ProfileNewPassword = ((PasswordBox)sender).Password; }
        }

        private void ConfirmPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((SettingsViewModel)this.DataContext).ProfileConfirmPassword = ((PasswordBox)sender).Password; }
        }
    }
}
