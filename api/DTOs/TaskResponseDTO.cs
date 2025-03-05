public class TaskResponseDTO
{
    public int TaskId { get; set; }
    public int AssigneeId { get; set; }
    public string TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public int PriorityId { get; set; }
    public int ProjectId { get; set; }
    public List<int> ProjectLabelIds { get; set; } = new List<int>();
}
