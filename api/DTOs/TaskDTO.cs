using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.DTOs;

public class TaskDTO
{
    [Required] public string TaskName { get; set; }
    [Required] public int ProjectId { get; set; }
    public int? AssigneeId { get; set; } = null;
    public int? PriorityId { get; set; } = null;
    public string? TaskDescription { get; set; } = "No description provided";
    public DateTime? DueDate { get; set; } = null;
    public List<int>? ProjectLabelIds { get; set; }
}
