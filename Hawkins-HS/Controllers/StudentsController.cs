using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Students
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Index()
    {
        var students = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Enrollments)
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.ApplicationUser.FullName)
            .ToListAsync();

        return View(students);
    }

    // GET: Students/Dashboard
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.Identity?.Name;
        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.ApplicationUser)
            .Include(s => s.Grades)
                .ThenInclude(g => g.Exam)
                    .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == userId);

        if (student == null)
        {
            return NotFound("Öğrenci profili bulunamadı.");
        }

        // Yaklaşan sınavlar
        var enrolledCourseIds = student.Enrollments.Select(e => e.CourseId).ToList();
        var upcomingExams = await _context.Exams
            .Where(e => enrolledCourseIds.Contains(e.CourseId) && e.ExamDate > DateTime.Now)
            .OrderBy(e => e.ExamDate)
            .Include(e => e.Course)
            .Take(5)
            .ToListAsync();

        ViewBag.UpcomingExams = upcomingExams;

        return View(student);
    }

    // GET: Students/Details/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Grades)
                .ThenInclude(g => g.Exam)
                    .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (student == null) return NotFound();

        return View(student);
    }

    // GET: Students/Grades
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyGrades()
    {
        var userId = User.Identity?.Name;
        var student = await _context.Students
            .Include(s => s.Grades)
                .ThenInclude(g => g.Exam)
                    .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == userId);

        if (student == null)
        {
            return NotFound("Öğrenci profili bulunamadı.");
        }

        return View(student.Grades.OrderByDescending(g => g.GradedAt).ToList());
    }
}
