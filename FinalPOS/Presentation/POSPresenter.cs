using System;
using System.Collections.Generic;
using System.Linq;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public class POSPresenter
    {
        private readonly IPOSView _view;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;

        public POSPresenter(IPOSView view, IProductRepository productRepository, ICartRepository cartRepository)
        {
            _view = view;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
        }

        public void NotifyCriticalItems()
        {
            try
            {
                var criticalProducts = _productRepository.GetCriticalProducts().ToList();
                if (criticalProducts.Any())
                {
                    string countText = criticalProducts.Count + " CRITICAL ITEM(S)";
                    string criticalItemsText = string.Join(
                        Environment.NewLine,
                        criticalProducts.Select((p, idx) => $"{idx + 1}.  {p.Description}")
                    );
                    _view.ShowCriticalAlert(countText, criticalItemsText);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void GetTransNo()
        {
            try
            {
                string sdate = DateTime.Now.ToString("yyyyMMdd");
                string lastTransNo = _cartRepository.GetLastTransactionNo(sdate);
                if (!string.IsNullOrEmpty(lastTransNo) && lastTransNo.Length > 8)
                {
                    string countStr = lastTransNo.Substring(8);
                    if (int.TryParse(countStr, out int count))
                    {
                        _view.TransactionNo = sdate + (count + 1);
                    }
                    else
                    {
                        _view.TransactionNo = sdate + "1001";
                    }
                }
                else
                {
                    _view.TransactionNo = sdate + "1001";
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void ScanBarcode()
        {
            try
            {
                string barcode = _view.SearchBarcode.Trim();
                if (string.IsNullOrEmpty(barcode)) return;

                var product = _productRepository.GetByBarcode(barcode);
                if (product != null)
                {
                    int qtyToAdd = 1;
                    if (int.TryParse(_view.Quantity, out int q) && q > 0)
                    {
                        qtyToAdd = q;
                    }

                    var existing = _cartRepository.GetPendingItem(_view.TransactionNo, product.ProductCode);
                    int currentInCart = existing?.Qty ?? 0;

                    if (product.Qty < (qtyToAdd + currentInCart))
                    {
                        _view.ShowMessage($"Impossible d'ajouter. La quantité disponible en stock est de {product.Qty}", "Attention", true);
                        return;
                    }

                    if (existing != null)
                    {
                        _cartRepository.UpdateCartQty(existing.Id, currentInCart + qtyToAdd);
                    }
                    else
                    {
                        _cartRepository.AddToCart(_view.TransactionNo, product.ProductCode, product.Price, qtyToAdd, _view.CashierName);
                    }

                    _view.SearchBarcode = "";
                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void LoadCart()
        {
            try
            {
                var items = _cartRepository.GetPendingCartItems(_view.TransactionNo).ToList();
                _view.PopulateCartGrid(items);

                if (items.Any())
                {
                    double total = items.Sum(item => item.Total);
                    double discount = items.Sum(item => item.DiscountAmount);
                    double vatRate = _cartRepository.GetVatRate();
                    double vat = total * vatRate;
                    double vatable = total - vat;

                    _view.SetCartTotals(total, discount, vat, vatable, total);
                    _view.CanSettle = true;
                    _view.CanDiscount = true;
                    _view.CanClearCart = true;
                }
                else
                {
                    _view.SetCartTotals(0, 0, 0, 0, 0);
                    _view.CanSettle = false;
                    _view.CanDiscount = false;
                    _view.CanClearCart = false;
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void AddCartItemQty(string pcode, int rowQty)
        {
            try
            {
                var products = _productRepository.Search(pcode).ToList();
                var product = products.FirstOrDefault(p => p.ProductCode == pcode);
                if (product == null) return;

                int qtyToAdd = 1;
                if (int.TryParse(_view.Quantity, out int q) && q > 0)
                {
                    qtyToAdd = q;
                }

                if (rowQty + qtyToAdd <= product.Qty)
                {
                    var existing = _cartRepository.GetPendingItem(_view.TransactionNo, pcode);
                    if (existing != null)
                    {
                        _cartRepository.UpdateCartQty(existing.Id, existing.Qty + qtyToAdd);
                        LoadCart();
                    }
                }
                else
                {
                    _view.ShowMessage($"La quantité disponible en stock est de {product.Qty} !", "Rupture de stock", true);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void RemoveCartItemQty(string pcode, int rowQty)
        {
            try
            {
                int qtyToRemove = 1;
                if (int.TryParse(_view.Quantity, out int q) && q > 0)
                {
                    qtyToRemove = q;
                }

                if (rowQty > 0)
                {
                    var existing = _cartRepository.GetPendingItem(_view.TransactionNo, pcode);
                    if (existing != null)
                    {
                        int newQty = Math.Max(0, existing.Qty - qtyToRemove);
                        _cartRepository.UpdateCartQty(existing.Id, newQty);
                        LoadCart();
                    }
                }
                else
                {
                    _view.ShowMessage($"La quantité restante dans le panier est de {rowQty} !", "Attention", true);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void DeleteCartItem(int id)
        {
            try
            {
                if (_view.ConfirmMessage("Retirer cet article du panier ?", "Retirer l'article"))
                {
                    _cartRepository.DeleteCartItem(id);
                    _view.ShowMessage("Article retiré avec succès.", "Système POS");
                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }

        public void ClearCart()
        {
            try
            {
                if (_view.ConfirmMessage("Voulez-vous retirer tous les articles du panier ?", "Vider le panier"))
                {
                    _cartRepository.ClearCart(_view.TransactionNo);
                    _view.ShowMessage("Le panier a été vidé avec succès.", "Succès");
                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "Attention", true);
            }
        }
    }
}
