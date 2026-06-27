using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmSoldItems : Form, ISoldItemsView
    {
        private readonly SoldItemsPresenter _presenter;
        public string suser;

        public frmSoldItems()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            
            // Initialisation du dépôt et du présentateur
            var repository = new Data.ReportRepository(new DBConnection().MyConnection());
            _presenter = new SoldItemsPresenter(this, repository);
            
            _presenter.Initialize();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _presenter.LoadRecords();
        }

        // Propriétés de l'interface ISoldItemsView
        public DateTime DateFrom
        {
            get => dt1.Value;
            set => dt1.Value = value;
        }

        public DateTime DateTo
        {
            get => dt2.Value;
            set => dt2.Value = value;
        }

        public string SelectedCashier
        {
            get => cboCashier.Text;
            set => cboCashier.Text = value;
        }

        public void LoadCashiers(IEnumerable<string> cashiers)
        {
            cboCashier.Items.Clear();
            foreach (var cashier in cashiers)
            {
                cboCashier.Items.Add(cashier);
            }
        }

        public void LoadSoldItems(IEnumerable<SoldItem> items, double totalAmount)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var item in items)
            {
                i++;
                dataGridView1.Rows.Add(
                    i, 
                    item.Id, 
                    item.TransNo, 
                    item.ProductCode, 
                    item.Description, 
                    item.Price.ToString("#,##0") + " FCFA", 
                    item.Qty, 
                    item.Discount, 
                    item.Total.ToString("#,##0") + " FCFA"
                );
            }
            lblDailyTotal.Text = totalAmount.ToString("#,##0") + " FCFA";
        }

        // Pour compatibilité externe (ex: frmCancelDetails)
        public void LoadRecords()
        {
            _presenter.LoadRecords();
        }

        private void dt1_ValueChanged(object sender, EventArgs e)
        {
            _presenter.LoadRecords();
        }

        private void dt2_ValueChanged(object sender, EventArgs e)
        {
            _presenter.LoadRecords();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            frmSoldIReport frm = new frmSoldIReport(this);
            frm.LoadReport();
            frm.ShowDialog();
        }

        private void cboCashier_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cboCashier_SelectedIndexChanged(object sender, EventArgs e)
        {
            _presenter.LoadRecords();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "colCancel")
            {
                frmCancelDetails f = new frmCancelDetails(this);
                f.txtID.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                f.txtTransno.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                f.txtpcode.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                f.txtdesc.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                f.txtPrice.Text = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                f.txtQty.Text = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString();
                f.txtDiscount.Text = dataGridView1.Rows[e.RowIndex].Cells[7].Value.ToString();
                f.txtTotal.Text = dataGridView1.Rows[e.RowIndex].Cells[8].Value.ToString();
                f.txtCancel.Text = suser;
                
                f.ShowDialog();
            }
        }
    }
}
