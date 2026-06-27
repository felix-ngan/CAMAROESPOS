using System;
using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface IStockRepository
    {
        IEnumerable<Stock> GetPendingStocksByRefNo(string refNo);
        IEnumerable<Stock> GetStockInHistory(DateTime dateFrom, DateTime dateTo);
        void AddPendingStock(string refNo, string pcode, DateTime sdate, string stockInBy, int vendorId);
        void CompleteStockIn(int id, string pcode, int qty);
        void DeletePendingStock(int id);
        IEnumerable<Vendor> GetVendors();
        Vendor GetVendorByName(string vendorName);
        void AdjustStock(string referenceNo, string pcode, int qty, string action, string remarks, DateTime sdate, string user);
    }
}
