using System;
using System.Collections.Generic;

namespace api.DTOs;

public partial class NotificationTypeDTO
{
    public int ID { get; set; }

    public string NotificationName { get; set; } = null!;
}
