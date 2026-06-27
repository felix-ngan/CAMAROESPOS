using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using FinalPOS.Presentation;
using FinalPOS.Data;

namespace FinalPOS
{
    public partial class frmSecurity : Form, ISecurityView
    {
        public string _pass, _username = "";
        public bool _isactive = false;

        private readonly SecurityPresenter _presenter;

        private ComboBox cboLang;

        public frmSecurity()
        {
            InitializeComponent();
            txtUsername.Focus();
            this.KeyPreview = true;
            _presenter = new SecurityPresenter(this, new UserRepository());
            InitializeLanguageSelector();
            FinalPOS.UI.ThemeManager.ApplyTheme(this);
        }

        private void InitializeLanguageSelector()
        {
            cboLang = new ComboBox();
            cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLang.Items.AddRange(new object[] { "Français", "English" });
            cboLang.SelectedIndex = UI.LanguageManager.CurrentLanguage == "English" ? 1 : 0;
            
            cboLang.Location = new Point(this.panel1.Width - 110, 10);
            cboLang.Width = 100;
            cboLang.Font = new Font("Segoe UI", 9);

            cboLang.SelectedIndexChanged += (s, e) => {
                UI.LanguageManager.CurrentLanguage = cboLang.SelectedItem.ToString();
                ApplyTranslations();
            };

            this.panel1.Controls.Add(cboLang);
            cboLang.BringToFront();
            ApplyTranslations();
        }

        private void ApplyTranslations()
        {
            if (UI.LanguageManager.CurrentLanguage == "English")
            {
                UI.LanguageManager.ApplyLanguage(this);
                label1.Text = "Login";
                btnCancel.Text = "EXIT";
                btnLogin.Text = "LOGIN";
                txtUsername.WaterMark = "Username";
                txtPassword.WaterMark = "Password";
            }
            else
            {
                label1.Text = "Connexion";
                btnCancel.Text = "QUITTER";
                btnLogin.Text = "SE CONNECTER";
                txtUsername.WaterMark = "Nom d'utilisateur";
                txtPassword.WaterMark = "Mot de passe";
            }
            txtUsername.Invalidate();
            txtPassword.Invalidate();
        }

        public string Username
        {
            get => txtUsername.Text;
            set => txtUsername.Text = value;
        }

        public string Password
        {
            get => txtPassword.Text;
            set => txtPassword.Text = value;
        }

        public void ShowMessage(string message, string title, bool isError)
        {
            MessageBoxIcon icon = isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        public void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }

        public void FocusUsername()
        {
            txtUsername.Focus();
        }

        public void HideView()
        {
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearFields();
            Application.Exit(); 
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            _presenter.Login();
        }

        private void frmSecurity_Load(object sender, EventArgs e)
        {
            
        }

        private void frmSecurity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
