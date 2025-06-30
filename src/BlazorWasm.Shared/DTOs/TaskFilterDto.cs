using BlazorWasm.Shared.Enums;

namespace BlazorWasm.Shared.DTOs;

/// <summary>
/// DTO for filtering tasks in queries and exports
/// </summary>
public class TaskFilterDto
{
    /// <summary>
    /// Search term to filter tasks by title or description
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by task status
    /// </summary>
    public BlazorWasm.Shared.Enums.TaskStatus? Status { get; set; }

    /// <summary>
    /// Filter by task priority
    /// </summary>
    public Priority? Priority { get; set; }

    /// <summary>
    /// Filter by assignee ID
    /// </summary>
    public int? AssigneeId { get; set; }

    /// <summary>
    /// Filter by creator ID
    /// </summary>
    public int? CreatorId { get; set; }

    /// <summary>
    /// Filter by due date range - start date
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Filter by due date range - end date
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Filter by creation date range - start date
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Filter by creation date range - end date
    /// </summary>
    public DateTime? CreatedTo { get; set; }
}
