using System;
using System.Collections.Generic;
using System.Linq;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class SoldItemsPresenter
    {
        private readonly ISoldItemsView _view;
        private readonly IReportRepository _repository;

        public SoldItemsPresenter(ISoldItemsView view, IReportRepository repository)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Initialize()
        {
            var cashiers = new List<string> { "Tous les Caissiers" };
            cashiers.AddRange(_repository.GetCashiers());
            _view.LoadCashiers(cashiers);
            
            _view.DateFrom = DateTime.Now;
            _view.DateTo = DateTime.Now;
            
            LoadRecords();
        }

        public void LoadRecords()
        {
            DateTime start = _view.DateFrom.Date; // 00:00:00
            DateTime end = _view.DateTo.Date.AddDays(1).AddTicks(-1); // 23:59:59
            
            var items = _repository.GetSoldItems(start, end, _view.SelectedCashier);
            double total = items.Sum(x => x.Total);
            _view.LoadSoldItems(items, total);
        }
    }
}
