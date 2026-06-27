using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmVendorList : Form, IVendorListView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly VendorListPresenter _presenter;

        public frmVendorList()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.VendorRepository(dbcon.MyConnection());
            _presenter = new VendorListPresenter(this, repository);
            LoadRecords();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            frmVendor f = new frmVendor(this);
            f.btnSave.Enabled = true;
            f.btnUpdate.Enabled = false;
            f.ShowDialog();
        }

        public void LoadRecords()
        {
            _presenter.LoadVendors();
        }

        public void PopulateGrid(IEnumerable<Vendor> vendors)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var vendor in vendors)
            {
                i++;
                dataGridView1.Rows.Add(i, vendor.Id, vendor.VendorName, vendor.Address, vendor.ContactPerson, vendor.Phone, vendor.Email);
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                frmVendor frm = new frmVendor(this);
                frm.VendorId = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.VendorName = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.Address = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                frm.ContactPerson = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                frm.Phone = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                frm.Email = dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString();
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                int id = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                _presenter.DeleteVendor(id);
            }
        }
    }
}
