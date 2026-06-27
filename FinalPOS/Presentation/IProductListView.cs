using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IProductListView
    {
        string SearchText { get; }
        void PopulateGrid(IEnumerable<Product> products);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
    }
}
