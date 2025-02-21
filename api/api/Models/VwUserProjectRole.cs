using System;
using System.Collections.Generic;

namespace api.Models;

public partial class VwUserProjectRole
{
    public int UserProjectId { get; set; }

    public string RoleName { get; set; } = null!;

    public int RoleId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string GitHubId { get; set; } = null!;
}
