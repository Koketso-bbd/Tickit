using System.ComponentModel.DataAnnotations;

namespace api.Models;

public partial class Task
{
    public int Id { get; set; }

    [Required]
    public int AssigneeId { get; set; }

    public required string TaskName { get; set; }

    public string? TaskDescription { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public int PriorityId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int StatusId { get; set; }

    public virtual User? Assignee { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Priority? Priority { get; set; }
    public virtual Project? Project { get; set; }
    public virtual Status? Status { get; set; }


    public virtual ICollection<StatusTrack> StatusTracks { get; set; } = new List<StatusTrack>();

    public virtual ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}



