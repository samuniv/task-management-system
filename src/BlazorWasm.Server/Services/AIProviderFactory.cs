using Azure.AI.OpenAI;

namespace BlazorWasm.Server.Services;

public interface IAIProviderFactory
{
    IAITaskParsingService CreateTaskParsingService();
}

public class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIProviderFactory> _logger;

    public AIProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<AIProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public IAITaskParsingService CreateTaskParsingService()
    {
        var aiProvider = _configuration["AI:Provider"] ?? "Mock";
        
        _logger.LogInformation("Creating AI task parsing service with provider: {Provider}", aiProvider);

        return aiProvider.ToLowerInvariant() switch
        {
            "azureopenai" => CreateAzureOpenAIService(),
            "googlegemini" => CreateGoogleGeminiService(),
            "mock" => CreateMockService(),
            _ => CreateMockService() // Default fallback
        };
    }

    private IAITaskParsingService CreateAzureOpenAIService()
    {
        try
        {
            var apiKey = _configuration["AI:AzureOpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            var endpoint = _configuration["AI:AzureOpenAI:Endpoint"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
            {
                _logger.LogWarning("Azure OpenAI credentials not configured. Falling back to mock service.");
                return CreateMockService();
            }

            var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey));
            var logger = _serviceProvider.GetRequiredService<ILogger<AzureOpenAITaskParsingService>>();
            
            return new AzureOpenAITaskParsingService(azureOpenAIClient, logger, _configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Azure OpenAI service. Falling back to mock service.");
            return CreateMockService();
        }
    }

    private IAITaskParsingService CreateGoogleGeminiService()
    {
        try
        {
            var apiKey = _configuration["AI:GoogleGemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Google Gemini API key not configured. Falling back to mock service.");
                return CreateMockService();
            }

            // Use reflection to create GoogleAI client to avoid compile-time dependency
            var googleAIType = Type.GetType("Google.GenerativeAI.GoogleAI, Google_GenerativeAI");
            if (googleAIType == null)
            {
                _logger.LogWarning("Google.GenerativeAI library not found. Falling back to mock service.");
                return CreateMockService();
            }

            var googleAI = Activator.CreateInstance(googleAIType, apiKey);
            var logger = _serviceProvider.GetRequiredService<ILogger<GoogleGeminiTaskParsingService>>();
            
            return new GoogleGeminiTaskParsingService((dynamic)googleAI!, logger, _configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Google Gemini service. Falling back to mock service.");
            return CreateMockService();
        }
    }

    private IAITaskParsingService CreateMockService()
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<MockAITaskParsingService>>();
        return new MockAITaskParsingService(logger);
    }
}
