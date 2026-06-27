using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmAdjustment : Form, IAdjustmentView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly AdjustmentPresenter _presenter;
        private int _qty;

        public frmAdjustment(Form1 f)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var productRepository = new Data.ProductRepository(dbcon.MyConnection());
            var stockRepository = new Data.StockRepository(dbcon.MyConnection());
            _presenter = new AdjustmentPresenter(this, productRepository, stockRepository);

            // Subscribe to search text changes programmatically
            txtSearchp.TextChanged += (sender, e) => LoadRecords();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public string SearchText => txtSearchp.Text;

        public string ReferenceNo
        {
            get => txtRefNo.Text;
            set => txtRefNo.Text = value;
        }

        public string ProductCode
        {
            get => txtPcode.Text;
            set => txtPcode.Text = value;
        }

        public string Description
        {
            get => txtDescription.Text;
            set => txtDescription.Text = value;
        }

        public string Qty
        {
            get => txtQty.Text;
            set => txtQty.Text = value;
        }

        public string Action
        {
            get => cboCommand.Text;
            set => cboCommand.Text = value;
        }

        public string Remarks
        {
            get => txtRemarks.Text;
            set => txtRemarks.Text = value;
        }

        public string CurrentUser
        {
            get => txtUser.Text;
            set => txtUser.Text = value;
        }

        public int AvailableStock
        {
            get => _qty;
            set => _qty = value;
        }

        public void RefrenceNo()
        {
            _presenter.GenerateReferenceNo();
        }

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
                dataGridView1.Rows.Add(
                    i, 
                    product.ProductCode, 
                    product.Barcode, 
                    product.Description, 
                    product.BrandName, 
                    product.CategoryName, 
                    product.Price.ToString("#,##0") + " FCFA", 
                    product.Qty
                );
            }
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void ClearFields()
        {
            txtDescription.Clear();
            txtPcode.Clear();
            txtQty.Clear();
            txtRefNo.Clear();
            txtRemarks.Clear();
            cboCommand.Text = "";
            RefrenceNo();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Select")
            {
                txtPcode.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtDescription.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString() + " " +
                                      dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString() + " " +
                                      dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                
                // Get the raw numeric quantity
                string qtyVal = dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString();
                int.TryParse(qtyVal, out _qty);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.SaveAdjustment();
        }
    }
}
