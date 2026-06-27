using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmCateogoryList : Form, ICategoryListView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly CategoryListPresenter _presenter;

        public frmCateogoryList()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var repository = new Data.CategoryRepository(dbcon.MyConnection());
            _presenter = new CategoryListPresenter(this, repository);
            LoadCategory();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void LoadCategory()
        {
            _presenter.LoadCategories();
        }

        public void PopulateGrid(IEnumerable<Category> categories)
        {
            int i = 0;
            dataGridView1.Rows.Clear();
            foreach (var category in categories)
            {
                i++;
                dataGridView1.Rows.Add(i, category.Id, category.CategoryName);
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            frmCateogory frm = new frmCateogory(this);
            frm.btnSave.Enabled = true;
            frm.btnUpdate.Enabled = false;
            frm.ShowDialog();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                frmCateogory frm = new frmCateogory(this);
                frm.CategoryName = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                frm.CategoryId = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                frm.btnSave.Enabled = false;
                frm.btnUpdate.Enabled = true;
                frm.ShowDialog();
            }
            else if (colName == "Delete")
            {
                int id = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                _presenter.DeleteCategory(id);
            }
        }
    }
}
