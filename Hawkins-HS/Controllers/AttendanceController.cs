using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Attendance/TakeAttendance/5 (CourseId)
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> TakeAttendance(int? id, DateTime? date)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        var attendanceDate = date ?? DateTime.Today;

        // Bu tarih için mevcut yoklama kayıtları
        var existingAttendance = await _context.Attendances
            .Where(a => a.CourseId == id && a.Date.Date == attendanceDate.Date)
            .Include(a => a.Student)
            .ToListAsync();

        ViewBag.Course = course;
        ViewBag.AttendanceDate = attendanceDate;
        ViewBag.ExistingAttendance = existingAttendance;

        return View();
    }

    // POST: Attendance/SaveAttendance
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> SaveAttendance(int courseId, int studentId, DateTime date, string status)
    {
        // Mevcut kaydı kontrol et
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.CourseId == courseId && a.StudentId == studentId && a.Date.Date == date.Date);

        if (existingAttendance != null)
        {
            // Güncelle
            existingAttendance.Status = status;
            _context.Update(existingAttendance);
        }
        else
        {
            // Yeni kayıt
            var attendance = new Attendance
            {
                CourseId = courseId,
                StudentId = studentId,
                Date = date.Date,
                Status = status
            };
            _context.Add(attendance);
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Attendance saved: CourseId {CourseId}, StudentId {StudentId}, Status {Status}", 
            courseId, studentId, status);

        return RedirectToAction(nameof(TakeAttendance), new { id = courseId, date = date.Date });
    }

    // GET: Attendance/CourseReport/5 (CourseId)
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CourseReport(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t!.ApplicationUser)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
            .Include(c => c.Attendances)
                .ThenInclude(a => a.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        ViewBag.Course = course;

        return View();
    }

    // GET: Attendance/StudentReport/5 (StudentId)
    [Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<IActionResult> StudentReport(int? id)
    {
        if (id == null)
        {
            // Öğrenci kendi raporunu görebilir
            if (User.IsInRole("Student"))
            {
                var userName = User.Identity?.Name;
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .Include(s => s.Attendances)
                        .ThenInclude(a => a.Course)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == userName);

                if (student == null) return NotFound();
                
                return View(student);
            }
            return NotFound();
        }

        var targetStudent = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.Attendances)
                .ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (targetStudent == null) return NotFound();

        return View(targetStudent);
    }
}
