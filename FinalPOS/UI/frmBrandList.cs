using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmBrandList : Form, IBrandListView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly BrandListPresenter _presenter;

        public frmBrandList()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.BrandRepository(dbcon.MyConnection());
            _presenter = new BrandListPresenter(this, repository);
            LoadRecords();  
        }

        public void LoadRecords()
        {
            _presenter.LoadBrands();
        }

        public void PopulateGrid(IEnumerable<Brand> brands)
        {
            int i = 0;
            dataGridView1.Rows.Clear();
            foreach (var brand in brands)
            {
                i += 1;
                dataGridView1.Rows.Add(i, brand.Id, brand.BrandName);
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
            frmBrand frm = new frmBrand(this);
            frm.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                frmBrand frm = new frmBrand(this);
                frm.btnSave.Enabled = false;
                frm.lblID.Visible = false;
                frm.BrandId = dataGridView1[1, e.RowIndex].Value.ToString();
                frm.BrandName = dataGridView1[2, e.RowIndex].Value.ToString();
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                int id = int.Parse(dataGridView1[1, e.RowIndex].Value.ToString());
                _presenter.DeleteBrand(id);
            }
        }
    }
}
