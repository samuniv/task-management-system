using System.ComponentModel.DataAnnotations;
using BlazorWasm.Shared.Enums;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;

namespace BlazorWasm.Shared.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public Priority Priority { get; set; } = Priority.Medium;

    public int? AssigneeId { get; set; }
    public virtual ApplicationUser? Assignee { get; set; }

    public int CreatorId { get; set; }
    public virtual ApplicationUser Creator { get; set; } = null!;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
