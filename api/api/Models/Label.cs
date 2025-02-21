using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Label
{
    public int Id { get; set; }

    public string LabelName { get; set; } = null!;

    public virtual ICollection<ProjectLabel> ProjectLabels { get; set; } = new List<ProjectLabel>();
}
