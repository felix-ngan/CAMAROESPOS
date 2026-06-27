using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmLookUp : Form, ILookUpView
    {
        private readonly frmPOS _posForm;
        private readonly DBConnection dbcon = new DBConnection();
        private readonly LookUpPresenter _presenter;

        public frmLookUp(frmPOS frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            _posForm = frm;
            var productRepository = new Data.ProductRepository(dbcon.MyConnection());
            _presenter = new LookUpPresenter(this, productRepository);
            this.KeyPreview = true;
        }

        public string SearchText => txtSearchp.Text;

        public void PopulateProducts(IEnumerable<Product> products)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var product in products)
            {
                i++;
                dataGridView1.Rows.Add(
                    i, 
                    product.ProductCode, 
                    product.Barcode, 
                    product.Description, 
                    product.BrandName, 
                    product.CategoryName, 
                    product.Price, 
                    product.Qty
                );
            }
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void LoadRecords()
        {
            _presenter.LoadProducts();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            CloseView();
        }

        private void txtSearchp_TextChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void frmLookUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseView();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string pcode = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            double price = double.Parse(dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString());
            string transno = _posForm.lblTransno.Text;
            int stock = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString());

            frmQty frm = new frmQty(_posForm);
            frm.ProductDetails(pcode, price, transno, stock);
            frm.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}
