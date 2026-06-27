using System;

namespace FinalPOS.Presentation
{
    public interface ISettleView
    {
        string TransactionNo { get; }
        string CashierName { get; }
        string SaleAmount { get; set; }
        string CashAmount { get; set; }
        string ChangeAmount { get; set; }
        bool SaveToWallet { get; }
        bool UseWallet { get; }
        string CustomerPhone { get; }

        void SetWalletBalance(double balance);
        void CloseView();
        void ShowMessage(string message, string title, bool isError = false);
        void ShowReceipt(string cash, string change);
    }
}
