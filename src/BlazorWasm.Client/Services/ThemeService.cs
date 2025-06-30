using Microsoft.JSInterop;

namespace BlazorWasm.Client.Services;

public interface IThemeService
{
    event EventHandler<string>? ThemeChanged;
    Task<string> GetCurrentThemeAsync();
    Task<string> GetEffectiveThemeAsync();
    Task SetThemeAsync(string theme);
    Task ToggleThemeAsync();
    Task ToggleHighContrastAsync();
    Task<bool> IsHighContrastActiveAsync();
    Task<bool> IsDarkModeActiveAsync();
    Task<bool> IsSystemManagedAsync();
    Task InitializeThemeAsync();
    Task<object[]> GetAvailableThemesAsync();
    Task ResetToSystemPreferenceAsync();
}

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ThemeService> _logger;
    private string _currentTheme = "auto";

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
            _currentTheme = theme ?? "auto";
            return _currentTheme;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get current theme");
            return "auto";
        }
    }

    public async Task<string> GetEffectiveThemeAsync()
    {
        try
        {
            var effectiveTheme = await _jsRuntime.InvokeAsync<string>("themeManager.getEffectiveTheme");
            return effectiveTheme ?? "light";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get effective theme");
            return "light";
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

    public async Task ToggleThemeAsync()
    {
        try
        {
            var newTheme = await _jsRuntime.InvokeAsync<string>("themeManager.toggleTheme");
            _currentTheme = newTheme;
            ThemeChanged?.Invoke(this, newTheme);
            
            _logger.LogInformation("Theme toggled to: {Theme}", newTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle theme");
        }
    }

    public async Task ToggleHighContrastAsync()
    {
        try
        {
            var newTheme = await _jsRuntime.InvokeAsync<string>("themeManager.toggleHighContrast");
            _currentTheme = newTheme;
            ThemeChanged?.Invoke(this, newTheme);
            
            _logger.LogInformation("High contrast toggled to: {Theme}", newTheme);
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
            return await _jsRuntime.InvokeAsync<bool>("themeManager.userPrefersHighContrast");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if high contrast is active");
            return false;
        }
    }

    public async Task<bool> IsDarkModeActiveAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("themeManager.userPrefersDarkMode");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if dark mode is active");
            return false;
        }
    }

    public async Task<bool> IsSystemManagedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("themeManager.isSystemManaged");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if theme is system managed");
            return false;
        }
    }

    public async Task<object[]> GetAvailableThemesAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<object[]>("themeManager.getAvailableThemes");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get available themes");
            return Array.Empty<object>();
        }
    }

    public async Task ResetToSystemPreferenceAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("themeManager.resetToSystemPreference");
            _currentTheme = "auto";
            ThemeChanged?.Invoke(this, "auto");
            
            _logger.LogInformation("Theme reset to system preference");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset theme to system preference");
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
