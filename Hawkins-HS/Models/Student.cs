namespace Hawkins_HS.Models;

public class Student
{
    public int Id { get; set; }
    
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    
    public string StudentNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty; // e.g. 11-A
    public int Year { get; set; } // Sınıf seviyesi: 9, 10, 11, 12
    public string? ProfilePhotoBase64 { get; set; } // Profil fotoğrafı BASE64 formatında
    
    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
