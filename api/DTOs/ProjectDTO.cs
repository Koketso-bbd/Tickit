using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class ProjectDTO
    {
        public int ID { get; set; }

        public string ProjectName { get; set; } = null!;

        public string? ProjectDescription { get; set; }

        public UserDTO Owner { get; set; } = null!;
        
        public List<UserDTO> AssignedUsers { get; set; } = new();
    }

    public class ProjectLabelDTO
    {
        public int ID { get; set; }

        public int LabelID { get; set; }

        public int ProjectID { get; set; }

        public required LabelDTO LabelName { get; set; }
    }

    public class ProjectWithTasksDTO : ProjectDTO
    {
        [JsonPropertyOrder(10)]
        public List<TaskDTO> Tasks { get; set; } = new();
    }

    public class  UpdateProjectDTO
    {
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
    }
}
