namespace FinalPOS.Presentation
{
    public interface IVendorView
    {
        string VendorId { get; set; }
        string VendorName { get; set; }
        string Address { get; set; }
        string ContactPerson { get; set; }
        string Phone { get; set; }
        string Email { get; set; }
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearView();
        void CloseView();
    }
}
