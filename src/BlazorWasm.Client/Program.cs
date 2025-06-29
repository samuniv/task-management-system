using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWasm.Client;
using BlazorWasm.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure root component
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure base HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Register API service
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
