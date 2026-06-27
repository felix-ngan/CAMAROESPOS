using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tulpep.NotificationWindow;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmPOS : Form, IPOSView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly POSPresenter _presenter;
        private readonly frmSecurity _securityForm;

        public string id;
        public string price;

        public frmPOS(frmSecurity frm)
        {
            InitializeComponent();
            lblDate.Text = DateTime.Now.ToLongDateString();
            dataGridView1.Rows.Clear();
            this.KeyPreview = true;

            var productRepository = new Data.ProductRepository(dbcon.MyConnection());
            var cartRepository = new Data.CartRepository(dbcon.MyConnection());
            _presenter = new POSPresenter(this, productRepository, cartRepository);

            _securityForm = frm;

            _presenter.NotifyCriticalItems();
            _presenter.LoadCart();

            // Programmatically add Theme toggle and apply theme
            AddThemeToggle();
            AddLanguageSelector();
        }

        private void AddThemeToggle()
        {
            System.Windows.Forms.Label lblTheme = new System.Windows.Forms.Label();
            lblTheme.Text = "Mode Sombre";
            lblTheme.Size = new Size(110, 20);
            lblTheme.Location = new Point(this.Width - 450, 20);
            lblTheme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTheme.ForeColor = Color.White;
            lblTheme.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            MetroFramework.Controls.MetroToggle themeToggle = new MetroFramework.Controls.MetroToggle();
            themeToggle.Name = "themeToggle";
            themeToggle.Size = new Size(80, 20);
            themeToggle.Location = new Point(this.Width - 340, 20);
            themeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeToggle.Checked = FinalPOS.UI.ThemeManager.CurrentTheme == FinalPOS.UI.ThemeMode.Dark;
            themeToggle.CheckedChanged += (s, ev) => {
                FinalPOS.UI.ThemeManager.CurrentTheme = themeToggle.Checked ? FinalPOS.UI.ThemeMode.Dark : FinalPOS.UI.ThemeMode.Light;
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                lblTheme.ForeColor = Color.White;
            };

            this.panel1.Controls.Add(lblTheme);
            this.panel1.Controls.Add(themeToggle);

            // Apply theme on load
            FinalPOS.UI.ThemeManager.ApplyTheme(this);
        }

        public string TransactionNo
        {
            get => lblTransno.Text;
            set => lblTransno.Text = value;
        }

        public string SearchBarcode
        {
            get => Searchhp.Text;
            set => Searchhp.Text = value;
        }

        public string CashierName
        {
            get => lblUser.Text;
            set => lblUser.Text = value;
        }

        public string Quantity
        {
            get => txtQuantity.Text;
            set => txtQuantity.Text = value;
        }

        public bool CanSettle
        {
            set => btnSettlePayment.Enabled = value;
        }

        public bool CanDiscount
        {
            set => btnDiscount.Enabled = value;
        }

        public bool CanClearCart
        {
            set => btnClearCart.Enabled = value;
        }

        public void PopulateCartGrid(IEnumerable<CartItem> cartItems)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var item in cartItems)
            {
                dataGridView1.Rows.Add(
                    i, 
                    item.Id.ToString(), 
                    item.ProductCode, 
                    item.ProductDescription, 
                    item.Price.ToString(), 
                    item.Qty.ToString(), 
                    item.DiscountAmount.ToString(), 
                    item.Total.ToString("#,##0") + " FCFA", 
                    "AJOUTER 1", 
                    "RETIRER 1"
                );
                i++;
            }
        }

        public void SetCartTotals(double total, double discount, double vat, double vatable, double displayTotal)
        {
            lblSalesTotal.Text = total.ToString("#,##0") + " FCFA";
            lblDiscount.Text = discount.ToString("#,##0") + " FCFA";
            lblVAT.Text = vat.ToString("#,##0") + " FCFA";
            lblVatable.Text = vatable.ToString("#,##0") + " FCFA";
            lblDisplayTotal.Text = displayTotal.ToString("#,##0") + " FCFA";
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void ClearView()
        {
            btnSettlePayment.Enabled = false;
            btnDiscount.Enabled = false;
            lblDisplayTotal.Text = "0 FCFA";
            lblDiscount.Text = "0 FCFA";
            lblVAT.Text = "0 FCFA";
            lblVatable.Text = "0 FCFA";
            lblSalesTotal.Text = "0 FCFA";
            Searchhp.Clear();
        }

        public void ShowCriticalAlert(string countText, string criticalItemsText)
        {
            PopupNotifier popup = new PopupNotifier();
            popup.Image = Properties.Resources.cancel__2_;
            popup.TitleText = countText;
            popup.ContentText = criticalItemsText;
            popup.Popup();
        }

        public void GetTransNo()
        {
            _presenter.GetTransNo();
            Searchhp.Focus();
        }

        public void LoadCart()
        {
            _presenter.LoadCart();
        }

        private void btnNewTransaction_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                return;
            }
            GetTransNo();
            Searchhp.Enabled = true;
            Searchhp.Focus();
        }

        private void btnSearchProduct_Click(object sender, EventArgs e)
        {
            if (lblTransno.Text == "0000000000000000")
            {
                return;
            }
            frmLookUp frm = new frmLookUp(this);
            FinalPOS.UI.ThemeManager.ApplyTheme(frm);
            FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
            frm.LoadRecords();
            frm.ShowDialog();
        }

        private void bntClose_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                MessageBox.Show("Impossible de fermer la session. Veuillez d'abord vider le panier.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Voulez-vous fermer la session ?", "Fermer la session", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Hide();
                if (_securityForm != null && !_securityForm.IsDisposed)
                {
                    _securityForm.Show();
                }
                else
                {
                    frmSecurity frm = new frmSecurity();
                    FinalPOS.UI.ThemeManager.ApplyTheme(frm);
                    FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
                    frm.ShowDialog();
                }
            }
        }

        private void Searchhp_TextChanged_2(object sender, EventArgs e)
        {
            _presenter.ScanBarcode();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (colName == "Delete")
            {
                int idVal = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                _presenter.DeleteCartItem(idVal);
            }
            else if (colName == "colAdd")
            {
                string pcode = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                int rowQty = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString());
                _presenter.AddCartItemQty(pcode, rowQty);
            }
            else if (colName == "colRemove")
            {
                string pcode = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                int rowQty = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString());
                _presenter.RemoveCartItemQty(pcode, rowQty);
            }
        }

        private void btnDiscount_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 1)
            {
                MessageBox.Show("Aucun article sélectionné pour appliquer une remise.", "Remise", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            frmDiscount frm = new frmDiscount(this);
            FinalPOS.UI.ThemeManager.ApplyTheme(frm);
            FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
            frm.lblID.Text = id;
            frm.txtPrice.Text = price;
            frm.ShowDialog();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int i = dataGridView1.CurrentRow.Index;
                id = dataGridView1[1, i].Value.ToString();
                price = dataGridView1[7, i].Value.ToString();
            }
        }

        private void btnSettlePayment_Click(object sender, EventArgs e)
        {
            frmSettle frm = new frmSettle(this);
            FinalPOS.UI.ThemeManager.ApplyTheme(frm);
            FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
            frm.txtSale.Text = lblDisplayTotal.Text;
            frm.ShowDialog();
        }

        private void btnDailySales_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            FinalPOS.UI.ThemeManager.ApplyTheme(frm);
            FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
            frm.dt1.Enabled = false;
            frm.suser = lblUser.Text;
            frm.dt2.Enabled = false;
            frm.cboCashier.Enabled = false;
            frm.cboCashier.Text = lblUser.Text;
            frm.ShowDialog();
        }

        private void frmPOS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                btnNewTransaction_Click(sender, e);
            }
            if (e.KeyCode == Keys.F2)
            {
                btnSearchProduct_Click(sender, e);
            }
            if (e.KeyCode == Keys.F3)
            {
                btnDiscount_Click(sender, e);
            }
            if (e.KeyCode == Keys.F4)
            {
                btnSettlePayment_Click(sender, e);
            }
            if (e.KeyCode == Keys.F5)
            {
                btnClearCart_Click(sender, e);
            }
            if (e.KeyCode == Keys.F6)
            {
                btnDailySales_Click(sender, e);
            }
            if (e.KeyCode == Keys.F7)
            {
                btnChangePassword_Click(sender, e);
            }
            if (e.KeyCode == Keys.F10)
            {
                Searchhp.SelectionStart = 0;
                Searchhp.SelectionLength = Searchhp.Text.Length;
                bntClose_Click_1(sender, e);
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            frmChangePassword frm = new frmChangePassword(this);
            FinalPOS.UI.ThemeManager.ApplyTheme(frm);
            FinalPOS.UI.LanguageManager.ApplyLanguage(frm);
            frm.ShowDialog();
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 1)
            {
                MessageBox.Show("Le panier est déjà vide.", "Panier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _presenter.ClearCart();
        }

        private void cbocustomer_CheckedChanged(object sender, EventArgs e)
        {
            if (cbocustomer.Checked == true)
            {
                btnSettlePayment.Visible = false;
                btnCustomer.Visible = true;
            }
        }

        private void Searchhp_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        }

        private void AddLanguageSelector()
        {
            System.Windows.Forms.Label lblLang = new System.Windows.Forms.Label();
            lblLang.Name = "lblLang";
            lblLang.Text = "Langue";
            lblLang.Size = new Size(110, 20);
            lblLang.Location = new Point(this.Width - 650, 20);
            lblLang.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblLang.ForeColor = Color.White;
            lblLang.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            System.Windows.Forms.ComboBox cboLang = new System.Windows.Forms.ComboBox();
            cboLang.Name = "cboLang";
            cboLang.Size = new Size(100, 20);
            cboLang.Location = new Point(this.Width - 540, 18);
            cboLang.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLang.Items.AddRange(new object[] { "Français", "English" });
            cboLang.SelectedIndex = FinalPOS.UI.LanguageManager.CurrentLanguage == "English" ? 1 : 0;
            cboLang.Font = new Font("Segoe UI", 9F);

            cboLang.SelectedIndexChanged += (s, ev) => {
                FinalPOS.UI.LanguageManager.CurrentLanguage = cboLang.SelectedItem.ToString();
                ApplyCurrentLanguage();
            };

            this.panel1.Controls.Add(lblLang);
            this.panel1.Controls.Add(cboLang);
            ApplyCurrentLanguage();
        }

        private void ApplyCurrentLanguage()
        {
            FinalPOS.UI.LanguageManager.ApplyLanguage(this);

            var lblLang = panel1.Controls["lblLang"] as System.Windows.Forms.Label;
            var lblTheme = panel1.Controls.Cast<Control>().FirstOrDefault(c => c.Text == "Mode Sombre" || c.Text == "Dark Mode") as System.Windows.Forms.Label;

            if (FinalPOS.UI.LanguageManager.CurrentLanguage == "English")
            {
                if (lblLang != null) lblLang.Text = "Language";
                if (lblTheme != null) lblTheme.Text = "Dark Mode";
            }
            else
            {
                if (lblLang != null) lblLang.Text = "Langue";
                if (lblTheme != null) lblTheme.Text = "Mode Sombre";
            }
            
            Searchhp.Invalidate();
            txtQuantity.Invalidate();
        }
    }
}
