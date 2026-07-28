using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CamaroesPOS.Data;
using System.Windows.Controls;

namespace CamaroesPOS.UI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>(Login);
            CloseCommand = new RelayCommand<object>(Close);
        }

        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez saisir un nom d'utilisateur et un mot de passe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new PosDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == Username && u.Password == password && u.IsActive);
                if (user != null)
                {
                    Application.Current.Properties["CurrentUser"] = user.Username;
                    Application.Current.Properties["CurrentUserRole"] = user.Role;
                    Application.Current.Properties["CurrentUserName"] = user.Name;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    var loginWin = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Name == "LoginWin");
                    if (loginWin != null) loginWin.Close();
                }
                else
                {
                    MessageBox.Show("Identifiants incorrects ou compte désactivé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close(object obj)
        {
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
