namespace api.Models;

public partial class Task
{
    public int Id { get; set; }

    public required int AssigneeId { get; set; }

    public required string TaskName { get; set; }

    public string? TaskDescription { get; set; }

    public DateTime? DueDate { get; set; }

    public required int PriorityId { get; set; }

    public required int ProjectId { get; set; }

    public required int StatusId { get; set; }

    public virtual User Assignee { get; set; }

    public virtual Priority Priority { get; set; }

    public virtual Project Project { get; set; }

    public virtual Status Status { get; set; }

    public virtual List<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();

    public virtual ICollection<StatusTrack> StatusTracks { get; set; } = new List<StatusTrack>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}



