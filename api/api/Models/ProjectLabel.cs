using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ProjectLabel
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int LabelId { get; set; }

    public virtual Label Label { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}
