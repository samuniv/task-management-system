using Microsoft.AspNetCore.Identity;

namespace BlazorWasm.Shared.Models;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
