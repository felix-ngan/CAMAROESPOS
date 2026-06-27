using System;
using System.Windows.Forms;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmQty : Form, IQtyView
    {
        private readonly frmPOS _posForm;
        private readonly DBConnection dbcon = new DBConnection();
        private readonly QtyPresenter _presenter;

        private string _pcode;
        private double _price;
        private string _transno;
        private int _availableStock;

        public frmQty(frmPOS frmpos)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            _posForm = frmpos;
            var cartRepository = new Data.CartRepository(dbcon.MyConnection());
            _presenter = new QtyPresenter(this, cartRepository);
            this.KeyPreview = true;
        }

        public string Quantity
        {
            get => txtQty.Text;
            set => txtQty.Text = value;
        }

        public string ProductCode
        {
            get => _pcode;
            set => _pcode = value;
        }

        public double Price
        {
            get => _price;
            set => _price = value;
        }

        public string TransactionNo
        {
            get => _transno;
            set => _transno = value;
        }

        public int AvailableStock
        {
            get => _availableStock;
            set => _availableStock = value;
        }

        public string CashierName => _posForm.lblUser.Text;

        public void ProductDetails(string pcode, double price, string transno, int qty)
        {
            this.ProductCode = pcode;
            this.Price = price;
            this.TransactionNo = transno;
            this.AvailableStock = qty;
        }

        public void CloseView()
        {
            this.Dispose();
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void txtQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8)
            {
                // accept backspace
            }
            else if ((e.KeyChar < 48) || (e.KeyChar > 57))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == 13) && (txtQty.Text != string.Empty))
            {
                if (_presenter.AddToCart())
                {
                    _posForm.Searchhp.Clear();
                    _posForm.Searchhp.Focus();
                    _posForm.LoadCart();
                }
            }
        }

        private void frmQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseView();
            }
        }

        private void txtQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseView();
            }
        }
    }
}
