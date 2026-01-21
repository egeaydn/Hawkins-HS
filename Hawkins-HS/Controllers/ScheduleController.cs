using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class ScheduleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(ApplicationDbContext context, ILogger<ScheduleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Schedule/Manage/5 (CourseId)
    public async Task<IActionResult> Manage(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.ClassSchedules)
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        ViewBag.Course = course;
        return View();
    }

    // POST: Schedule/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, string day, TimeSpan startTime, TimeSpan endTime, string room)
    {
        if (startTime >= endTime)
        {
            TempData["Error"] = "Bitiş saati başlangıç saatinden sonra olmalıdır.";
            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        var schedule = new ClassSchedule
        {
            CourseId = courseId,
            Day = day,
            StartTime = startTime,
            EndTime = endTime,
            Room = room
        };

        _context.ClassSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Schedule created for Course {CourseId}: {Day} {StartTime}-{EndTime}", 
            courseId, day, startTime, endTime);

        TempData["Success"] = "Ders programı eklendi.";
        return RedirectToAction(nameof(Manage), new { id = courseId });
    }

    // POST: Schedule/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseId)
    {
        var schedule = await _context.ClassSchedules.FindAsync(id);
        if (schedule != null)
        {
            _context.ClassSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Schedule deleted: {ScheduleId}", id);
            TempData["Success"] = "Ders programı silindi.";
        }

        return RedirectToAction(nameof(Manage), new { id = courseId });
    }

    // GET: Schedule/Weekly
    [AllowAnonymous]
    public async Task<IActionResult> Weekly()
    {
        string? userName = null;
        List<ClassSchedule> schedules;

        if (User.Identity?.IsAuthenticated ?? false)
        {
            userName = User.Identity.Name;

            if (User.IsInRole("Student"))
            {
                // Öğrencinin kayıtlı olduğu derslerin programı
                var student = await _context.Students
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Course)
                            .ThenInclude(c => c.ClassSchedules)
                    .Include(s => s.Enrollments)
                        .ThenInclude(e => e.Course)
                            .ThenInclude(c => c.Teacher)
                                .ThenInclude(t => t.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == userName);

                schedules = student?.Enrollments
                    .SelectMany(e => e.Course.ClassSchedules)
                    .OrderBy(s => s.Day)
                    .ThenBy(s => s.StartTime)
                    .ToList() ?? new List<ClassSchedule>();
            }
            else if (User.IsInRole("Teacher"))
            {
                // Öğretmenin verdiği derslerin programı
                var teacher = await _context.Teachers
                    .Include(t => t.Courses)
                        .ThenInclude(c => c.ClassSchedules)
                    .FirstOrDefaultAsync(t => t.ApplicationUser.UserName == userName);

                schedules = teacher?.Courses
                    .SelectMany(c => c.ClassSchedules)
                    .OrderBy(s => s.Day)
                    .ThenBy(s => s.StartTime)
                    .ToList() ?? new List<ClassSchedule>();
            }
            else
            {
                // Admin - tüm programlar
                schedules = await _context.ClassSchedules
                    .Include(s => s.Course)
                        .ThenInclude(c => c.Teacher)
                            .ThenInclude(t => t.ApplicationUser)
                    .OrderBy(s => s.Day)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();
            }
        }
        else
        {
            schedules = new List<ClassSchedule>();
        }

        return View(schedules);
    }
}
