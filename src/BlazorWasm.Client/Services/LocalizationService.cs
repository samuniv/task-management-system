using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Globalization;

namespace BlazorWasm.Client.Services
{
    public interface ILocalizationService
    {
        string GetString(string key);
        string GetString(string key, params object[] args);
        CultureInfo CurrentCulture { get; }
        Task SetCultureAsync(string cultureName);
        IEnumerable<CultureInfo> SupportedCultures { get; }
        event Action<CultureInfo>? CultureChanged;
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<LocalizationService> _localizer;
        private readonly IJSRuntime _jsRuntime;
        
        public CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

        public IEnumerable<CultureInfo> SupportedCultures => new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("fr-FR")
        };

        public event Action<CultureInfo>? CultureChanged;

        public LocalizationService(IStringLocalizer<LocalizationService> localizer, IJSRuntime jsRuntime)
        {
            _localizer = localizer;
            _jsRuntime = jsRuntime;
        }

        public string GetString(string key)
        {
            return _localizer[key].Value;
        }

        public string GetString(string key, params object[] args)
        {
            return _localizer[key, args].Value;
        }

        public async Task SetCultureAsync(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return;

            var culture = new CultureInfo(cultureName);
            
            // Set the culture for the current thread
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Store in local storage for persistence
            await SetCultureInStorageAsync(cultureName);

            // Notify components about culture change
            CultureChanged?.Invoke(culture);
        }

        private async Task SetCultureInStorageAsync(string cultureName)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "culture", cultureName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving culture to localStorage: {ex.Message}");
            }
        }

        public async Task<string?> GetSavedCultureAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "culture");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading culture from localStorage: {ex.Message}");
                return null;
            }
        }
    }
}
