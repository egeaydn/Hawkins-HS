namespace Hawkins_HS.Models;

public class Course
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g. HAW101
    public string Description { get; set; } = string.Empty;
    public int Credits { get; set; }
    
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    
    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
