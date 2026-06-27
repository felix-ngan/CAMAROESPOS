using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class BrandPresenter
    {
        private readonly IBrandView _view;
        private readonly IBrandRepository _repository;

        public BrandPresenter(IBrandView view, IBrandRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public bool Save()
        {
            string brandName = _view.BrandName.Trim();
            if (string.IsNullOrEmpty(brandName))
            {
                _view.ShowMessage("Le nom de la marque ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (_repository.Exists(brandName))
            {
                _view.ShowMessage("Cette marque existe déjà.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment enregistrer cette marque ?", "Enregistrer la marque"))
            {
                try
                {
                    var brand = new Brand { BrandName = brandName };
                    _repository.Add(brand);
                    _view.ShowMessage("La marque a été enregistrée avec succès.", "Enregistrer la marque");
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
            string brandName = _view.BrandName.Trim();
            if (string.IsNullOrEmpty(brandName))
            {
                _view.ShowMessage("Le nom de la marque ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (!int.TryParse(_view.BrandId, out int id))
            {
                _view.ShowMessage("ID de marque invalide.", "Erreur", true);
                return false;
            }

            if (_repository.Exists(brandName, id))
            {
                _view.ShowMessage("Une autre marque possède déjà ce nom.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment modifier cette marque ?", "Modifier la marque"))
            {
                try
                {
                    var brand = new Brand { Id = id, BrandName = brandName };
                    _repository.Update(brand);
                    _view.ShowMessage("La marque a été modifiée avec succès.", "Modifier la marque");
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
