namespace Hawkins_HS.Models;

public class Grade
{
    public int Id { get; set; }
    
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public decimal Score { get; set; }
    public string Letter { get; set; } = string.Empty; // A, B, C, D, F
    public string? Comment { get; set; }
    public DateTime GradedAt { get; set; } = DateTime.UtcNow;
}
