using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.DTOs;


public class TaskDTO
{
    [Required] public int AssigneeId { get; set; }
    [Required] public string TaskName { get; set; }
    [Required] public int ProjectId { get; set; }
    public int? PriorityId { get; set; }
    public string? TaskDescription { get; set; } = "No description provided";
    public DateTime? DueDate { get; set; }
    public List<int>? ProjectLabelIds { get; set; }
}
