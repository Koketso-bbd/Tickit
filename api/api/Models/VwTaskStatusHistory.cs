using System;
using System.Collections.Generic;

namespace api.Models;

public partial class VwTaskStatusHistory
{
    public int TaskId { get; set; }

    public string TaskName { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}
