using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class CategoryListPresenter
    {
        private readonly ICategoryListView _view;
        private readonly ICategoryRepository _repository;

        public CategoryListPresenter(ICategoryListView view, ICategoryRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadCategories()
        {
            try
            {
                var categories = _repository.GetAll();
                _view.PopulateGrid(categories);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void DeleteCategory(int id)
        {
            if (_view.ConfirmMessage("Voulez-vous vraiment supprimer cette catégorie ?", "Supprimer une Catégorie"))
            {
                try
                {
                    _repository.Delete(id);
                    _view.ShowMessage("La catégorie a été supprimée avec succès.", "POS");
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
        }
    }
}
