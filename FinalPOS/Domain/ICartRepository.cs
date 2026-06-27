using System;
using System.Collections.Generic;

namespace FinalPOS.Domain
{
    public interface ICartRepository
    {
        IEnumerable<CartItem> GetPendingCartItems(string transNo);
        string GetLastTransactionNo(string datePrefix);
        CartItem GetPendingItem(string transNo, string pcode);
        void AddToCart(string transNo, string pcode, double price, int qty, string cashier);
        void UpdateCartQty(int id, int qty);
        void UpdateCartQtyByPcode(string transNo, string pcode, int qty);
        void DeleteCartItem(int id);
        void ClearCart(string transNo);
        void ApplyDiscount(int id, double discountAmount, double discountPercent);
        void SettlePayment(string transNo, string cashier, IEnumerable<(int cartId, string pcode, int qty)> items, string customerPhone, double walletDeduction, double walletCredit);
        double GetWalletBalance(string phone);
        void UpdateWalletBalance(string phone, double amount);
        double GetVatRate();
    }
}
