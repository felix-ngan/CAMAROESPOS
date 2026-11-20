using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public class SessionState
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => string.Equals(CurrentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public string DisplayName => CurrentUser == null ? "" : (string.IsNullOrWhiteSpace(CurrentUser.Name) ? CurrentUser.Username : CurrentUser.Name);
    public string Role => CurrentUser?.Role ?? "";

    public event Action? OnChange;

    public void Login(User user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }

    public bool TryRestoreFromStore(string? username, PosStoreState store)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        var user = store.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
        if (user == null) return false;
        CurrentUser = user;
        OnChange?.Invoke();
        return true;
    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }
}
