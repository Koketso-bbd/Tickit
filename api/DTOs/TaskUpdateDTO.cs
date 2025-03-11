using System.Text.Json;

public class TaskUpdateDTO
{
    public int? AssigneeId { get; set; }
    public string? TaskName { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public List<int>? ProjectLabelIds { get; set; }

}
