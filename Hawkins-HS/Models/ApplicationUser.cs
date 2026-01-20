using Microsoft.AspNetCore.Identity;

namespace Hawkins_HS.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    
    // Navigation properties
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<Announcement> CreatedAnnouncements { get; set; } = new List<Announcement>();
}
