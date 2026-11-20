using Microsoft.JSInterop;

namespace CamaroesPOS.Web.Services;

public class BrowserStorageService
{
    private const string StorageKey = "camaroes_pos_web_v1";
    private readonly IJSRuntime _js;

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

    public async Task PersistAsync(PosStoreState store)
    {
        try
        {
            await _js.InvokeVoidAsync("posInterop.setItem", StorageKey, store.ExportSnapshotJson());
        }
        catch
        {
            // Private browsing / quota exceeded fallback: keep running in memory
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
            // Ignore storage errors
        }
        store.SeedDemoData();
        await PersistAsync(store);
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
