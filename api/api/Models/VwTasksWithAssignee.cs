namespace api.Models;

public partial class VwTasksWithAssignee
{
    public int TaskId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public string AssigneeGitHubId { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public int PriorityId { get; set; }
}
