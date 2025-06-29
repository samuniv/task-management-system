using System.ComponentModel.DataAnnotations;

namespace BlazorWasm.Shared.Models;

public class Comment
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public virtual TaskItem Task { get; set; } = null!;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
