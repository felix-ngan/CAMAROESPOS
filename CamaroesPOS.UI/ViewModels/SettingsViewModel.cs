using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CamaroesPOS.Domain.Entities;
using CamaroesPOS.Data;

namespace CamaroesPOS.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<User> Users { get; set; }
        
        private User _selectedUser;
        public User SelectedUser
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

        // Form for User
        private string _newUsername;
        public string NewUsername { get => _newUsername; set { _newUsername = value; OnPropertyChanged(); } }
        private string _newPassword;
        public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }
        private string _newName;
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }
        private string _newRole = "Cashier";
        public string NewRole { get => _newRole; set { _newRole = value; OnPropertyChanged(); } }
        private bool _isActiveUser = true;
        public bool IsActiveUser { get => _isActiveUser; set { _isActiveUser = value; OnPropertyChanged(); } }

        public ICommand SaveUserCommand { get; }
        public ICommand ClearUserCommand { get; }

        // Form for System Settings (Store)
        private string _storeName;
        public string StoreName { get => _storeName; set { _storeName = value; OnPropertyChanged(); } }
        private string _storeAddress;
        public string StoreAddress { get => _storeAddress; set { _storeAddress = value; OnPropertyChanged(); } }
        private string _momoCode;
        public string MomoCode { get => _momoCode; set { _momoCode = value; OnPropertyChanged(); } }
        private string _orangeCode;
        public string OrangeCode { get => _orangeCode; set { _orangeCode = value; OnPropertyChanged(); } }

        public ICommand SaveStoreCommand { get; }

        // Form for My Profile (Change Password)
        private string _oldPassword;
        public string OldPassword { get => _oldPassword; set { _oldPassword = value; OnPropertyChanged(); } }
        private string _profileNewPassword;
        public string ProfileNewPassword { get => _profileNewPassword; set { _profileNewPassword = value; OnPropertyChanged(); } }
        private string _profileConfirmPassword;
        public string ProfileConfirmPassword { get => _profileConfirmPassword; set { _profileConfirmPassword = value; OnPropertyChanged(); } }

        public ICommand ChangePasswordCommand { get; }

        public SettingsViewModel()
        {
            Users = new ObservableCollection<User>();
            
            SaveUserCommand = new RelayCommand<object>(SaveUser, CanSaveUser);
            ClearUserCommand = new RelayCommand<object>(ClearUser);
            SaveStoreCommand = new RelayCommand<object>(SaveStore, CanSaveStore);
            ChangePasswordCommand = new RelayCommand<object>(ChangePassword);

            LoadData();
        }

        private void LoadData()
        {
            using (var context = new PosDbContext())
            {
                Users.Clear();
                foreach (var u in context.Users.ToList())
                {
                    Users.Add(u);
                }

                var store = context.Stores.FirstOrDefault();
                if (store != null)
                {
                    StoreName = store.StoreName;
                    StoreAddress = store.Address;
                    MomoCode = store.MomoCode;
                    OrangeCode = store.OrangeCode;
                }
                else
                {
                    StoreName = "CAMAROES POS";
                    StoreAddress = "Douala, Cameroun";
                    MomoCode = "";
                    OrangeCode = "";
                }
            }
        }

        private bool CanSaveUser(object obj)
        {
            return !string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewPassword);
        }

        private void SaveUser(object obj)
        {
            using (var context = new PosDbContext())
            {
                var existing = context.Users.FirstOrDefault(u => u.Username == NewUsername);
                if (existing != null)
                {
                    existing.Name = NewName;
                    existing.Role = NewRole;
                    existing.Password = NewPassword;
                    existing.IsActive = IsActiveUser;
                }
                else
                {
                    context.Users.Add(new User
                    {
                        Username = NewUsername,
                        Name = NewName ?? NewUsername,
                        Role = NewRole,
                        Password = NewPassword,
                        IsActive = IsActiveUser
                    });
                }
                context.SaveChanges();
            }
            MessageBox.Show("Utilisateur enregistré avec succès !", "Paramètres", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearUser(null);
            LoadData();
        }

        private void ClearUser(object obj)
        {
            SelectedUser = null;
            NewUsername = "";
            NewPassword = "";
            NewName = "";
            NewRole = "Cashier";
            IsActiveUser = true;
        }

        private bool CanSaveStore(object obj)
        {
            return !string.IsNullOrWhiteSpace(StoreName);
        }

        private void SaveStore(object obj)
        {
            using (var context = new PosDbContext())
            {
                var store = context.Stores.FirstOrDefault();
                if (store != null)
                {
                    store.StoreName = StoreName;
                    store.Address = StoreAddress;
                    store.MomoCode = MomoCode;
                    store.OrangeCode = OrangeCode;
                }
                else
                {
                    context.Stores.Add(new Store
                    {
                        StoreName = StoreName,
                        Address = StoreAddress,
                        MomoCode = MomoCode,
                        OrangeCode = OrangeCode,
                        Logo = new byte[0]
                    });
                }
                context.SaveChanges();
            }
            MessageBox.Show("Paramètres système enregistrés !", "Paramètres", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangePassword(object obj)
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(ProfileNewPassword) || string.IsNullOrWhiteSpace(ProfileConfirmPassword))
            {
                MessageBox.Show("Veuillez remplir tous les champs de mot de passe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProfileNewPassword != ProfileConfirmPassword)
            {
                MessageBox.Show("Le nouveau mot de passe et la confirmation ne correspondent pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Application.Current.Properties.Contains("CurrentUserName"))
            {
                string username = Application.Current.Properties["CurrentUserName"].ToString();
                using (var context = new PosDbContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username && u.Password == OldPassword);
                    if (user == null)
                    {
                        MessageBox.Show("L'ancien mot de passe est incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    user.Password = ProfileNewPassword;
                    context.SaveChanges();
                    MessageBox.Show("Votre mot de passe a été modifié avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    OldPassword = "";
                    ProfileNewPassword = "";
                    ProfileConfirmPassword = "";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
