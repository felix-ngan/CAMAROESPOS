using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class CategoryPresenter
    {
        private readonly ICategoryView _view;
        private readonly ICategoryRepository _repository;

        public CategoryPresenter(ICategoryView view, ICategoryRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public bool Save()
        {
            string categoryName = _view.CategoryName.Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                _view.ShowMessage("Le nom de la catégorie ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (_repository.Exists(categoryName))
            {
                _view.ShowMessage("Cette catégorie existe déjà.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment enregistrer cette catégorie ?", "Enregistrer la catégorie"))
            {
                try
                {
                    var category = new Category { CategoryName = categoryName };
                    _repository.Add(category);
                    _view.ShowMessage("La catégorie a été enregistrée avec succès.", "Enregistrer la catégorie");
                    _view.ClearView();
                    return true;
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
            return false;
        }

        public bool Update()
        {
            string categoryName = _view.CategoryName.Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                _view.ShowMessage("Le nom de la catégorie ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (!int.TryParse(_view.CategoryId, out int id))
            {
                _view.ShowMessage("ID de catégorie invalide.", "Erreur", true);
                return false;
            }

            if (_repository.Exists(categoryName, id))
            {
                _view.ShowMessage("Une autre catégorie possède déjà ce nom.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment modifier cette catégorie ?", "Modifier la catégorie"))
            {
                try
                {
                    var category = new Category { Id = id, CategoryName = categoryName };
                    _repository.Update(category);
                    _view.ShowMessage("La catégorie a été modifiée avec succès.", "Modifier la catégorie");
                    _view.ClearView();
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
