using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmBrand : Form, IBrandView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly frmBrandList frmlist;
        private readonly BrandPresenter _presenter;

        public frmBrand(frmBrandList flist)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.BrandRepository(dbcon.MyConnection());
            _presenter = new BrandPresenter(this, repository);
            frmlist = flist;
        }

        public string BrandId
        {
            get => lblID.Text;
            set => lblID.Text = value;
        }

        public string BrandName
        {
            get => txtBrand.Text;
            set => txtBrand.Text = value;
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
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtBrand.Clear();
            txtBrand.Focus();
        }

        public void CloseView()
        {
            this.Dispose();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            CloseView();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearView();
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Update())
            {
                frmlist.LoadRecords();
                CloseView();
            }
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Save())
            {
                frmlist.LoadRecords();
                CloseView();
            }
        }
    }
}
