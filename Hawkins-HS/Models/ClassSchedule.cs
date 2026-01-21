namespace Hawkins_HS.Models;

public class ClassSchedule
{
    public int Id { get; set; }
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public string Day { get; set; } = string.Empty; // Pazartesi, Salı, vb.
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
}
