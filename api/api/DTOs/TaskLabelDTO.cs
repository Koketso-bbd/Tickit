namespace api.DTOs;

public partial class TaskLabelDTO
{
    public int ID { get; set; }

    public int TaskId { get; set; }

    public int ProjectLabelId { get; set; }
}
