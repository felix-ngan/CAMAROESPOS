using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface ICategoryListView
    {
        void PopulateGrid(IEnumerable<Category> categories);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
    }
}
