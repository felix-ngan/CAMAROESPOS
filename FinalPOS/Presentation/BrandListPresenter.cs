using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class BrandListPresenter
    {
        private readonly IBrandListView _view;
        private readonly IBrandRepository _repository;

        public BrandListPresenter(IBrandListView view, IBrandRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadBrands()
        {
            try
            {
                var brands = _repository.GetAll();
                _view.PopulateGrid(brands);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void DeleteBrand(int id)
        {
            if (_view.ConfirmMessage("Voulez-vous vraiment supprimer cette marque ?", "Supprimer une Marque"))
            {
                try
                {
                    _repository.Delete(id);
                    _view.ShowMessage("La marque a été supprimée avec succès.", "POS");
                    LoadBrands();
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
        }
    }
}
