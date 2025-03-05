public class TaskUpdateDTO
{
    public string? TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public int? PriorityId { get; set; }
    public int? AssigneeId { get; set; }
    public List<int>? ProjectLabelIds { get; set; }
}
