using Microsoft.JSInterop;
using BlazorWasm.Shared.DTOs;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace BlazorWasm.Client.Services;

public class AuthService
{
    private readonly ApiService _apiService;
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(ApiService apiService, IJSRuntime jsRuntime)
    {
        _apiService = apiService;
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public event Action? AuthenticationStateChanged;

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _apiService.LoginAsync(loginRequest);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Store the JWT token in session storage
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", response.Token);

                // Create user profile from login response
                var userProfile = new UserProfileDto
                {
                    Id = 0, // Will be set from JWT claims
                    Email = response.Email,
                    FirstName = response.FirstName,
                    LastName = response.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                var userJson = JsonSerializer.Serialize(userProfile, _jsonOptions);
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUser", userJson);

                // Notify authentication state changed
                AuthenticationStateChanged?.Invoke();

                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Call the logout endpoint to invalidate refresh token
            await _apiService.LogoutAsync();
        }
        catch (Exception)
        {
            // Continue with logout even if API call fails
        }

        // Remove tokens and user info from storage
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "currentUser");

        // Notify authentication state changed
        AuthenticationStateChanged?.Invoke();
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "authToken");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserProfileDto?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "currentUser");

            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<UserProfileDto>(userJson, _jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrEmpty(token))
            return false;

        // Check if token is expired
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            return jsonToken.ValidTo > DateTime.UtcNow;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<ClaimsPrincipal> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrEmpty(token))
            return new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            // Check if token is expired
            if (jsonToken.ValidTo <= DateTime.UtcNow)
            {
                // Try to refresh the token
                var refreshed = await RefreshTokenAsync();
                if (!refreshed)
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }

                // Get the new token
                token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }

                jsonToken = handler.ReadJwtToken(token);
            }

            var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var success = await _apiService.RefreshTokenAsync();

            if (success)
            {
                // The refresh token endpoint should set new tokens in cookies
                // For now, we'll just return success - the token handling will be
                // improved when we implement the HTTP delegating handler
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
