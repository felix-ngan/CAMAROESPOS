using CamaroesPOS.Application.Security;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Web.Services;

public class SessionState
{
    private readonly SessionTokenService _tokens;

    public SessionState(SessionTokenService? tokens = null)
    {
        _tokens = tokens ?? new SessionTokenService();
    }

    public User? CurrentUser { get; private set; }
    public string? CurrentToken { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => string.Equals(CurrentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public string DisplayName => CurrentUser == null ? "" : (string.IsNullOrWhiteSpace(CurrentUser.Name) ? CurrentUser.Username : CurrentUser.Name);
    public string Role => CurrentUser?.Role ?? "";

    public event Action? OnChange;

    public string Login(User user)
    {
        CurrentUser = user;
        CurrentToken = _tokens.CreateToken(user.Username, user.Role);
        OnChange?.Invoke();
        return CurrentToken;
    }

    public bool TryRestoreFromToken(string? token, PosStoreState store)
    {
        if (!_tokens.TryValidate(token, out var username, out var role))
            return false;

        var user = store.Users.FirstOrDefault(u => u.Username == username && u.IsActive && string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase));
        if (user == null) return false;

        CurrentUser = user;
        CurrentToken = token;
        OnChange?.Invoke();
        return true;
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentToken = null;
        OnChange?.Invoke();
    }
}
