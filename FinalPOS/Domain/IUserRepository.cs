using System;

namespace FinalPOS.Domain
{
    public interface IUserRepository
    {
        User Authenticate(string username, string password);
        string GetPassword(string username);
        void AddUser(string username, string password, string role, string name);
        void UpdatePassword(string username, string password);
        bool GetUserActivationStatus(string username);
        bool UserExists(string username);
        void UpdateUserActivationStatus(string username, bool isActive);
        void BackupDatabase(string backupPath);
    }
}
