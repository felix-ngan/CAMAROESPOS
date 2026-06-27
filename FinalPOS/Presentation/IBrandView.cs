namespace FinalPOS.Presentation
{
    public interface IBrandView
    {
        string BrandId { get; set; }
        string BrandName { get; set; }
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
        void CloseView();
    }
}
