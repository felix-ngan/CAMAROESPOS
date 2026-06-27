using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class StorePresenter
    {
        private readonly IStoreView _view;
        private readonly IStoreRepository _storeRepository;

        public StorePresenter(IStoreView view, IStoreRepository storeRepository)
        {
            _view = view;
            _storeRepository = storeRepository;
        }

        public void LoadStoreDetails()
        {
            try
            {
                var store = _storeRepository.GetStoreDetails();
                if (store != null)
                {
                    _view.StoreName = store.StoreName;
                    _view.StoreAddress = store.Address;
                    _view.StoreLogo = store.Logo;
                    _view.MomoCode = store.MomoCode;
                    _view.OrangeCode = store.OrangeCode;
                }
                else
                {
                    _view.StoreName = "";
                    _view.StoreAddress = "";
                    _view.StoreLogo = null;
                    _view.MomoCode = "";
                    _view.OrangeCode = "";
                }

                _view.StoreId = _storeRepository.GetLocalSetting("StoreId");
                _view.RegisterId = _storeRepository.GetLocalSetting("RegisterId");
                _view.CentralConnection = _storeRepository.GetLocalSetting("CentralConnectionString");
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void SaveStoreDetails()
        {
            try
            {
                string name = _view.StoreName.Trim();
                string address = _view.StoreAddress.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    _view.ShowMessage("Le nom de la boutique ne peut pas être vide.", "Attention", true);
                    return;
                }

                if (_view.ConfirmMessage("Enregistrer les paramètres de la boutique ?", "Paramètres Boutique"))
                {
                    var store = new Store 
                    { 
                        StoreName = name, 
                        Address = address, 
                        Logo = _view.StoreLogo,
                        MomoCode = _view.MomoCode,
                        OrangeCode = _view.OrangeCode
                    };
                    _storeRepository.SaveStoreDetails(store);
                    
                    _storeRepository.SaveLocalSetting("StoreId", _view.StoreId.Trim());
                    _storeRepository.SaveLocalSetting("RegisterId", _view.RegisterId.Trim());
                    _storeRepository.SaveLocalSetting("CentralConnectionString", _view.CentralConnection.Trim());

                    _view.ShowMessage("Les détails de la boutique ont été enregistrés avec succès.", "Enregistrement réussi");
                    _view.CloseView();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }
    }
}
