using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface ISearchProductStockInView
    {
        string SearchText { get; }
        string RefNo { get; }
        string StockInBy { get; }
        string VendorId { get; }
        DateTime StockInDate { get; }
        void PopulateGrid(IEnumerable<Product> products);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void CloseView();
    }
}
