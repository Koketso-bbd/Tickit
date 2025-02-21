using System;
using System.Collections.Generic;

namespace api.Models;

public partial class NotificationType
{
    public int Id { get; set; }

    public string NotificationName { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
