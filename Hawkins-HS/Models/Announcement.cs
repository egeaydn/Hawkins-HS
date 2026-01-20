namespace Hawkins_HS.Models;

public class Announcement
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    
    public string CreatorId { get; set; } = string.Empty;
    public ApplicationUser Creator { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Audience { get; set; } = "All"; // All, Students, Teachers, Class:11-A
    public bool IsImportant { get; set; } = false;
}
