using BlazorWasm.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;

namespace BlazorWasm.Server.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public int CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaskDetailDto : TaskDto
{
    public List<CommentDto> Comments { get; set; } = new();
}

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public Priority Priority { get; set; } = Priority.Medium;
    public int? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
