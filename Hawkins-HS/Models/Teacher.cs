namespace Hawkins_HS.Models;

public class Teacher
{
    public int Id { get; set; }
    
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
