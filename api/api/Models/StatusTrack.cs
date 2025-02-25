namespace api.Models;

public partial class StatusTrack
{
    public int Id { get; set; }

    public int StatusId { get; set; }

    public int TaskId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
