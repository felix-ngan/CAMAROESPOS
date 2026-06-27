using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmCateogory : Form, ICategoryView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly frmCateogoryList flist;
        private readonly CategoryPresenter _presenter;

        public frmCateogory(frmCateogoryList frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.CategoryRepository(dbcon.MyConnection());
            _presenter = new CategoryPresenter(this, repository);
            flist = frm;
        }

        public string CategoryId
        {
            get => lblID.Text;
            set => lblID.Text = value;
        }

        public string CategoryName
        {
            get => txtCategory.Text;
            set => txtCategory.Text = value;
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
            txtCategory.Clear();
            txtCategory.Focus();
        }

        public void CloseView()
        {
            this.Dispose();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Save())
            {
                flist.LoadCategory();
                CloseView();
            }
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Update())
            {
                flist.LoadCategory();
                CloseView();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearView();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
