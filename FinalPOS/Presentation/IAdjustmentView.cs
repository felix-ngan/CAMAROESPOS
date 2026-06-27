using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IAdjustmentView
    {
        string SearchText { get; }
        string ReferenceNo { get; set; }
        string ProductCode { get; set; }
        string Description { get; set; }
        string Qty { get; set; }
        string Action { get; set; }
        string Remarks { get; set; }
        string CurrentUser { get; set; }
        int AvailableStock { get; set; }

        void PopulateGrid(IEnumerable<Product> products);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearFields();
    }
}
