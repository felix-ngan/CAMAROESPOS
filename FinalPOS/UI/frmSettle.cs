using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmSettle : Form, ISettleView
    {
        private readonly frmPOS _fpos;
        private readonly DBConnection dbcon = new DBConnection();
        private readonly SettlePresenter _presenter;

        private Panel panelWallet;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblWalletBal;
        private CheckBox chkUseWallet;
        private CheckBox chkSaveToWallet;
        private double _walletBalance = 0.0;

        public frmSettle(frmPOS fp)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            _fpos = fp;
            var cartRepository = new Data.CartRepository(dbcon.MyConnection());
            _presenter = new SettlePresenter(this, cartRepository);
            InitializeCustomControls();
            this.KeyPreview = true;
            txtCash.Focus();
        }

        private void InitializeCustomControls()
        {
            // Expand window size
            this.ClientSize = new System.Drawing.Size(620, 542);

            // Add separator
            Panel separator = new Panel();
            separator.Location = new System.Drawing.Point(347, 50);
            separator.Size = new System.Drawing.Size(1, 492);
            separator.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(separator);

            // Add custom wallet panel
            panelWallet = new Panel();
            panelWallet.Location = new System.Drawing.Point(355, 60);
            panelWallet.Size = new System.Drawing.Size(250, 470);
            panelWallet.BackColor = System.Drawing.Color.White;

            lblPhone = new Label();
            lblPhone.Text = "Téléphone du Client";
            lblPhone.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblPhone.Location = new System.Drawing.Point(10, 10);
            lblPhone.Size = new System.Drawing.Size(230, 25);

            txtPhone = new TextBox();
            txtPhone.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtPhone.Location = new System.Drawing.Point(10, 40);
            txtPhone.Size = new System.Drawing.Size(230, 30);
            txtPhone.TextChanged += (s, e) => {
                _presenter.LoadCustomerWallet();
                _presenter.CalculateChange();
            };

            lblWalletBal = new Label();
            lblWalletBal.Text = "Solde disponible : 0 FCFA";
            lblWalletBal.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            lblWalletBal.ForeColor = System.Drawing.Color.DarkSlateGray;
            lblWalletBal.Location = new System.Drawing.Point(10, 90);
            lblWalletBal.Size = new System.Drawing.Size(230, 25);

            chkUseWallet = new CheckBox();
            chkUseWallet.Text = "Payer avec le solde";
            chkUseWallet.Font = new System.Drawing.Font("Segoe UI", 11F);
            chkUseWallet.Location = new System.Drawing.Point(10, 130);
            chkUseWallet.Size = new System.Drawing.Size(230, 30);
            chkUseWallet.CheckedChanged += (s, e) => {
                _presenter.CalculateChange();
            };

            chkSaveToWallet = new CheckBox();
            chkSaveToWallet.Text = "Épargner la monnaie";
            chkSaveToWallet.Font = new System.Drawing.Font("Segoe UI", 11F);
            chkSaveToWallet.Location = new System.Drawing.Point(10, 170);
            chkSaveToWallet.Size = new System.Drawing.Size(230, 30);

            panelWallet.Controls.Add(lblPhone);
            panelWallet.Controls.Add(txtPhone);
            panelWallet.Controls.Add(lblWalletBal);
            panelWallet.Controls.Add(chkUseWallet);
            panelWallet.Controls.Add(chkSaveToWallet);

            this.Controls.Add(panelWallet);
        }

        public string TransactionNo => _fpos.lblTransno.Text;

        public string CashierName => _fpos.lblUser.Text;

        public string SaleAmount
        {
            get => txtSale.Text;
            set => txtSale.Text = value;
        }

        public string CashAmount
        {
            get => txtCash.Text;
            set => txtCash.Text = value;
        }

        public string ChangeAmount
        {
            get => txtChange.Text;
            set => txtChange.Text = value;
        }

        public bool SaveToWallet => chkSaveToWallet.Checked;

        public bool UseWallet => chkUseWallet.Checked;

        public string CustomerPhone => txtPhone.Text;

        public void SetWalletBalance(double balance)
        {
            _walletBalance = balance;
            lblWalletBal.Text = "Solde disponible : " + balance.ToString("#,##0") + " FCFA";
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public void ShowReceipt(string cash, string change)
        {
            FrmReciept frm = new FrmReciept(_fpos);
            frm.LoadReport(cash, change);
            frm.ShowDialog();
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            _presenter.CalculateChange();
        }

        private void buttonEnter_Click_1(object sender, EventArgs e)
        {
            if (_presenter.ConfirmPayment())
            {
                _fpos.LoadCart();
                if (_fpos.dataGridView1.Rows.Count < 1)
                {
                    _fpos.lblDisplayTotal.Text = "0 FCFA";
                    _fpos.lblDiscount.Text = "0 FCFA";
                    _fpos.lblVAT.Text = "0 FCFA";
                    _fpos.lblVatable.Text = "0 FCFA";
                    _fpos.lblSalesTotal.Text = "0 FCFA";
                    _fpos.dataGridView1.Rows.Clear();
                    _fpos.btnSettlePayment.Enabled = false;
                    _fpos.btnDiscount.Enabled = false;
                    _fpos.GetTransNo();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            txtCash.Text += button7.Text;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            txtCash.Text += button8.Text;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            txtCash.Text += button9.Text;
        }

        private void button0_Click(object sender, EventArgs e)
        {
            txtCash.Text += button0.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtCash.Text += button4.Text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtCash.Text += button5.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            txtCash.Text += button6.Text;
        }

        private void button00_Click(object sender, EventArgs e)
        {
            txtCash.Text += button00.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtCash.Text += button1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtCash.Text += button2.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txtCash.Text += button3.Text;
        }

        private void buttonC_Click(object sender, EventArgs e)
        {
            txtCash.Clear();
            txtCash.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void txtCash_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8)
            {
                // accept backspace
            }
            else if ((e.KeyChar < 48) || (e.KeyChar > 57))
            {
                e.Handled = true;
            }
        }

        private void frmSettle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonEnter_Click_1(sender, e);
            }
        }
    }
}
