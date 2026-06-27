
using System;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class DiscountPresenter
    {
        private readonly IDiscountView _view;
        private readonly ICartRepository _cartRepository;

        public DiscountPresenter(IDiscountView view, ICartRepository cartRepository)
        {
            _view = view;
            _cartRepository = cartRepository;
        }

        public void CalculateDiscount()
        {
            try
            {
                double priceVal = double.Parse(_view.Price.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);
                double discPercent = double.Parse(_view.DiscountPercent.Replace("%", "").Trim(), System.Globalization.NumberStyles.Any);
                
                double discount = priceVal * (discPercent / 100.0); // original: priceVal * discPercent, but let's see, if they input 10, does it mean 10% or 0.10?
                // Wait! In the original code:
                // double discount = priceVal * discPercent;
                // Where discPercent was double.Parse(txtDisocunt.Text).
                // Wait, if user types "10" for 10% discount, then priceVal * 10 would multiply by 10 instead of 10%. But wait! In the original:
                // "Assuming txtDisocunt.Text is the percentage (e.g. 10 for 10%)"
                // Actually, let's keep the exact original logic:
                // Double.Parse(txtPrice.Text) * Double.Parse(txtDisocunt.Text)
                // Wait! If they input 0.10 for 10%, or if they input 10 for 10% but price was divided, let's look at the original code in frmDiscount.cs:
                // double discount = priceVal * discPercent;
                // We will keep exactly `priceVal * discPercent` (or if it has % in text, it gets parsed). To avoid breaking any existing behavior, we will match the original calculation `priceVal * discPercent`.
                double discountAmount = priceVal * discPercent;
                _view.DiscountAmount = discountAmount.ToString("#,##0") + " FCFA";
            }
            catch
            {
                _view.DiscountAmount = "0 FCFA";
            }
        }

        public bool ConfirmDiscount()
        {
            try
            {
                if (string.IsNullOrEmpty(_view.CartId) || !int.TryParse(_view.CartId, out int cartId))
                {
                    _view.ShowMessage("ID de panier non valide.", "Attention", true);
                    return false;
                }

                if (_view.ConfirmMessage("Ajouter la remise ? Cliquez sur Oui pour confirmer", "Remise"))
                {
                    double discountAmount = double.Parse(_view.DiscountAmount.Replace(" FCFA", "").Trim(), System.Globalization.NumberStyles.Any);
                    double discountPercent = double.Parse(_view.DiscountPercent.Replace("%", "").Trim(), System.Globalization.NumberStyles.Any);

                    _cartRepository.ApplyDiscount(cartId, discountAmount, discountPercent);
                    _view.CloseView();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
            return false;
        }
    }
}
