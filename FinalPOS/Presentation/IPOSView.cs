using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IPOSView
    {
        string TransactionNo { get; set; }
        string SearchBarcode { get; set; }
        string CashierName { get; set; }
        string Quantity { get; set; }

        bool CanSettle { set; }
        bool CanDiscount { set; }
        bool CanClearCart { set; }

        void PopulateCartGrid(IEnumerable<CartItem> cartItems);
        void SetCartTotals(double total, double discount, double vat, double vatable, double displayTotal);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
        void ShowCriticalAlert(string countText, string criticalItemsText);
    }
}
