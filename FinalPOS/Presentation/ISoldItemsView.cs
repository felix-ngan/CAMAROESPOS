using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface ISoldItemsView
    {
        DateTime DateFrom { get; set; }
        DateTime DateTo { get; set; }
        string SelectedCashier { get; set; }
        void LoadCashiers(IEnumerable<string> cashiers);
        void LoadSoldItems(IEnumerable<SoldItem> items, double totalAmount);
    }
}
