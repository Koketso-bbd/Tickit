using System;
using System.Collections.Generic;

namespace api.Models;

public partial class UserProject
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int MemberId { get; set; }

    public int RoleId { get; set; }

    public virtual User Member { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
