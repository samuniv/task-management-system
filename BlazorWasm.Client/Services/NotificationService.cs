using BlazorWasm.Shared.Models;

namespace BlazorWasm.Client.Services;

public interface INotificationService
{
    event Action<List<ToastNotification>>? NotificationsChanged;
    IReadOnlyList<ToastNotification> Notifications { get; }
    void ShowSuccess(string message, int dismissAfterMs = 5000);
    void ShowError(string message, int dismissAfterMs = 8000);
    void ShowWarning(string message, int dismissAfterMs = 6000);
    void ShowInfo(string message, int dismissAfterMs = 5000);
    void RemoveNotification(Guid id);
    void ClearAll();
}

public class NotificationService : INotificationService
{
    private readonly List<ToastNotification> _notifications = new();
    private readonly Dictionary<string, DateTime> _recentMessages = new();
    private readonly TimeSpan _duplicateThreshold = TimeSpan.FromSeconds(3);

    public event Action<List<ToastNotification>>? NotificationsChanged;

    public IReadOnlyList<ToastNotification> Notifications => _notifications.AsReadOnly();

    public void ShowSuccess(string message, int dismissAfterMs = 5000)
    {
        AddNotification(new ToastNotification
        {
            Message = message,
            Type = NotificationType.Success,
            DismissAfterMs = dismissAfterMs
        });
    }

    public void ShowError(string message, int dismissAfterMs = 8000)
    {
        AddNotification(new ToastNotification
        {
            Message = message,
            Type = NotificationType.Error,
            DismissAfterMs = dismissAfterMs
        });
    }

    public void ShowWarning(string message, int dismissAfterMs = 6000)
    {
        AddNotification(new ToastNotification
        {
            Message = message,
            Type = NotificationType.Warning,
            DismissAfterMs = dismissAfterMs
        });
    }

    public void ShowInfo(string message, int dismissAfterMs = 5000)
    {
        AddNotification(new ToastNotification
        {
            Message = message,
            Type = NotificationType.Info,
            DismissAfterMs = dismissAfterMs
        });
    }

    public void RemoveNotification(Guid id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification != null)
        {
            _notifications.Remove(notification);
            NotificationsChanged?.Invoke(_notifications);
        }
    }

    public void ClearAll()
    {
        _notifications.Clear();
        NotificationsChanged?.Invoke(_notifications);
    }

    private void AddNotification(ToastNotification notification)
    {
        // Prevent duplicate messages within the threshold
        if (IsDuplicate(notification.Message))
        {
            return;
        }

        _notifications.Add(notification);
        _recentMessages[notification.Message] = DateTime.UtcNow;

        // Clean up old duplicate tracking entries
        CleanupRecentMessages();

        NotificationsChanged?.Invoke(_notifications);

        // Auto-dismiss if enabled
        if (notification.AutoDismiss)
        {
            _ = Task.Delay(notification.DismissAfterMs).ContinueWith(_ =>
            {
                RemoveNotification(notification.Id);
            });
        }
    }

    private bool IsDuplicate(string message)
    {
        if (_recentMessages.TryGetValue(message, out var lastShown))
        {
            return DateTime.UtcNow - lastShown < _duplicateThreshold;
        }
        return false;
    }

    private void CleanupRecentMessages()
    {
        var cutoff = DateTime.UtcNow - _duplicateThreshold;
        var keysToRemove = _recentMessages
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _recentMessages.Remove(key);
        }
    }
}
