using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmProduct : Form, IProductView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly frmProductList flist;
        private readonly ProductPresenter _presenter;

        public frmProduct(frmProductList frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.ProductRepository(dbcon.MyConnection());
            _presenter = new ProductPresenter(this, repository);
            flist = frm;
        }

        public string ProductCode
        {
            get => txtpcode.Text;
            set => txtpcode.Text = value;
        }

        public string Barcode
        {
            get => txtBarcode.Text;
            set => txtBarcode.Text = value;
        }

        public string Description
        {
            get => descriptionTxtBox.Text;
            set => descriptionTxtBox.Text = value;
        }

        public string BrandName
        {
            get => brandcbo.Text;
            set => brandcbo.Text = value;
        }

        public string CategoryName
        {
            get => categorycbo.Text;
            set => categorycbo.Text = value;
        }

        public string Price
        {
            get => pricetxtbox.Text;
            set => pricetxtbox.Text = value;
        }

        public string Reorder
        {
            get => txtReOrder.Text;
            set => txtReOrder.Text = value;
        }

        public void SetBrands(IEnumerable<string> brands)
        {
            brandcbo.Items.Clear();
            foreach (var brand in brands)
            {
                brandcbo.Items.Add(brand);
            }
        }

        public void SetCategories(IEnumerable<string> categories)
        {
            categorycbo.Items.Clear();
            foreach (var category in categories)
            {
                categorycbo.Items.Add(category);
            }
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void LoadCategory()
        {
            _presenter.LoadBrandsAndCategories();
        }

        public void LoadBrand()
        {
            _presenter.LoadBrandsAndCategories();
        }

        public void ClearView()
        {
            txtpcode.Clear();
            descriptionTxtBox.Clear();
            categorycbo.Text = "";
            brandcbo.Text = "";
            txtBarcode.Clear();
            pricetxtbox.Clear();
            txtpcode.Focus();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtReOrder.Clear();
        }

        public void CloseView()
        {
            this.Dispose();
        }

        private void pricetxtbox_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 46 || e.KeyChar == 8)
            {
                // accept dot and backspace
            }
            else if ((e.KeyChar < 48) || (e.KeyChar > 57))
            {
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearView();
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Update())
            {
                flist.LoadRecords();
                CloseView();
            }
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (_presenter.Save())
            {
                flist.LoadRecords();
                CloseView();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }

        private void pricetxtbox_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
