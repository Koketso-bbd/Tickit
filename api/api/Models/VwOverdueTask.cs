using System;
using System.Collections.Generic;

namespace api.Models;

public partial class VwOverdueTask
{
    public int TaskId { get; set; }

    public string TaskName { get; set; } = null!;

    public string AssigneeGitHubId { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public DateTime DueDate { get; set; }
}
