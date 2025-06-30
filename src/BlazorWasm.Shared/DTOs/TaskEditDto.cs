using BlazorWasm.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;

namespace BlazorWasm.Shared.DTOs;

public class TaskEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public Priority Priority { get; set; } = Priority.Medium;

    public int? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }

    public int CreatorId { get; set; }

    public string CreatorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CommentDisplayDto> Comments { get; set; } = new();
}

public class CommentCreateDto
{
    [Required(ErrorMessage = "Comment content is required")]
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    public string Content { get; set; } = string.Empty;

    public int TaskId { get; set; }
}

public class CommentDisplayDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
