﻿namespace api.DTOs
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
}
