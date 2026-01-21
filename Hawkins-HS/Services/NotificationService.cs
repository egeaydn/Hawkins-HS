using Hawkins_HS.Data;
using Hawkins_HS.Models;
using Microsoft.EntityFrameworkCore;

namespace Hawkins_HS.Services;

public interface INotificationService
{
    Task NotifyExamCreatedAsync(Exam exam);
    Task NotifyGradeEnteredAsync(Grade grade);
    Task NotifyAnnouncementCreatedAsync(Announcement announcement);
    Task<List<string>> GetUserNotificationsAsync(string userName);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task NotifyExamCreatedAsync(Exam exam)
    {
        // Derse kayıtlı öğrencilere bildirim
        var course = await _context.Courses
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == exam.CourseId);

        if (course != null)
        {
            var message = $"📝 Yeni Sınav: {exam.Title} - {course.Title} ({exam.ExamDate:dd MMM yyyy HH:mm})";
            
            _logger.LogInformation("Exam notification: {Message}", message);
            
            // Gerçek uygulamada email, SMS veya push notification gönderimi
            // Şimdilik log'a yazıyoruz
            foreach (var enrollment in course.Enrollments)
            {
                _logger.LogInformation("Notification to {Email}: {Message}", 
                    enrollment.Student.ApplicationUser.Email, message);
            }
        }
    }

    public async Task NotifyGradeEnteredAsync(Grade grade)
    {
        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .FirstOrDefaultAsync(s => s.Id == grade.StudentId);

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == grade.ExamId);

        if (student != null && exam != null)
        {
            var message = $"🎯 Yeni Not: {exam.Course.Title} - {exam.Title}: {grade.Score} ({grade.Letter})";
            
            _logger.LogInformation("Grade notification to {Email}: {Message}", 
                student.ApplicationUser.Email, message);
        }
    }

    public async Task NotifyAnnouncementCreatedAsync(Announcement announcement)
    {
        var creator = await _context.Users.FindAsync(announcement.CreatorId);
        
        var message = $"📢 Yeni Duyuru: {announcement.Title}";
        
        _logger.LogInformation("Announcement notification: {Message} - Audience: {Audience}", 
            message, announcement.Audience);

        // Hedef kitleye göre bildirim
        // Gerçek uygulamada role-based email/notification gönderimi
    }

    public async Task<List<string>> GetUserNotificationsAsync(string userName)
    {
        var notifications = new List<string>();
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user == null) return notifications;

        if (await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id))
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            if (roles.Contains("Student"))
            {
                var student = await _context.Students
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Course)
                            .ThenInclude(c => c.Exams)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == userName);

                if (student != null)
                {
                    // Yaklaşan sınavlar
                    var upcomingExams = student.Enrollments
                        .SelectMany(e => e.Course.Exams)
                        .Where(ex => ex.ExamDate > DateTime.Now && ex.ExamDate < DateTime.Now.AddDays(7))
                        .OrderBy(ex => ex.ExamDate)
                        .ToList();

                    foreach (var exam in upcomingExams)
                    {
                        var daysUntil = (exam.ExamDate - DateTime.Now).Days;
                        notifications.Add($"📝 {exam.Title} - {daysUntil} gün sonra");
                    }

                    // Devamsızlık uyarıları
                    foreach (var enrollment in student.Enrollments)
                    {
                        var attendances = await _context.Attendances
                            .Where(a => a.StudentId == student.Id && a.CourseId == enrollment.CourseId)
                            .ToListAsync();

                        if (attendances.Any())
                        {
                            var totalDays = attendances.Count;
                            var absences = attendances.Count(a => a.Status == "Absent");
                            var absenceRate = (absences * 100.0) / totalDays;

                            if (absenceRate > 20)
                            {
                                notifications.Add($"⚠️ {enrollment.Course.Title}: Devamsızlık kritik seviyede (%{absenceRate:0.0})");
                            }
                            else if (absenceRate > 10)
                            {
                                notifications.Add($"⚠️ {enrollment.Course.Title}: Devamsızlığınız yüksek (%{absenceRate:0.0})");
                            }
                        }
                    }
                }
            }

            if (roles.Contains("Teacher"))
            {
                var teacher = await _context.Teachers
                    .Include(t => t.Courses)
                        .ThenInclude(c => c.Exams)
                    .FirstOrDefaultAsync(t => t.ApplicationUser.UserName == userName);

                if (teacher != null)
                {
                    // Yaklaşan sınavlar
                    var upcomingExams = teacher.Courses
                        .SelectMany(c => c.Exams)
                        .Where(e => e.ExamDate > DateTime.Now && e.ExamDate < DateTime.Now.AddDays(3))
                        .OrderBy(e => e.ExamDate)
                        .ToList();

                    foreach (var exam in upcomingExams)
                    {
                        var gradedCount = await _context.Grades.CountAsync(g => g.ExamId == exam.Id);
                        var totalStudents = await _context.Enrollments.CountAsync(e => e.CourseId == exam.CourseId);
                        
                        if (gradedCount < totalStudents)
                        {
                            notifications.Add($"📝 {exam.Title}: {gradedCount}/{totalStudents} not girildi");
                        }
                    }
                }
            }
        }

        return notifications;
    }
}
