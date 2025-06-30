using Microsoft.JSInterop;

namespace BlazorWasm.Client.Services;

public interface IConnectivityService
{
    bool IsOnline { get; }
    event Action<bool>? ConnectivityChanged;
    Task InitializeAsync();
    Task<bool> CheckConnectivityAsync();
    Task ClearCacheAsync();
    Task<CacheStatus> GetCacheStatusAsync();
}

public class ConnectivityService : IConnectivityService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ConnectivityService>? _dotNetRef;
    private bool _isOnline = true;

    public bool IsOnline => _isOnline;
    public event Action<bool>? ConnectivityChanged;

    public ConnectivityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            
            // Set up JavaScript interop for connectivity monitoring
            await _jsRuntime.InvokeVoidAsync("window.blazorCulture.setDotNetHelper", _dotNetRef);
            
            // Get initial connectivity status
            var status = await _jsRuntime.InvokeAsync<ConnectivityStatus>("window.serviceWorkerManager.getConnectivityStatus");
            _isOnline = status.IsOnline;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing connectivity service: {ex.Message}");
            // Default to online if we can't determine status
            _isOnline = true;
        }
    }

    public async Task<bool> CheckConnectivityAsync()
    {
        try
        {
            var status = await _jsRuntime.InvokeAsync<ConnectivityStatus>("window.serviceWorkerManager.getConnectivityStatus");
            _isOnline = status.IsOnline;
            return _isOnline;
        }
        catch
        {
            return _isOnline; // Return last known status
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.serviceWorkerManager.clearCaches");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    public async Task<CacheStatus> GetCacheStatusAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<CacheStatus>("window.serviceWorkerManager.getCacheStatus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting cache status: {ex.Message}");
            return new CacheStatus();
        }
    }

    [JSInvokable]
    public Task OnConnectivityChanged(bool isOnline)
    {
        _isOnline = isOnline;
        ConnectivityChanged?.Invoke(isOnline);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnServiceWorkerUpdate()
    {
        // Handle service worker updates
        // Could show a toast notification or update UI
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {        
        _dotNetRef?.Dispose();
        return ValueTask.CompletedTask;
    }
}

public class ConnectivityStatus
{
    public bool IsOnline { get; set; }
    public bool ServiceWorkerActive { get; set; }
}

public class CacheStatus
{
    public int ApiCacheSize { get; set; }
    public int StaticCacheSize { get; set; }
    public int TotalCacheSize { get; set; }
}
