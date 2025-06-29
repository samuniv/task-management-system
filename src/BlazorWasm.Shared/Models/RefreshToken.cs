using System.ComponentModel.DataAnnotations;

namespace BlazorWasm.Shared.Models;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}
