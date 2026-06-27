using System;
using System.Windows.Forms;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class SecurityPresenter
    {
        private readonly ISecurityView _view;
        private readonly IUserRepository _userRepository;

        public SecurityPresenter(ISecurityView view, IUserRepository userRepository)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public void Login()
        {
            string username = _view.Username.Trim();
            string password = _view.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _view.ShowMessage("Veuillez saisir votre nom d'utilisateur et votre mot de passe.", "Champs requis", true);
                return;
            }

            try
            {
                User user = _userRepository.Authenticate(username, password);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        _view.ShowMessage("Ce compte est inactif. Impossible de se connecter.", "Compte inactif", true);
                        return;
                    }

                    _view.ShowMessage("Bienvenue " + user.Name + " !", "ACCÈS AUTORISÉ", false);
                    _view.ClearFields();
                    _view.HideView();

                    if (user.Role == "Cashier")
                    {
                        frmSecurity viewForm = _view as frmSecurity;
                        frmPOS frm = new frmPOS(viewForm);
                        frm.lblUser.Text = user.Username;
                        frm.lblName.Text = user.Name + " | " + user.Role;
                        frm.ShowDialog();
                    }
                    else if (user.Role == "System Administrator")
                    {
                        Form1 frm = new Form1();
                        frm.lblRole.Text = user.Role;
                        frm._pass = user.Password;
                        frm._username = user.Username;
                        frm.ShowDialog();
                    }
                }
                else
                {
                    _view.ShowMessage("Nom d'utilisateur ou mot de passe incorrect.", "ACCÈS REFUSÉ", true);
                    _view.ClearFields();
                    _view.FocusUsername();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Erreur lors de la connexion: " + ex.Message, "Erreur", true);
            }
        }
    }
}
