using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmStockIn : Form, IStockInView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly StockInPresenter _presenter;

        public frmStockIn()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.StockRepository(dbcon.MyConnection());
            _presenter = new StockInPresenter(this, repository);
            LoadVendor();
        }

        public string RefNo
        {
            get => txtrefno.Text;
            set => txtrefno.Text = value;
        }

        public string StockInBy
        {
            get => txtstockinby.Text;
            set => txtstockinby.Text = value;
        }

        public DateTime StockInDate
        {
            get => dttime.Value;
            set => dttime.Value = value;
        }

        public string SelectedVendorName
        {
            get => cboVendor.Text;
            set => cboVendor.Text = value;
        }

        public string VendorId
        {
            get => lblVendorID.Text;
            set => lblVendorID.Text = value;
        }

        public string ContactPerson
        {
            get => txtContactPerson.Text;
            set => txtContactPerson.Text = value;
        }

        public DateTime HistoryDateFrom => date1.Value;
        public DateTime HistoryDateTo => date2.Value;

        public void SetVendors(IEnumerable<string> vendorNames)
        {
            cboVendor.Items.Clear();
            foreach (var name in vendorNames)
            {
                cboVendor.Items.Add(name);
            }
        }

        public void PopulatePendingGrid(IEnumerable<Stock> stocks)
        {
            stgrids.Rows.Clear();
            int i = 0;
            foreach (var s in stocks)
            {
                i++;
                stgrids.Rows.Add(i, s.Id, s.RefNo, s.ProductCode, s.Description, s.Qty, s.StockInDate.ToString("yyyy-MM-dd"), s.StockInBy, s.VendorName);
            }
        }

        public void PopulateHistoryGrid(IEnumerable<Stock> stocks)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var s in stocks)
            {
                i++;
                dataGridView1.Rows.Add(i, s.Id, s.RefNo, s.ProductCode, s.Description, s.Qty, s.StockInDate.ToShortDateString(), s.StockInBy, s.VendorName);
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

        public void ClearView()
        {
            txtstockinby.Clear();
            txtrefno.Clear();
            cboVendor.Text = "";
            txtContactPerson.Clear();
            dttime.Value = DateTime.Now;
        }

        public void LoadStocksIn()
        {
            _presenter.LoadPendingStocks();
        }

        public void LoadVendor()
        {
            _presenter.LoadVendors();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _presenter.LoadHistory();
        }

        private void stgrids_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = stgrids.Columns[e.ColumnIndex].Name;
            if (colName == "Delete")
            {
                int id = int.Parse(stgrids.Rows[e.RowIndex].Cells[1].Value.ToString());
                _presenter.DeleteStock(id, false);
            }
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            var items = new List<(int id, string pcode, int qty)>();
            for (int i = 0; i < stgrids.Rows.Count; i++)
            {
                int id = int.Parse(stgrids.Rows[i].Cells[1].Value.ToString());
                string pcode = stgrids.Rows[i].Cells[3].Value.ToString();
                int qty = int.Parse(stgrids.Rows[i].Cells[5].Value.ToString());
                items.Add((id, pcode, qty));
            }
            _presenter.SaveStockIn(items);
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmSearchProduct_StocksIn frm = new frmSearchProduct_StocksIn(this);
            frm.LoadProduct();
            frm.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Delete")
            {
                int id = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                _presenter.DeleteStock(id, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearView();
            txtrefno.Focus();
            stgrids.Rows.Clear();
        }

        private void cboVendor_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cboVendor_TextChanged(object sender, EventArgs e)
        {
            _presenter.OnVendorSelected();
        }

        private void cboVendor_SelectedValueChanged(object sender, EventArgs e)
        {
            _presenter.OnVendorSelected();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Random rnd = new Random();
            txtrefno.Clear();
            txtrefno.Text += rnd.Next();
        }
    }
}
