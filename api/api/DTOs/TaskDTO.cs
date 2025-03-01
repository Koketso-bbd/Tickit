namespace api.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }
        
        public required int AssigneeId { get; set; } 

        public required string TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public DateTime? DueDate { get; set; }

        public required int PriorityId { get; set; }

        public required int ProjectId { get; set; }

        public required int StatusId { get; set; }
    }

}
