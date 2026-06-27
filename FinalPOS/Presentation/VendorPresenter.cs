using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class VendorPresenter
    {
        private readonly IVendorView _view;
        private readonly IVendorRepository _repository;

        public VendorPresenter(IVendorView view, IVendorRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public bool Save()
        {
            string vendorName = _view.VendorName.Trim();
            if (string.IsNullOrEmpty(vendorName))
            {
                _view.ShowMessage("Le nom du fournisseur ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (_repository.Exists(vendorName))
            {
                _view.ShowMessage("Ce fournisseur existe déjà.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous vraiment enregistrer ce fournisseur ?", "Confirmer"))
            {
                try
                {
                    var vendor = new Vendor
                    {
                        VendorName = vendorName,
                        Address = _view.Address.Trim(),
                        ContactPerson = _view.ContactPerson.Trim(),
                        Phone = _view.Phone.Trim(),
                        Email = _view.Email.Trim()
                    };
                    _repository.Add(vendor);
                    _view.ShowMessage("Fournisseur enregistré avec succès.", "Succès");
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
            string vendorName = _view.VendorName.Trim();
            if (string.IsNullOrEmpty(vendorName))
            {
                _view.ShowMessage("Le nom du fournisseur ne peut pas être vide.", "Champs requis", true);
                return false;
            }

            if (!int.TryParse(_view.VendorId, out int id))
            {
                _view.ShowMessage("ID de fournisseur invalide.", "Erreur", true);
                return false;
            }

            if (_repository.Exists(vendorName, id))
            {
                _view.ShowMessage("Un autre fournisseur possède déjà ce nom.", "Erreur de doublon", true);
                return false;
            }

            if (_view.ConfirmMessage("Voulez-vous modifier ce fournisseur ?", "Confirmer"))
            {
                try
                {
                    var vendor = new Vendor
                    {
                        Id = id,
                        VendorName = vendorName,
                        Address = _view.Address.Trim(),
                        ContactPerson = _view.ContactPerson.Trim(),
                        Phone = _view.Phone.Trim(),
                        Email = _view.Email.Trim()
                    };
                    _repository.Update(vendor);
                    _view.ShowMessage("Fournisseur modifié avec succès.", "Succès");
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
