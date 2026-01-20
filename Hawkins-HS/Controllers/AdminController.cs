using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Dashboard()
    {
        var stats = new
        {
            TotalStudents = await _context.Students.CountAsync(),
            TotalTeachers = await _context.Teachers.CountAsync(),
            TotalCourses = await _context.Courses.CountAsync(),
            TotalExams = await _context.Exams.CountAsync(),
            UpcomingExams = await _context.Exams
                .Where(e => e.ExamDate > DateTime.Now)
                .OrderBy(e => e.ExamDate)
                .Take(5)
                .Include(e => e.Course)
                .ToListAsync(),
            RecentAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Include(a => a.Creator)
                .ToListAsync()
        };

        return View(stats);
    }

    public async Task<IActionResult> Users()
    {
        var students = await _context.Students
            .Include(s => s.ApplicationUser)
            .ToListAsync();

        var teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .ToListAsync();

        ViewBag.Students = students;
        ViewBag.Teachers = teachers;

        return View();
    }
}
