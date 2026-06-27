using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class QtyPresenter
    {
        private readonly IQtyView _view;
        private readonly ICartRepository _cartRepository;

        public QtyPresenter(IQtyView view, ICartRepository cartRepository)
        {
            _view = view;
            _cartRepository = cartRepository;
        }

        public bool AddToCart()
        {
            try
            {
                if (!int.TryParse(_view.Quantity, out int qtyToAdd) || qtyToAdd <= 0)
                {
                    _view.ShowMessage("Veuillez saisir une quantité valide (supérieure à 0).", "Attention", true);
                    return false;
                }

                string transNo = _view.TransactionNo;
                string pcode = _view.ProductCode;
                double price = _view.Price;
                int availableStock = _view.AvailableStock;
                string cashier = _view.CashierName;

                var existingItem = _cartRepository.GetPendingItem(transNo, pcode);
                int existingQty = existingItem?.Qty ?? 0;

                if (availableStock < (qtyToAdd + existingQty))
                {
                    _view.ShowMessage($"Impossible d'ajouter. La quantité disponible en stock est de {availableStock}", "Attention", true);
                    return false;
                }

                if (existingItem != null)
                {
                    _cartRepository.UpdateCartQty(existingItem.Id, existingQty + qtyToAdd);
                }
                else
                {
                    _cartRepository.AddToCart(transNo, pcode, price, qtyToAdd, cashier);
                }

                _view.CloseView();
                return true;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
            return false;
        }
    }
}
