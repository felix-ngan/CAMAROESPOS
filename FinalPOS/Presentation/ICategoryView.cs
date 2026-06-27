namespace FinalPOS.Presentation
{
    public interface ICategoryView
    {
        string CategoryId { get; set; }
        string CategoryName { get; set; }
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
        void CloseView();
    }
}
