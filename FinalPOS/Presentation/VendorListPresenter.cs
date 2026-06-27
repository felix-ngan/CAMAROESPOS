using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class VendorListPresenter
    {
        private readonly IVendorListView _view;
        private readonly IVendorRepository _repository;

        public VendorListPresenter(IVendorListView view, IVendorRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        public void LoadVendors()
        {
            try
            {
                var vendors = _repository.GetAll();
                _view.PopulateGrid(vendors);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Erreur", true);
            }
        }

        public void DeleteVendor(int id)
        {
            if (_view.ConfirmMessage("Voulez-vous vraiment supprimer ce fournisseur ?", "Supprimer un Fournisseur"))
            {
                try
                {
                    _repository.Delete(id);
                    _view.ShowMessage("Le fournisseur a été supprimé avec succès.", "Supprimer un Fournisseur");
                    LoadVendors();
                }
                catch (Exception ex)
                {
                    _view.ShowMessage(ex.Message, "Erreur", true);
                }
            }
        }
    }
}
