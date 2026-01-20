namespace Hawkins_HS.Models;

public class Attendance
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused
    public string? Note { get; set; }
}
