using BlazorWasm.Client.Pages;
using BlazorWasm.Server.Components;
using BlazorWasm.Server.Data;
using BlazorWasm.Server.Services;
using BlazorWasm.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using System.Text;
using Serilog;
using Serilog.Events;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Azure.AI.OpenAI;
using Azure;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/taskmanager-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

try
{
    Log.Information("Starting Task Management API");

    // Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("TasksDb");
});

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MySecretKeyThatIsAtLeast32CharactersLongForDevelopment!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagerClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Admin-only access policy
    options.AddPolicy("AdminAccess", policy =>
        policy.RequireRole("Admin"));

    // User access policy (both Admin and User roles)
    options.AddPolicy("UserAccess", policy =>
        policy.RequireRole("Admin", "User"));

    // Task management policies
    options.AddPolicy("CanManageTasks", policy =>
        policy.RequireRole("Admin", "User"));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("Admin"));

    // Analytics and reporting access
    options.AddPolicy("CanViewAnalytics", policy =>
        policy.RequireRole("Admin"));

    // Comment management
    options.AddPolicy("CanManageComments", policy =>
        policy.RequireRole("Admin", "User"));
});

// Register services
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure Azure OpenAI services
builder.Services.AddSingleton<AzureOpenAIClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["AzureOpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
    var endpoint = configuration["AzureOpenAI:Endpoint"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    
    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
    {
        // Log warning but don't fail startup - fallback to mock service
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Azure OpenAI credentials not configured. Using mock AI service for development.");
        return null!;
    }
    
    return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
});

// Register AI parsing service - use Azure OpenAI if available, otherwise mock
builder.Services.AddScoped<IAITaskParsingService>(serviceProvider =>
{
    var azureOpenAIClient = serviceProvider.GetService<AzureOpenAIClient>();
    if (azureOpenAIClient != null)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<AzureOpenAITaskParsingService>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new AzureOpenAITaskParsingService(azureOpenAIClient, logger, configuration);
    }
    else
    {
        // Fallback to mock service for development
        var logger = serviceProvider.GetRequiredService<ILogger<MockAITaskParsingService>>();
        return new MockAITaskParsingService(logger);
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("database", () => 
    {
        // Simple check for InMemory database
        return HealthCheckResult.Healthy("Database is accessible");
    });

var app = builder.Build();

// Initialize database with sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await DbInitializer.InitializeAsync(context, userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add Serilog request logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

// Map health checks endpoint
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
