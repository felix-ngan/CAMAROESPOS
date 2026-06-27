using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmUserAccounts : Form, IUserAccountsView
    {
        private readonly Form1 f;
        private readonly UserAccountsPresenter _presenter;

        public frmUserAccounts(Form1 f)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var userRepository = new Data.UserRepository();
            _presenter = new UserAccountsPresenter(this, userRepository);
            this.f = f;
        }

        // Create Account
        public string NewUsername
        {
            get => txtUser.Text;
            set => txtUser.Text = value;
        }

        public string NewPassword
        {
            get => txtPassword.Text;
            set => txtPassword.Text = value;
        }

        public string NewConfirmPassword
        {
            get => txtRePassword.Text;
            set => txtRePassword.Text = value;
        }

        public string NewRole
        {
            get => cboRole.Text;
            set => cboRole.Text = value;
        }

        public string NewFullName
        {
            get => txtName.Text;
            set => txtName.Text = value;
        }

        // Change Password
        public string CurrentUsername
        {
            get => txtUsername.Text;
            set => txtUsername.Text = value;
        }

        public string OldPassword
        {
            get => txtOldPassword.Text;
            set => txtOldPassword.Text = value;
        }

        public string NewPasswordChange
        {
            get => txtNewPassword.Text;
            set => txtNewPassword.Text = value;
        }

        public string ConfirmNewPassword
        {
            get => txtRetypeNew.Text;
            set => txtRetypeNew.Text = value;
        }

        // Activate/Deactivate
        public string SearchUsername
        {
            get => textBox1.Text;
            set => textBox1.Text = value;
        }

        public bool IsAccountActive
        {
            get => checkBox1.Checked;
            set => checkBox1.Checked = value;
        }

        // Backup
        public string BackupPath
        {
            get => txtbackupbrowse.Text;
            set => txtbackupbrowse.Text = value;
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void ClearCreateAccountFields()
        {
            txtName.Clear();
            txtPassword.Clear();
            txtRePassword.Clear();
            txtUser.Clear();
            cboRole.Text = "";
            txtUser.Focus();
        }

        public void ClearChangePasswordFields()
        {
            txtRetypeNew.Clear();
            txtNewPassword.Clear();
            txtOldPassword.Clear();
        }

        public void ClearActivateFields()
        {
            textBox1.Clear();
            checkBox1.Checked = false;
        }

        private void frmUserAccounts_Resize(object sender, EventArgs e)
        {
            metroTabControl1.Left = (this.Width - metroTabControl1.Width) / 2;
            metroTabControl1.Top = (this.Width - metroTabControl1.Width) / 2;
        }

        private void frmUserAccounts_Load(object sender, EventArgs e)
        {
            txtUser.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearCreateAccountFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _presenter.CreateAccount();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _presenter.ChangePassword(f._pass);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _presenter.OnSearchUsernameChanged();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _presenter.UpdateUserActivation();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtbackupbrowse.Text = dlg.SelectedPath;
                backupbutton.Enabled = true;
            }
        }

        private void backupbutton_Click(object sender, EventArgs e)
        {
            _presenter.ExecuteBackup();
        }
    }
}