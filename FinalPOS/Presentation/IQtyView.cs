using System;

namespace FinalPOS.Presentation
{
    public interface IQtyView
    {
        string Quantity { get; set; }
        string ProductCode { get; set; }
        double Price { get; set; }
        string TransactionNo { get; set; }
        int AvailableStock { get; set; }
        string CashierName { get; }

        void CloseView();
        void ShowMessage(string message, string title, bool isError = false);
    }
}
