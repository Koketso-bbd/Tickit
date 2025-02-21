using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Priority
{
    public int Id { get; set; }

    public string PriorityLevel { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
