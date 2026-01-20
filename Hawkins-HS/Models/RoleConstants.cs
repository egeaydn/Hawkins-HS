namespace Hawkins_HS.Models;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    
    public static string[] AllRoles => new[] { Admin, Teacher, Student };
}
