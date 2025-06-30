using BlazorWasm.Shared.DTOs;
using BlazorWasm.Shared.Enums;

namespace BlazorWasm.Server.Services;

public class MockAITaskParsingService : IAITaskParsingService
{
    private readonly ILogger<MockAITaskParsingService> _logger;

    public MockAITaskParsingService(ILogger<MockAITaskParsingService> logger)
    {
        _logger = logger;
    }

    public async Task<ParsedTaskResult> ParseNaturalLanguageAsync(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return new ParsedTaskResult 
            { 
                IsSuccess = false, 
                ErrorMessage = "Input cannot be empty" 
            };
        }

        // Simple mock parsing logic for demonstration
        await Task.Delay(500); // Simulate AI processing time

        _logger.LogInformation("Mock parsing for input: {UserInput}", userInput);

        var result = new ParsedTaskResult
        {
            IsSuccess = true,
            Title = ExtractTitle(userInput),
            Description = ExtractDescription(userInput),
            Assignee = ExtractAssignee(userInput),
            DueDate = ExtractDueDate(userInput),
            Priority = ExtractPriority(userInput),
            ConfidenceScore = 0.75 // Mock confidence
        };

        return result;
    }

    private static string ExtractTitle(string input)
    {
        // Simple heuristics for title extraction
        var title = input.Length > 50 ? input[..50] + "..." : input;
        
        // Remove common prefixes
        title = title.Replace("I need to ", "").Replace("Please ", "").Replace("Can you ", "");
        
        // Capitalize first letter
        if (title.Length > 0)
        {
            title = char.ToUpper(title[0]) + title[1..];
        }

        return title;
    }

    private static string? ExtractDescription(string input)
    {
        // If input is long enough, use it as description
        return input.Length > 20 ? input : null;
    }

    private static string? ExtractAssignee(string input)
    {
        var lowerInput = input.ToLower();
        
        // Look for common patterns
        if (lowerInput.Contains("with "))
        {
            var withIndex = lowerInput.IndexOf("with ");
            var afterWith = input[(withIndex + 5)..];
            var words = afterWith.Split(' ');
            if (words.Length > 0)
            {
                return words[0];
            }
        }
        
        if (lowerInput.Contains("assign to "))
        {
            var assignIndex = lowerInput.IndexOf("assign to ");
            var afterAssign = input[(assignIndex + 10)..];
            var words = afterAssign.Split(' ');
            if (words.Length > 0)
            {
                return words[0];
            }
        }

        return null;
    }

    private static DateTime? ExtractDueDate(string input)
    {
        var lowerInput = input.ToLower();
        var now = DateTime.Now;

        if (lowerInput.Contains("tomorrow"))
        {
            return now.AddDays(1);
        }
        
        if (lowerInput.Contains("next week"))
        {
            return now.AddDays(7);
        }
        
        if (lowerInput.Contains("next friday"))
        {
            var daysUntilFriday = ((int)DayOfWeek.Friday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilFriday == 0) daysUntilFriday = 7; // Next Friday, not today
            return now.AddDays(daysUntilFriday);
        }

        if (lowerInput.Contains("by "))
        {
            // Try to parse date after "by"
            var byIndex = lowerInput.IndexOf("by ");
            var afterBy = input[(byIndex + 3)..];
            if (DateTime.TryParse(afterBy, out var parsedDate))
            {
                return parsedDate;
            }
        }

        return null;
    }

    private static Priority? ExtractPriority(string input)
    {
        var lowerInput = input.ToLower();

        if (lowerInput.Contains("urgent") || lowerInput.Contains("asap") || lowerInput.Contains("critical"))
        {
            return Priority.Critical;
        }
        
        if (lowerInput.Contains("high priority") || lowerInput.Contains("important"))
        {
            return Priority.High;
        }
        
        if (lowerInput.Contains("low priority") || lowerInput.Contains("when you have time"))
        {
            return Priority.Low;
        }

        return Priority.Medium; // Default to medium
    }
}
