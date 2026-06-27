using System.Collections.Generic;

namespace FinalPOS.Presentation
{
    public interface IProductView
    {
        string ProductCode { get; set; }
        string Barcode { get; set; }
        string Description { get; set; }
        string BrandName { get; set; }
        string CategoryName { get; set; }
        string Price { get; set; }
        string Reorder { get; set; }
        void SetBrands(IEnumerable<string> brands);
        void SetCategories(IEnumerable<string> categories);
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
        void CloseView();
    }
}
