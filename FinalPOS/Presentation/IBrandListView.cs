using System.Collections.Generic;
using FinalPOS.Domain;

namespace FinalPOS.Presentation
{
    public interface IBrandListView
    {
        void PopulateGrid(IEnumerable<Brand> brands);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
    }
}
