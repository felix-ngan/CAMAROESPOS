using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class LookUpPresenter
    {
        private readonly ILookUpView _view;
        private readonly IProductRepository _productRepository;

        public LookUpPresenter(ILookUpView view, IProductRepository productRepository)
        {
            _view = view;
            _productRepository = productRepository;
        }

        public void LoadProducts()
        {
            try
            {
                var products = _productRepository.Search(_view.SearchText);
                _view.PopulateProducts(products);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }
    }
}
