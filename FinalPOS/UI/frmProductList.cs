using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmProductList : Form, IProductListView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly ProductListPresenter _presenter;

        public frmProductList()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.ProductRepository(dbcon.MyConnection());
            _presenter = new ProductListPresenter(this, repository);
        }

        public string SearchText => txtSearchp.Text;

        public void LoadRecords()
        {
            _presenter.LoadProducts();
        }

        public void PopulateGrid(IEnumerable<Product> products)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var product in products)
            {
                i++;
                dataGridView1.Rows.Add(i, product.ProductCode, product.Barcode, product.Description, product.BrandName, product.CategoryName, product.Price.ToString("#,##0") + " FCFA", product.Reorder);
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

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            frmProduct frm = new frmProduct(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.LoadBrand();
            frm.LoadCategory();
            frm.ShowDialog();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void txtSearchp_TextChanged_1(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                frmProduct frm = new frmProduct(this);
                frm.LoadCategory();
                frm.LoadBrand();
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.txtpcode.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.txtBarcode.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.descriptionTxtBox.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                frm.brandcbo.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                frm.categorycbo.Text = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                frm.pricetxtbox.Text = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString();
                frm.txtReOrder.Text = dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                string pcode = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                _presenter.DeleteProduct(pcode);
            }
        }

        private void frmProductList_Load(object sender, EventArgs e)
        {
            LoadRecords();
        }
    }
}
