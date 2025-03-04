namespace api.DTOs;

public partial class NotificationsDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    public int NotificationTypeId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

}
