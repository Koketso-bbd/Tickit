namespace api.Models;

public partial class Status
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<StatusTrack> StatusTracks { get; set; } = new List<StatusTrack>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
