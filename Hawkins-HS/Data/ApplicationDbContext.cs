using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Models;

namespace Hawkins_HS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<ClassSchedule> ClassSchedules { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Attendance> Attendances { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Enrollment (many-to-many) configuration
        builder.Entity<Enrollment>()
            .HasKey(e => new { e.StudentId, e.CourseId });

        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student - ApplicationUser (one-to-one)
        builder.Entity<Student>()
            .HasOne(s => s.ApplicationUser)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Student>()
            .HasIndex(s => s.StudentNumber)
            .IsUnique();

        // Teacher - ApplicationUser (one-to-one)
        builder.Entity<Teacher>()
            .HasOne(t => t.ApplicationUser)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Teacher>()
            .HasIndex(t => t.EmployeeNumber)
            .IsUnique();

        // Course - Teacher (many-to-one)
        builder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Course>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // ClassSchedule - Course (many-to-one)
        builder.Entity<ClassSchedule>()
            .HasOne(cs => cs.Course)
            .WithMany(c => c.ClassSchedules)
            .HasForeignKey(cs => cs.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Exam - Course (many-to-one)
        builder.Entity<Exam>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Exams)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Grade configuration
        builder.Entity<Grade>()
            .HasOne(g => g.Exam)
            .WithMany(e => e.Grades)
            .HasForeignKey(g => g.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Grade>()
            .Property(g => g.Score)
            .HasPrecision(5, 2);

        // Announcement - Creator (many-to-one)
        builder.Entity<Announcement>()
            .HasOne(a => a.Creator)
            .WithMany(u => u.CreatedAnnouncements)
            .HasForeignKey(a => a.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attendance configuration
        builder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attendance>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Attendances)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attendance>()
            .HasIndex(a => new { a.StudentId, a.CourseId, a.Date })
            .IsUnique();
    }
}
