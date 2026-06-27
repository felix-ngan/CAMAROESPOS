using System;
using System.Linq;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class SettlePresenter
    {
        private readonly ISettleView _view;
        private readonly ICartRepository _cartRepository;

        public SettlePresenter(ISettleView view, ICartRepository cartRepository)
        {
            _view = view;
            _cartRepository = cartRepository;
        }

        public void LoadCustomerWallet()
        {
            try
            {
                string phone = _view.CustomerPhone.Trim();
                if (string.IsNullOrEmpty(phone))
                {
                    _view.SetWalletBalance(0);
                    return;
                }
                double balance = _cartRepository.GetWalletBalance(phone);
                _view.SetWalletBalance(balance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading customer wallet: " + ex.Message);
            }
        }

        public void CalculateChange()
        {
            try
            {
                double sale = double.Parse(_view.SaleAmount.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);
                double cash = 0.0;
                if (!string.IsNullOrEmpty(_view.CashAmount))
                {
                    cash = double.Parse(_view.CashAmount.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);
                }

                double walletBalance = 0.0;
                string phone = _view.CustomerPhone.Trim();
                if (_view.UseWallet && !string.IsNullOrEmpty(phone))
                {
                    walletBalance = _cartRepository.GetWalletBalance(phone);
                }

                double amountToPay = Math.Max(0.0, sale - walletBalance);
                double change = cash - amountToPay;

                _view.ChangeAmount = change.ToString("#,##0") + " FCFA";
            }
            catch
            {
                _view.ChangeAmount = "0 FCFA";
            }
        }

        public bool ConfirmPayment()
        {
            try
            {
                string phone = _view.CustomerPhone.Trim();
                if ((_view.UseWallet || _view.SaveToWallet) && string.IsNullOrEmpty(phone))
                {
                    _view.ShowMessage("Veuillez saisir le numéro de téléphone du client pour utiliser le porte-monnaie virtuel.", "Attention", true);
                    return false;
                }

                if (string.IsNullOrEmpty(_view.CashAmount))
                {
                    _view.ShowMessage("Montant insuffisant. Veuillez saisir un montant correct.", "Attention", true);
                    return false;
                }

                double sale = double.Parse(_view.SaleAmount.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);
                double cash = double.Parse(_view.CashAmount.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);

                double walletBalance = 0.0;
                if (_view.UseWallet && !string.IsNullOrEmpty(phone))
                {
                    walletBalance = _cartRepository.GetWalletBalance(phone);
                }

                double amountToPay = Math.Max(0.0, sale - walletBalance);

                if (cash < amountToPay)
                {
                    _view.ShowMessage("Montant insuffisant. Veuillez saisir un montant correct.", "Attention", true);
                    return false;
                }

                double change = cash - amountToPay;

                double walletDeduction = 0.0;
                if (_view.UseWallet && !string.IsNullOrEmpty(phone))
                {
                    walletDeduction = Math.Min(sale, walletBalance);
                }

                double walletCredit = 0.0;
                if (_view.SaveToWallet && !string.IsNullOrEmpty(phone) && change > 0)
                {
                    walletCredit = change;
                    change = 0.0; // monnaie enregistrée sur le compte
                }

                // Get pending items for this transaction from the database
                var pendingItems = _cartRepository.GetPendingCartItems(_view.TransactionNo).ToList();
                if (!pendingItems.Any())
                {
                    _view.ShowMessage("Aucun article dans le panier pour cette transaction.", "Attention", true);
                    return false;
                }

                var itemsToSettle = pendingItems.Select(item => (item.Id, item.ProductCode, item.Qty));

                // Perform settlement
                _cartRepository.SettlePayment(_view.TransactionNo, _view.CashierName, itemsToSettle, phone, walletDeduction, walletCredit);

                // Show receipt and complete
                _view.ShowReceipt(cash.ToString("#,##0") + " FCFA", change.ToString("#,##0") + " FCFA");
                _view.CloseView();
                return true;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
            return false;
        }
    }
}
