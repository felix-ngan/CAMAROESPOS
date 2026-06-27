using System;
using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface IReportRepository
    {
        IEnumerable<string> GetCashiers();
        IEnumerable<SoldItem> GetSoldItems(DateTime from, DateTime to, string cashier);
        IEnumerable<SoldItem> GetTopSellingByAmount(DateTime from, DateTime to);
        IEnumerable<SoldItem> GetTopSellingByQty(DateTime from, DateTime to);
        IEnumerable<SoldItem> GetSoldItemsGrouped(DateTime from, DateTime to);
        double GetTotalRevenue(DateTime from, DateTime to);
        IEnumerable<CancelledOrder> GetCancelledOrders(DateTime from, DateTime to);
        IEnumerable<CustomerWallet> GetCustomerWallets(string searchPhone);
        IEnumerable<WalletTransaction> GetWalletTransactions(DateTime from, DateTime to, string searchPhone);
        IEnumerable<StoreRevenue> GetStoreRevenueConsolidated(DateTime from, DateTime to, string centralConnStr);
    }
}
