namespace api.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    public int NotificationTypeId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual NotificationType NotificationType { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Task? Task { get; set; }

    public virtual User User { get; set; } = null!;
}
