namespace api.Models;

public partial class VwUnreadNotification
{
    public int NotificationId { get; set; }

    public string UserGitHubId { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string? TaskName { get; set; }

    public string NotificationType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
