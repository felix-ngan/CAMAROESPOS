using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class AdjustmentPresenter
    {
        private readonly IAdjustmentView _view;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public AdjustmentPresenter(IAdjustmentView view, IProductRepository productRepository, IStockRepository stockRepository)
        {
            _view = view;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
        }

        public void GenerateReferenceNo()
        {
            Random rnd = new Random();
            _view.ReferenceNo = rnd.Next().ToString();
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
                _view.ShowMessage(ex.Message, "ATTENTION", true);
            }
        }

        public void SaveAdjustment()
        {
            try
            {
                string refNo = _view.ReferenceNo;
                string pcode = _view.ProductCode;
                string qtyStr = _view.Qty;
                string action = _view.Action;
                string remarks = _view.Remarks;
                string user = _view.CurrentUser;
                int availableStock = _view.AvailableStock;

                if (string.IsNullOrEmpty(pcode))
                {
                    _view.ShowMessage("Veuillez sélectionner un produit dans la liste.", "ATTENTION", true);
                    return;
                }

                if (string.IsNullOrEmpty(action))
                {
                    _view.ShowMessage("Veuillez sélectionner une action (RETIRER DU STOCK ou AJOUTER AU STOCK).", "ATTENTION", true);
                    return;
                }

                if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
                {
                    _view.ShowMessage("Veuillez saisir une quantité d'ajustement valide (supérieure à 0).", "ATTENTION", true);
                    return;
                }

                if (qty > availableStock)
                {
                    _view.ShowMessage("LA QUANTITÉ EN STOCK DOIT ÊTRE SUPÉRIEURE À LA QUANTITÉ D'AJUSTEMENT", "ATTENTION", true);
                    return;
                }

                _stockRepository.AdjustStock(refNo, pcode, qty, action, remarks, DateTime.Now, user);

                _view.ShowMessage("LE STOCK A ÉTÉ AJUSTÉ AVEC SUCCÈS", "SUCCÈS");
                _view.ClearFields();
                LoadProducts();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "ATTENTION", true);
            }
        }
    }
}
