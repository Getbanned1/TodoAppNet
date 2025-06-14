using System.Collections.Generic;

public class TaskItem
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? CategoryId { get; set; }
 


   public bool IsCompleted { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
    public long? Deadline { get; set; }
    public bool IsRecurring { get; set; }
    public string RecurrenceRule { get; set; }
    public long? LastSyncTimestamp { get; set; }
    public int SortOrder { get; set; }

    public User User { get; set; }
    public Category Category { get; set; }
    public ICollection<TaskTag> TaskTags { get; set; }
    public ICollection<Reminder> Reminders { get; set; }
}
