using Microsoft.JSInterop;

namespace CamaroesPOS.Web.Services;

public class BrowserStorageService
{
    private const string StorageKey = "camaroes_pos_web_v2";
    private const string SessionTokenKey = "camaroes_pos_session_token_v2";
    private const string ThemeKey = "camaroes_pos_theme_v2";
    private const string LangKey = "camaroes_pos_lang_v2";

    private readonly IJSRuntime _js;

    public bool StorageQuotaExceeded { get; private set; }
    public event Action? OnStorageWarning;

    public BrowserStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task LoadIntoAsync(PosStoreState store)
    {
        if (store.IsInitialized) return;
        try
        {
            var json = await _js.InvokeAsync<string?>("posInterop.getItem", StorageKey);
            store.ImportSnapshotJson(json);
            if (string.IsNullOrWhiteSpace(json))
            {
                await PersistAsync(store);
            }
        }
        catch
        {
            store.SeedDemoData();
        }
    }

    public async Task RestoreSessionAsync(SessionState session, PosStoreState store)
    {
        if (session.IsAuthenticated) return;
        try
        {
            var token = await _js.InvokeAsync<string?>("posInterop.getSessionItem", SessionTokenKey);
            if (!session.TryRestoreFromToken(token, store) && !string.IsNullOrWhiteSpace(token))
            {
                // Clear invalid/tampered/expired token
                await ClearSessionAsync();
            }
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task SaveSessionTokenAsync(string token)
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.setSessionItem", SessionTokenKey, token);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task ClearSessionAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.removeSessionItem", SessionTokenKey);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task<bool> PersistAsync(PosStoreState store)
    {
        try
        {
            bool saved = await _js.InvokeAsync<bool>("posInterop.setItem", StorageKey, store.ExportSnapshotJson());
            StorageQuotaExceeded = !saved;
            if (!saved) OnStorageWarning?.Invoke();
            return saved;
        }
        catch
        {
            StorageQuotaExceeded = true;
            OnStorageWarning?.Invoke();
            return false;
        }
    }

    public async Task ResetAsync(PosStoreState store)
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.removeItem", StorageKey);
        }
        catch
        {
            // Ignore
        }
        StorageQuotaExceeded = false;
        store.SeedDemoData();
        await PersistAsync(store);
    }

    public async Task<string> LoadThemeAsync()
    {
        try
        {
            var theme = await _js.InvokeAsync<string?>("posInterop.getItem", ThemeKey);
            var resolved = theme == "dark" ? "dark" : "light";
            await _js.InvokeVoidAsync("posInterop.setTheme", resolved);
            return resolved;
        }
        catch
        {
            return "light";
        }
    }

    public async Task SaveThemeAsync(string theme)
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.setTheme", theme);
            await _js.InvokeVoidAsync("posInterop.setItem", ThemeKey, theme);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task<string?> LoadLangAsync()
    {
        try { return await _js.InvokeAsync<string?>("posInterop.getItem", LangKey); }
        catch { return null; }
    }

    public async Task SaveLangAsync(string lang)
    {
        try { await _js.InvokeVoidAsync("posInterop.setItem", LangKey, lang); }
        catch { }
    }

    public async Task DownloadFileAsync(string filename, string content, string mimeType = "text/plain;charset=utf-8")
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.downloadFile", filename, content, mimeType);
        }
        catch
        {
            // Ignore
        }
    }

    public async Task PrintAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.printWindow");
        }
        catch
        {
            // Ignore
        }
    }
}
