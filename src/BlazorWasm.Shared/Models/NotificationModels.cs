namespace BlazorWasm.Shared.Models;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool AutoDismiss { get; set; } = true;
    public int DismissAfterMs { get; set; } = 5000;
}
