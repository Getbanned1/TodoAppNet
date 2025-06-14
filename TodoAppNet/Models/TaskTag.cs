public class TaskTag
{
    public string TaskId { get; set; }
    public int TagId { get; set; }

    public TaskItem Task { get; set; }
 
    public Tag Tag { get; set; }
}
