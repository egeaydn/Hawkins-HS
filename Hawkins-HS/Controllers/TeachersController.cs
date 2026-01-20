using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize]
public class TeachersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(ApplicationDbContext context, ILogger<TeachersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Teachers
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Include(t => t.Courses)
            .ToListAsync();

        return View(teachers);
    }

    // GET: Teachers/Dashboard
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.Identity?.Name;
        var teacher = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Include(t => t.Courses)
                .ThenInclude(c => c.Enrollments)
            .FirstOrDefaultAsync(t => t.ApplicationUser.UserName == userId);

        if (teacher == null)
        {
            return NotFound("Öğretmen profili bulunamadı.");
        }

        // Yaklaşan sınavlar
        var courseIds = teacher.Courses.Select(c => c.Id).ToList();
        var upcomingExams = await _context.Exams
            .Where(e => courseIds.Contains(e.CourseId) && e.ExamDate > DateTime.Now)
            .OrderBy(e => e.ExamDate)
            .Include(e => e.Course)
            .Take(5)
            .ToListAsync();

        ViewBag.UpcomingExams = upcomingExams;

        return View(teacher);
    }

    // GET: Teachers/Details/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var teacher = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Include(t => t.Courses)
                .ThenInclude(c => c.Enrollments)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (teacher == null) return NotFound();

        return View(teacher);
    }
}
