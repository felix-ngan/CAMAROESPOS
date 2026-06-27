using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmDiscount : Form, IDiscountView
    {
        private readonly DBConnection dbcon = new DBConnection();
        private readonly DiscountPresenter _presenter;
        private readonly frmPOS _posForm;

        public frmDiscount(frmPOS frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            var cartRepository = new Data.CartRepository(dbcon.MyConnection());
            _presenter = new DiscountPresenter(this, cartRepository);
            _posForm = frm;
            this.KeyPreview = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public string CartId
        {
            get => lblID.Text;
            set => lblID.Text = value;
        }

        public string Price
        {
            get => txtPrice.Text;
            set => txtPrice.Text = value;
        }

        public string DiscountPercent
        {
            get => txtDisocunt.Text;
            set => txtDisocunt.Text = value;
        }

        public string DiscountAmount
        {
            get => txtDiscountAmount.Text;
            set => txtDiscountAmount.Text = value;
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        public bool ConfirmMessage(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void txtDisocunt_TextChanged(object sender, EventArgs e)
        {
            _presenter.CalculateDiscount();
        }

        private void btnConfirm_Click_1(object sender, EventArgs e)
        {
            if (_presenter.ConfirmDiscount())
            {
                _posForm.LoadCart();
            }
        }

        private void frmDiscount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}
