using System.Net.Http.Headers;

namespace BlazorWasm.Client.Services;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public JwtAuthenticationHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get the current token
        var token = await _authService.GetTokenAsync();

        // Add Authorization header if token exists
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 401 Unauthorized, try to refresh the token
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Check if this is not already a refresh request to avoid infinite loops
            if (!request.RequestUri?.AbsolutePath.Contains("/api/auth/refresh") == true)
            {
                // Try to refresh the token
                var refreshed = await TryRefreshTokenAsync();

                if (refreshed)
                {
                    // Get the new token and retry the original request
                    var newToken = await _authService.GetTokenAsync();

                    if (!string.IsNullOrEmpty(newToken))
                    {
                        // Clone the original request since HttpRequestMessage can only be sent once
                        var newRequest = await CloneRequestAsync(request);
                        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                        // Retry the request with the new token
                        response = await base.SendAsync(newRequest, cancellationToken);
                    }
                }
                else
                {
                    // Refresh failed, logout the user
                    await _authService.LogoutAsync();
                }
            }
        }

        return response;
    }

    private Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            // The AuthService will handle the refresh token logic
            // For now, this is a placeholder since the refresh mechanism
            // needs to be properly implemented with the server
            return Task.FromResult(false);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage originalRequest)
    {
        var clonedRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

        // Copy headers
        foreach (var header in originalRequest.Headers)
        {
            clonedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if it exists
        if (originalRequest.Content != null)
        {
            var contentBytes = await originalRequest.Content.ReadAsByteArrayAsync();
            clonedRequest.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in originalRequest.Content.Headers)
            {
                clonedRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy properties
        foreach (var property in originalRequest.Options)
        {
            clonedRequest.Options.TryAdd(property.Key, property.Value);
        }

        return clonedRequest;
    }
}
