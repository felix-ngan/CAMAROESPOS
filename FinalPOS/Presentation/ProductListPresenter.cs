using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class ProductListPresenter
    {
        private readonly IProductListView _view;
        private readonly IProductRepository _repository;

        public ProductListPresenter(IProductListView view, IProductRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadProducts()
        {
            try
            {
                var products = _repository.Search(_view.SearchText);
                _view.PopulateGrid(products);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void DeleteProduct(string pcode)
        {
            if (_view.ConfirmMessage("Voulez-vous vraiment supprimer ce produit ?", "Supprimer un Produit"))
            {
                try
                {
                    _repository.Delete(pcode);
                    _view.ShowMessage("Le produit a été supprimé avec succès.", "POS");
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
        }
    }
}
