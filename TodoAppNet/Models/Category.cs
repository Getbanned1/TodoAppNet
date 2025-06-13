using System.Collections.Generic;

public class Category
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }

    public User User { get; set; }
    public ICollection<TaskItem> Tasks { get; set; }
}
