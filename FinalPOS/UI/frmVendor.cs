using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmVendor : Form, IVendorView
    {
        private readonly frmVendorList f;
        private readonly DBConnection dbcon = new DBConnection();
        private readonly VendorPresenter _presenter;

        public frmVendor(frmVendorList flist)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.VendorRepository(dbcon.MyConnection());
            _presenter = new VendorPresenter(this, repository);
            f = flist;
        }

        public string VendorId
        {
            get => lblID.Text;
            set => lblID.Text = value;
        }

        public string VendorName
        {
            get => txtvendor.Text;
            set => txtvendor.Text = value;
        }

        public string Address
        {
            get => txtaddress.Text;
            set => txtaddress.Text = value;
        }

        public string ContactPerson
        {
            get => txtContactPerson.Text;
            set => txtContactPerson.Text = value;
        }

        public string Phone
        {
            get => txtTelephone.Text;
            set => txtTelephone.Text = value;
        }

        public string Email
        {
            get => txtEmail.Text;
            set => txtEmail.Text = value;
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void ClearView()
        {
            txtvendor.Clear();
            txtaddress.Clear();
            txtContactPerson.Clear();
            txtTelephone.Clear();
            txtEmail.Clear();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }

        public void CloseView()
        {
            this.Dispose();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_presenter.Save())
            {
                f.LoadRecords();
                CloseView();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_presenter.Update())
            {
                f.LoadRecords();
                CloseView();
            }
        }
    }
}
