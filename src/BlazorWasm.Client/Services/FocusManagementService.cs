using Microsoft.JSInterop;

namespace BlazorWasm.Client.Services;

public interface IFocusManagementService
{
    Task FocusElementAsync(string elementId);
    Task FocusFirstElementAsync(string containerId);
    Task TrapFocusAsync(string containerId);
    Task ReleaseFocusTrapAsync();
    Task SetFocusWithinAsync(string containerId);
    Task RestorePreviousFocusAsync();
    Task SaveCurrentFocusAsync();
    Task AnnounceLiveTextAsync(string text, string priority = "polite");
    Task MoveFocusToTopAsync();
    Task MoveFocusToMainContentAsync();
}

public class FocusManagementService : IFocusManagementService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<FocusManagementService> _logger;

    public FocusManagementService(IJSRuntime jsRuntime, ILogger<FocusManagementService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task FocusElementAsync(string elementId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.focusElement", elementId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to focus element with ID: {ElementId}", elementId);
        }
    }

    public async Task FocusFirstElementAsync(string containerId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.focusFirstElement", containerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to focus first element in container: {ContainerId}", containerId);
        }
    }

    public async Task TrapFocusAsync(string containerId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.trapFocus", containerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to trap focus in container: {ContainerId}", containerId);
        }
    }

    public async Task ReleaseFocusTrapAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.releaseFocusTrap");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to release focus trap");
        }
    }

    public async Task SetFocusWithinAsync(string containerId)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.setFocusWithin", containerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set focus within container: {ContainerId}", containerId);
        }
    }

    public async Task RestorePreviousFocusAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.restorePreviousFocus");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to restore previous focus");
        }
    }

    public async Task SaveCurrentFocusAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.saveCurrentFocus");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save current focus");
        }
    }

    public async Task AnnounceLiveTextAsync(string text, string priority = "polite")
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.announceLiveText", text, priority);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to announce live text: {Text}", text);
        }
    }

    public async Task MoveFocusToTopAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.moveFocusToTop");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to move focus to top");
        }
    }

    public async Task MoveFocusToMainContentAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("focusManagement.moveFocusToMainContent");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to move focus to main content");
        }
    }
}
