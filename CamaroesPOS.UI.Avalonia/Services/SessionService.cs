using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.Avalonia.Services;

public static class SessionService
{
    public static string? CurrentUser { get; private set; }
    public static string? CurrentUserName { get; private set; }
    public static string? CurrentUserRole { get; private set; }

    public static bool IsAdmin => string.Equals(CurrentUserRole, "Admin", System.StringComparison.OrdinalIgnoreCase);

    public static void SetSession(User user)
    {
        CurrentUser = user.Username;
        CurrentUserName = string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name;
        CurrentUserRole = user.Role;
    }

    public static void Clear()
    {
        CurrentUser = null;
        CurrentUserName = null;
        CurrentUserRole = null;
    }
}
