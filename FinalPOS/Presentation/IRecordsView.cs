using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IRecordsView
    {
        // Top Selling
        string TopSellingFilter { get; set; }
        DateTime TopSellingDateFrom { get; set; }
        DateTime TopSellingDateTo { get; set; }
        void LoadTopSellingItems(IEnumerable<SoldItem> items);
        void LoadTopSellingChart(IEnumerable<SoldItem> items, string yValueMember);

        // Sold Items
        DateTime SoldItemsDateFrom { get; set; }
        DateTime SoldItemsDateTo { get; set; }
        void LoadSoldItemsGrouped(IEnumerable<SoldItem> items, double totalAmount);

        // Inventory
        void LoadInventory(IEnumerable<Product> items);

        // Critical Items
        void LoadCriticalItems(IEnumerable<Product> items);

        // Stock In History
        DateTime StockInDateFrom { get; set; }
        DateTime StockInDateTo { get; set; }
        void LoadStockInHistory(IEnumerable<Stock> items);

        // Cancelled Orders
        DateTime CancelledOrdersDateFrom { get; set; }
        DateTime CancelledOrdersDateTo { get; set; }
        void LoadCancelledOrders(IEnumerable<CancelledOrder> items);

        // Customer Wallets & Change Management
        string WalletSearchPhone { get; set; }
        void LoadCustomerWallets(IEnumerable<CustomerWallet> wallets);

        // Wallet Transactions
        DateTime WalletDateFrom { get; set; }
        DateTime WalletDateTo { get; set; }
        void LoadWalletTransactions(IEnumerable<WalletTransaction> transactions);

        // Consolidation
        DateTime ConsolidationDateFrom { get; set; }
        DateTime ConsolidationDateTo { get; set; }
        void LoadConsolidation(IEnumerable<StoreRevenue> revenues);
    }
}
