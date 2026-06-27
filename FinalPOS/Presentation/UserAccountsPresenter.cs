using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class UserAccountsPresenter
    {
        private readonly IUserAccountsView _view;
        private readonly IUserRepository _userRepository;

        public UserAccountsPresenter(IUserAccountsView view, IUserRepository userRepository)
        {
            _view = view;
            _userRepository = userRepository;
        }

        public void CreateAccount()
        {
            try
            {
                string username = _view.NewUsername.Trim();
                string password = _view.NewPassword;
                string confirmPass = _view.NewConfirmPassword;
                string role = _view.NewRole;
                string name = _view.NewFullName.Trim();

                if (string.IsNullOrEmpty(username))
                {
                    _view.ShowMessage("Le nom d'utilisateur est obligatoire.", "Champs requis", true);
                    return;
                }

                if (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(confirmPass))
                {
                    _view.ShowMessage("Le mot de passe est obligatoire.", "Champs requis", true);
                    return;
                }

                if (password != confirmPass)
                {
                    _view.ShowMessage("Les mots de passe ne correspondent pas.", "Erreur de confirmation du mot de passe", true);
                    return;
                }

                if (string.IsNullOrEmpty(role))
                {
                    _view.ShowMessage("Veuillez sélectionner un rôle.", "Champs requis", true);
                    return;
                }

                // DB Role Mapping for compatibility
                string dbRole = (role == "Administrateur Système") ? "System Administrator" : "Cashier";

                _userRepository.AddUser(username, password, dbRole, name);
                _view.ShowMessage("Utilisateur enregistré avec succès !", "Succès");
                _view.ClearCreateAccountFields();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void ChangePassword(string loggedInUserPassword)
        {
            try
            {
                string username = _view.CurrentUsername;
                string oldPassword = _view.OldPassword;
                string newPassword = _view.NewPasswordChange;
                string confirmNew = _view.ConfirmNewPassword;

                if (!PasswordHasher.VerifyPassword(oldPassword, loggedInUserPassword))
                {
                    _view.ShowMessage("ANCIEN MOT DE PASSE INCORRECT", "MOT DE PASSE INVALIDE", true);
                    return;
                }

                if (newPassword != confirmNew)
                {
                    _view.ShowMessage("LA CONFIRMATION DU MOT DE PASSE EST INCORRECTE", "MOT DE PASSE INVALIDE", true);
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    _view.ShowMessage("Le nouveau mot de passe ne peut pas être vide.", "MOT DE PASSE INVALIDE", true);
                    return;
                }

                _userRepository.UpdatePassword(username, newPassword);
                _view.ShowMessage("Le mot de passe a été modifié avec succès.", "SUCCÈS");
                _view.ClearChangePasswordFields();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void OnSearchUsernameChanged()
        {
            try
            {
                string searchUser = _view.SearchUsername.Trim();
                if (string.IsNullOrEmpty(searchUser))
                {
                    _view.IsAccountActive = false;
                    return;
                }

                if (_userRepository.UserExists(searchUser))
                {
                    _view.IsAccountActive = _userRepository.GetUserActivationStatus(searchUser);
                }
                else
                {
                    _view.IsAccountActive = false;
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void UpdateUserActivation()
        {
            try
            {
                string searchUser = _view.SearchUsername.Trim();
                if (string.IsNullOrEmpty(searchUser))
                {
                    _view.ShowMessage("Veuillez saisir un nom d'utilisateur.", "Attention", true);
                    return;
                }

                if (_userRepository.UserExists(searchUser))
                {
                    _userRepository.UpdateUserActivationStatus(searchUser, _view.IsAccountActive);
                    _view.ShowMessage("Le statut du compte a été mis à jour avec succès.", "SUCCÈS");
                    _view.ClearActivateFields();
                }
                else
                {
                    _view.ShowMessage("Ce compte n'existe pas.", "AVERTISSEMENT", true);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void ExecuteBackup()
        {
            try
            {
                string path = _view.BackupPath.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    _view.ShowMessage("Veuillez spécifier un dossier de sauvegarde.", "Attention", true);
                    return;
                }

                string backupFile = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd") + ".bak");
                backupFile = backupFile.Replace("\\", "/");

                bool performBackup = true;
                if (System.IO.File.Exists(backupFile))
                {
                    if (!_view.ConfirmMessage("Voulez-vous écraser le fichier existant ?", "Le fichier existe déjà"))
                    {
                        performBackup = false;
                    }
                }

                if (performBackup)
                {
                    _userRepository.BackupDatabase(backupFile);
                    _view.ShowMessage("Sauvegarde effectuée avec succès.", "Sauvegarde");
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }
    }
}
