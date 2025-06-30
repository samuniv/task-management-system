using BlazorWasm.Shared.DTOs;
using BlazorWasm.Shared.Enums;

namespace BlazorWasm.Server.Services;

public interface IAITaskParsingService
{
    Task<ParsedTaskResult> ParseNaturalLanguageAsync(string userInput);
}

public class ParsedTaskResult
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Assignee { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority? Priority { get; set; }
    public string? ErrorMessage { get; set; }
    public double? ConfidenceScore { get; set; }
}
