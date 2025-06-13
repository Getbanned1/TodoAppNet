public class Reminder
{
    public int Id { get; set; }
    public string TaskId { get; set; }
    public long ReminderTime { get; set; }
    public bool IsNotified { get; set; }

    public TaskItem Task { get; set; }
}
