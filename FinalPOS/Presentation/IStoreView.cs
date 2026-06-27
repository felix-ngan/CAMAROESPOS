namespace FinalPOS.Presentation
{
    public interface IStoreView
    {
        string StoreName { get; set; }
        string StoreAddress { get; set; }
        byte[] StoreLogo { get; set; }
        string MomoCode { get; set; }
        string OrangeCode { get; set; }
        string StoreId { get; set; }
        string RegisterId { get; set; }
        string CentralConnection { get; set; }
        void CloseView();
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
    }
}
