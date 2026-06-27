using System;
using System.Collections.Generic;
using System.Linq;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class RecordsPresenter
    {
        private readonly IRecordsView _view;
        private readonly IReportRepository _reportRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public RecordsPresenter(
            IRecordsView view, 
            IReportRepository reportRepository, 
            IProductRepository productRepository, 
            IStockRepository stockRepository)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        }

        public void LoadTopSelling()
        {
            if (string.IsNullOrEmpty(_view.TopSellingFilter))
            {
                return;
            }

            DateTime start = _view.TopSellingDateFrom.Date;
            DateTime end = _view.TopSellingDateTo.Date.AddDays(1).AddTicks(-1);

            IEnumerable<SoldItem> items;
            string yValueMember;

            if (_view.TopSellingFilter == "TRIER PAR MONTANT TOTAL")
            {
                // Trié par montant total
                items = _reportRepository.GetTopSellingByAmount(start, end);
                yValueMember = "total";
            }
            else
            {
                // Trié par quantité
                items = _reportRepository.GetTopSellingByQty(start, end);
                yValueMember = "qty";
            }

            _view.LoadTopSellingItems(items);
            _view.LoadTopSellingChart(items, yValueMember);
        }

        public void LoadSoldItemsGrouped()
        {
            DateTime start = _view.SoldItemsDateFrom.Date;
            DateTime end = _view.SoldItemsDateTo.Date.AddDays(1).AddTicks(-1);

            var items = _reportRepository.GetSoldItemsGrouped(start, end);
            double total = _reportRepository.GetTotalRevenue(start, end);
            _view.LoadSoldItemsGrouped(items, total);
        }

        public void LoadInventory()
        {
            // Récupère tous les produits via ProductRepository
            // note: On a besoin d'une méthode GetAll ou Search("") sur IProductRepository
            var products = _productRepository.Search("");
            _view.LoadInventory(products);
        }

        public void LoadCriticalItems()
        {
            var products = _productRepository.GetCriticalProducts();
            _view.LoadCriticalItems(products);
        }

        public void LoadStockInHistory()
        {
            DateTime start = _view.StockInDateFrom.Date;
            DateTime end = _view.StockInDateTo.Date.AddDays(1).AddTicks(-1);

            var history = _stockRepository.GetStockInHistory(start, end);
            _view.LoadStockInHistory(history);
        }

        public void LoadCancelledOrders()
        {
            DateTime start = _view.CancelledOrdersDateFrom.Date;
            DateTime end = _view.CancelledOrdersDateTo.Date.AddDays(1).AddTicks(-1);

            var cancelled = _reportRepository.GetCancelledOrders(start, end);
            _view.LoadCancelledOrders(cancelled);
        }

        public void LoadCustomerWallets()
        {
            var wallets = _reportRepository.GetCustomerWallets(_view.WalletSearchPhone);
            _view.LoadCustomerWallets(wallets);
        }

        public void LoadWalletTransactions()
        {
            DateTime start = _view.WalletDateFrom.Date;
            DateTime end = _view.WalletDateTo.Date.AddDays(1).AddTicks(-1);
            var transactions = _reportRepository.GetWalletTransactions(start, end, _view.WalletSearchPhone);
            _view.LoadWalletTransactions(transactions);
        }

        public void LoadConsolidation(string centralConnStr)
        {
            DateTime start = _view.ConsolidationDateFrom.Date;
            DateTime end = _view.ConsolidationDateTo.Date.AddDays(1).AddTicks(-1);
            var revenues = _reportRepository.GetStoreRevenueConsolidated(start, end, centralConnStr);
            _view.LoadConsolidation(revenues);
        }
    }
}
