using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorWasm.Client;
using BlazorWasm.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure root component
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure base HttpClient for API calls with JWT handler
builder.Services.AddScoped<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<JwtAuthenticationHandler>();

// Also register a basic HttpClient for services that don't need authentication
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient();
});

// Register API service (already registered via typed HttpClient above)
// builder.Services.AddScoped<ApiService>();

// Register authentication service
builder.Services.AddScoped<AuthService>();

// Register authentication state provider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Add authorization services
builder.Services.AddAuthorizationCore();

// Register notification service
builder.Services.AddSingleton<INotificationService, NotificationService>();

await builder.Build().RunAsync();
