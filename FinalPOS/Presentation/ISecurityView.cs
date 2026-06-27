using System;

namespace FinalPOS.Presentation
{
    public interface ISecurityView
    {
        string Username { get; set; }
        string Password { get; set; }
        void ShowMessage(string message, string title, bool isError);
        void ClearFields();
        void FocusUsername();
        void HideView();
    }
}
