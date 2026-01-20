namespace Hawkins_HS.Models;

public class Exam
{
    public int Id { get; set; }
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int DurationMinutes { get; set; }
    public string ExamType { get; set; } = "Written"; // Written, Oral, Practical, Quiz
    
    // Navigation properties
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
