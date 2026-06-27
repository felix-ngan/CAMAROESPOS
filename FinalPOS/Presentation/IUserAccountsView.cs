using System;

namespace FinalPOS.Presentation
{
    public interface IUserAccountsView
    {
        // Create Account
        string NewUsername { get; set; }
        string NewPassword { get; set; }
        string NewConfirmPassword { get; set; }
        string NewRole { get; set; }
        string NewFullName { get; set; }

        // Change Password
        string CurrentUsername { get; set; }
        string OldPassword { get; set; }
        string NewPasswordChange { get; set; }
        string ConfirmNewPassword { get; set; }

        // Activate/Deactivate
        string SearchUsername { get; set; }
        bool IsAccountActive { get; set; }

        // Backup
        string BackupPath { get; set; }

        void CloseView();
        void ShowMessage(string message, string title, bool isError = false);
        bool ConfirmMessage(string message, string title);
        void ClearCreateAccountFields();
        void ClearChangePasswordFields();
        void ClearActivateFields();
    }
}
