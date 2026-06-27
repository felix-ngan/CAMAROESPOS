using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IStockInView
    {
        string RefNo { get; set; }
        string StockInBy { get; set; }
        DateTime StockInDate { get; set; }
        string SelectedVendorName { get; set; }
        string VendorId { get; set; }
        string ContactPerson { get; set; }
        DateTime HistoryDateFrom { get; }
        DateTime HistoryDateTo { get; }
        void SetVendors(IEnumerable<string> vendorNames);
        void PopulatePendingGrid(IEnumerable<Stock> stocks);
        void PopulateHistoryGrid(IEnumerable<Stock> stocks);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
    }
}
