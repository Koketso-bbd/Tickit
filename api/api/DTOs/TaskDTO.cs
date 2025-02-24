using System;
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }

        [Required]
        public int AssigneeId { get; set; }

        [Required]
        public string TaskName { get; set; }

        public string? TaskDescription { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int PriorityId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int StatusId { get; set; }
    }
}
