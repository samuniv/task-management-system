using Microsoft.JSInterop;

namespace BlazorWasm.Client.Services;

public interface IThemeService
{
    event EventHandler<string>? ThemeChanged;
    Task<string> GetCurrentThemeAsync();
    Task SetThemeAsync(string theme);
    Task ToggleHighContrastAsync();
    Task<bool> IsHighContrastActiveAsync();
    Task InitializeThemeAsync();
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ThemeService> _logger;
    private string _currentTheme = "default";

    public event EventHandler<string>? ThemeChanged;

    public ThemeService(IJSRuntime jsRuntime, ILogger<ThemeService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<string> GetCurrentThemeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("themeManager.getCurrentTheme");
            _currentTheme = theme ?? "default";
            return _currentTheme;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get current theme");
            return "default";
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        try
        {
            if (theme != _currentTheme)
            {
                await _jsRuntime.InvokeVoidAsync("themeManager.setTheme", theme);
                _currentTheme = theme;
                ThemeChanged?.Invoke(this, theme);
                
                _logger.LogInformation("Theme changed to: {Theme}", theme);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set theme to: {Theme}", theme);
        }
    }

    public async Task ToggleHighContrastAsync()
    {
        try
        {
            var currentTheme = await GetCurrentThemeAsync();
            var newTheme = currentTheme == "high-contrast" ? "default" : "high-contrast";
            await SetThemeAsync(newTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle high contrast theme");
        }
    }

    public async Task<bool> IsHighContrastActiveAsync()
    {
        try
        {
            var theme = await GetCurrentThemeAsync();
            return theme == "high-contrast";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if high contrast is active");
            return false;
        }
    }

    public async Task InitializeThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.initialize");
            _currentTheme = await GetCurrentThemeAsync();
            
            _logger.LogInformation("Theme service initialized with theme: {Theme}", _currentTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize theme service");
        }
    }
}
