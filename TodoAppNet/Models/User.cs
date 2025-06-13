using System.Collections.Generic;

public class User
{
    public string Id { get; set; } // Firebase UID или другой уникальный идентификатор
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string PhotoUrl { get; set; }
    public long? LastSyncTimestamp { get; set; }

    public ICollection<Category> Categories { get; set; }
    public ICollection<Tag> Tags { get; set; }
    public ICollection<TaskItem> Tasks { get; set; }
}
