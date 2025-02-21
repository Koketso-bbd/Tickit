using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Task
{
    public int Id { get; set; }

    public int AssigneeId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public DateTime DueDate { get; set; }

    public int PriorityId { get; set; }

    public int ProjectId { get; set; }

    public int StatusId { get; set; }

    public virtual User Assignee { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Priority Priority { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<StatusTrack> StatusTracks { get; set; } = new List<StatusTrack>();

    public virtual ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}
