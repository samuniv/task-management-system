using BlazorWasm.Shared.Enums;

namespace BlazorWasm.Shared.DTOs;

public class NaturalLanguageTaskRequest
{
    public string Input { get; set; } = string.Empty;
}

public class NaturalLanguageTaskResponse
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public string OriginalInput { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
