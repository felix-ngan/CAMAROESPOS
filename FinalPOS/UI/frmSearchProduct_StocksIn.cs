using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmSearchProduct_StocksIn : Form, ISearchProductStockInView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly SearchProductStockInPresenter _presenter;
        private readonly frmStockIn slist;

        public frmSearchProduct_StocksIn(frmStockIn flist)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var productRepository = new Data.ProductRepository(dbcon.MyConnection());
            var stockRepository = new Data.StockRepository(dbcon.MyConnection());
            _presenter = new SearchProductStockInPresenter(this, productRepository, stockRepository);
            slist = flist;
        }

        public string SearchText => txtSearch.Text;

        public string RefNo => slist.RefNo;

        public string StockInBy => slist.StockInBy;

        public string VendorId => slist.VendorId;

        public DateTime StockInDate => slist.StockInDate;

        public void PopulateGrid(IEnumerable<Product> products)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var p in products)
            {
                i++;
                dataGridView1.Rows.Add(i, p.ProductCode, p.Description, p.Qty, p.Price);
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

        public void CloseView()
        {
            this.Dispose();
        }

        public void LoadProduct()
        {
            _presenter.LoadProducts();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Select")
            {
                string pcode = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                if (_presenter.SelectProduct(pcode))
                {
                    slist.LoadStocksIn();
                }
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            LoadProduct();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CloseView();
        }
    }
}
