using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class StockInPresenter
    {
        private readonly IStockInView _view;
        private readonly IStockRepository _repository;

        public StockInPresenter(IStockInView view, IStockRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadVendors()
        {
            try
            {
                var vendors = _repository.GetVendors();
                var names = new List<string>();
                foreach (var v in vendors)
                {
                    names.Add(v.VendorName);
                }
                _view.SetVendors(names);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void LoadPendingStocks()
        {
            string refNo = _view.RefNo.Trim();
            if (string.IsNullOrEmpty(refNo))
            {
                _view.PopulatePendingGrid(new List<Stock>());
                return;
            }
            try
            {
                var stocks = _repository.GetPendingStocksByRefNo(refNo);
                _view.PopulatePendingGrid(stocks);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void LoadHistory()
        {
            try
            {
                var stocks = _repository.GetStockInHistory(_view.HistoryDateFrom, _view.HistoryDateTo);
                _view.PopulateHistoryGrid(stocks);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void OnVendorSelected()
        {
            string vendorName = _view.SelectedVendorName;
            if (string.IsNullOrEmpty(vendorName))
            {
                _view.VendorId = "";
                _view.ContactPerson = "";
                return;
            }
            try
            {
                var vendor = _repository.GetVendorByName(vendorName);
                if (vendor != null)
                {
                    _view.VendorId = vendor.Id.ToString();
                    _view.ContactPerson = vendor.ContactPerson;
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void DeleteStock(int id, bool isHistory)
        {
            if (_view.ConfirmMessage("Retirer cet article ?", "Système POS"))
            {
                try
                {
                    _repository.DeletePendingStock(id);
                    _view.ShowMessage("L'article a été retiré avec succès.", "Système POS");
                    if (isHistory)
                    {
                        LoadHistory();
                    }
                    else
                    {
                        LoadPendingStocks();
                    }
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
        }

        public bool SaveStockIn(IEnumerable<(int id, string pcode, int qty)> items)
        {
            if (_view.ConfirmMessage("Voulez-vous vraiment enregistrer cette entrée en stock ?", "Système POS"))
            {
                try
                {
                    foreach (var item in items)
                    {
                        _repository.CompleteStockIn(item.id, item.pcode, item.qty);
                    }
                    _view.ShowMessage("L'entrée en stock a été enregistrée avec succès.", "Succès");
                    _view.ClearView();
                    LoadPendingStocks();
                    return true;
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
            return false;
        }
    }
}
