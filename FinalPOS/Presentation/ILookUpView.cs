using System;
using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface ILookUpView
    {
        string SearchText { get; }

        void PopulateProducts(IEnumerable<Product> products);
        void ShowMessage(string message, string title, bool isError = false);
        void CloseView();
    }
}
