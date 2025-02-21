using System;
using System.Collections.Generic;

namespace api.Models;

public partial class User
{
    public int Id { get; set; }

    public string GitHubId { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
