using System;
using System.Collections.Generic;

namespace api.Models;

public partial class VwProjectWithOwner
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? ProjectDescription { get; set; }

    public string OwnerId { get; set; } = null!;
}
