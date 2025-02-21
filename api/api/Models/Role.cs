using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Role
{
    public int Id { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
