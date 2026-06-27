using System;

namespace FinalPOS.Presentation
{
    public interface IDiscountView
    {
        string CartId { get; set; }
        string Price { get; set; }
        string DiscountPercent { get; set; }
        string DiscountAmount { get; set; }

        void CloseView();
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
    }
}
