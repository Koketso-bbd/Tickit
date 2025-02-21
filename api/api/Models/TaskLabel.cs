using System;
using System.Collections.Generic;

namespace api.Models;

public partial class TaskLabel
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int ProjectLabelId { get; set; }

    public virtual ProjectLabel ProjectLabel { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
