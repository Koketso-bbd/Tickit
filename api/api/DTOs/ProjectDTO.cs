using api.Models;

namespace api.DTOs
{
    public class ProjectDTO
    {
        public int ID { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? ProjectDescription { get; set; }
        public int OwnerID { get; set; }
        public UserDTO Owner { get; set; } = null!;
        public List<UserDTO> AssignedUsers { get; set; } = new();
    }
}
