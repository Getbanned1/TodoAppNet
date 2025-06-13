using System.Collections.Generic;

public class Tag
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }

    public User User { get; set; }
    public ICollection<TaskTag> TaskTags { get; set; }
}
