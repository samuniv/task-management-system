using Azure.AI.OpenAI;
using BlazorWasm.Shared.DTOs;
using BlazorWasm.Shared.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenAI.Chat;

namespace BlazorWasm.Server.Services;

public class AzureOpenAITaskParsingService : IAITaskParsingService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AzureOpenAITaskParsingService> _logger;
    private readonly string _deploymentName;

    public AzureOpenAITaskParsingService(AzureOpenAIClient azureOpenAIClient, ILogger<AzureOpenAITaskParsingService> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-35-turbo";
        _chatClient = azureOpenAIClient?.GetChatClient(_deploymentName) ?? throw new ArgumentNullException(nameof(azureOpenAIClient));
    }

    public async Task<ParsedTaskResult> ParseNaturalLanguageAsync(string naturalLanguageInput)
    {
        if (string.IsNullOrWhiteSpace(naturalLanguageInput))
        {
            throw new ArgumentException("Input cannot be empty", nameof(naturalLanguageInput));
        }

        try
        {
            _logger.LogInformation("Parsing natural language input: {Input}", naturalLanguageInput);

            var systemPrompt = @"You are a task management assistant. Parse the user's input and extract structured task information.

Return a JSON object with these fields:
- title: A concise task title (required)
- description: A detailed description (optional, can be empty string)
- assignee: The person assigned (optional, extract from phrases like 'with John', 'assign to Mary', can be empty string)
- priority: One of 'Low', 'Medium', 'High' (default to 'Medium' if not specified)
- dueDate: ISO 8601 date string or empty string (extract from phrases like 'by Friday', 'due next week', 'tomorrow')

Examples:
Input: 'Review Q3 report with Alice by next Friday'
Output: {""title"": ""Review Q3 report"", ""description"": ""Review the Q3 financial report"", ""assignee"": ""Alice"", ""priority"": ""Medium"", ""dueDate"": """"}

Input: 'High priority: Fix the login bug ASAP'
Output: {""title"": ""Fix the login bug"", ""description"": ""Urgent fix needed for login functionality"", ""assignee"": """", ""priority"": ""High"", ""dueDate"": """"}

Only return valid JSON, no other text.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(naturalLanguageInput)
            };

            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.3f, // Lower temperature for more consistent structured output
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);
            var content = response.Value.Content[0].Text;

            _logger.LogDebug("OpenAI response: {Response}", content);

            // Parse the JSON response
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            var result = new ParsedTaskResult
            {
                Title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty,
                Description = root.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                Assignee = root.TryGetProperty("assignee", out var assigneeProp) ? assigneeProp.GetString() ?? string.Empty : string.Empty,
                Priority = ParsePriority(root.TryGetProperty("priority", out var priorityProp) ? priorityProp.GetString() : "Medium"),
                DueDate = ParseDueDate(root.TryGetProperty("dueDate", out var dueDateProp) ? dueDateProp.GetString() : string.Empty)
            };

            // Fallback to title if empty
            if (string.IsNullOrWhiteSpace(result.Title))
            {
                result.Title = TruncateToTitle(naturalLanguageInput);
            }

            _logger.LogInformation("Successfully parsed task: {Title}", result.Title);
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse OpenAI JSON response, falling back to rule-based parsing");
            return FallbackToRuleBasedParsing(naturalLanguageInput);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API, falling back to rule-based parsing");
            return FallbackToRuleBasedParsing(naturalLanguageInput);
        }
    }

    private static Priority ParsePriority(string? priorityText)
    {
        if (string.IsNullOrWhiteSpace(priorityText))
            return Priority.Medium;

        return priorityText.ToLowerInvariant() switch
        {
            "low" => Priority.Low,
            "high" => Priority.High,
            "urgent" => Priority.High,
            _ => Priority.Medium
        };
    }

    private static DateTime? ParseDueDate(string? dueDateText)
    {
        if (string.IsNullOrWhiteSpace(dueDateText))
            return null;

        if (DateTime.TryParse(dueDateText, out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }

    private static string TruncateToTitle(string input)
    {
        // Take first 50 characters as title if OpenAI fails
        if (input.Length <= 50)
            return input;

        var truncated = input.Substring(0, 47) + "...";
        return truncated;
    }

    // Fallback method using the same logic as MockAITaskParsingService
    private ParsedTaskResult FallbackToRuleBasedParsing(string naturalLanguageInput)
    {
        _logger.LogInformation("Using fallback rule-based parsing for: {Input}", naturalLanguageInput);

        var result = new ParsedTaskResult();

        // Extract assignee patterns
        var assigneePatterns = new[]
        {
            @"with\s+(\w+)",
            @"assign(?:ed)?\s+to\s+(\w+)",
            @"for\s+(\w+)",
            @"by\s+(\w+)\s+(?:due|by)"
        };

        foreach (var pattern in assigneePatterns)
        {
            var match = Regex.Match(naturalLanguageInput, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                result.Assignee = match.Groups[1].Value;
                break;
            }
        }

        // Extract priority
        if (Regex.IsMatch(naturalLanguageInput, @"\b(urgent|high|critical|asap)\b", RegexOptions.IgnoreCase))
        {
            result.Priority = Priority.High;
        }
        else if (Regex.IsMatch(naturalLanguageInput, @"\blow\b", RegexOptions.IgnoreCase))
        {
            result.Priority = Priority.Low;
        }
        else
        {
            result.Priority = Priority.Medium;
        }

        // Extract due date patterns
        var dueDatePatterns = new[]
        {
            (@"by\s+(monday|tuesday|wednesday|thursday|friday|saturday|sunday)", 7),
            (@"by\s+next\s+(week|friday|monday)", 7),
            (@"due\s+(tomorrow|next\s+week)", 1),
            (@"by\s+(today|tonight)", 0)
        };

        foreach (var (pattern, daysToAdd) in dueDatePatterns)
        {
            if (Regex.IsMatch(naturalLanguageInput, pattern, RegexOptions.IgnoreCase))
            {
                result.DueDate = DateTime.Now.AddDays(daysToAdd);
                break;
            }
        }

        // Generate title and description
        var cleanInput = naturalLanguageInput;
        
        // Remove assignee mentions
        cleanInput = Regex.Replace(cleanInput, @"\bwith\s+\w+\b", "", RegexOptions.IgnoreCase).Trim();
        cleanInput = Regex.Replace(cleanInput, @"\bassign(?:ed)?\s+to\s+\w+\b", "", RegexOptions.IgnoreCase).Trim();
        
        // Remove due date mentions
        cleanInput = Regex.Replace(cleanInput, @"\bby\s+\w+\b", "", RegexOptions.IgnoreCase).Trim();
        cleanInput = Regex.Replace(cleanInput, @"\bdue\s+\w+\b", "", RegexOptions.IgnoreCase).Trim();
        
        // Remove priority mentions
        cleanInput = Regex.Replace(cleanInput, @"\b(urgent|high|low|priority|critical|asap):?\s*", "", RegexOptions.IgnoreCase).Trim();

        // Clean up extra spaces
        cleanInput = Regex.Replace(cleanInput, @"\s+", " ").Trim();

        result.Title = string.IsNullOrWhiteSpace(cleanInput) ? "New Task" : cleanInput;
        result.Description = $"Parsed from: {naturalLanguageInput}";

        return result;
    }
}
