using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class SearchProductStockInPresenter
    {
        private readonly ISearchProductStockInView _view;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public SearchProductStockInPresenter(ISearchProductStockInView view, IProductRepository productRepository, IStockRepository stockRepository)
        {
            _view = view;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
        }

        public void LoadProducts()
        {
            try
            {
                var products = _productRepository.Search(_view.SearchText);
                _view.PopulateGrid(products);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public bool SelectProduct(string pcode)
        {
            string refNo = _view.RefNo.Trim();
            string stockInBy = _view.StockInBy.Trim();
            string vendorIdStr = _view.VendorId;

            if (string.IsNullOrEmpty(refNo))
            {
                _view.ShowMessage("Veuillez saisir le numéro de référence.", "Champs requis", true);
                return false;
            }

            if (string.IsNullOrEmpty(stockInBy))
            {
                _view.ShowMessage("Veuillez saisir le nom de la personne effectuant l'entrée en stock.", "Champs requis", true);
                return false;
            }

            if (string.IsNullOrEmpty(vendorIdStr) || !int.TryParse(vendorIdStr, out int vendorId))
            {
                _view.ShowMessage("Veuillez sélectionner un fournisseur valide.", "Champs requis", true);
                return false;
            }

            if (_view.ConfirmMessage("Ajouter cet article ?", "Système POS"))
            {
                try
                {
                    _stockRepository.AddPendingStock(refNo, pcode, _view.StockInDate, stockInBy, vendorId);
                    _view.ShowMessage("Article ajouté avec succès.", "Succès");
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
