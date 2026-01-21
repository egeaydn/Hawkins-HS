using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Hawkins_HS.Data;
using Hawkins_HS.Models;
using Hawkins_HS.ViewModels;

namespace Hawkins_HS.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<CoursesController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        var coursesQuery = _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .Include(c => c.Enrollments)
            .AsQueryable();

        // Öğretmen sadece kendi derslerini görsün
        if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity?.Name;
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.ApplicationUser.UserName == userName);
            
            if (teacher != null)
            {
                coursesQuery = coursesQuery.Where(c => c.TeacherId == teacher.Id);
            }
        }

        var courses = await coursesQuery.ToListAsync();
        var viewModels = _mapper.Map<List<CourseViewModel>>(courses);
        return View(viewModels);
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t!.ApplicationUser)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.ApplicationUser)
            .Include(c => c.ClassSchedules)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (course == null) return NotFound();

        // Öğretmenin bu dersi verip vermediğini kontrol et
        bool isTeacherOfCourse = false;
        if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity?.Name;
            isTeacherOfCourse = course.Teacher?.ApplicationUser?.UserName == userName;
        }
        else if (User.IsInRole("Admin"))
        {
            isTeacherOfCourse = true; // Admin her şeyi yapabilir
        }

        ViewBag.IsTeacherOfCourse = isTeacherOfCourse;

        return View(course);
    }

    // GET: Courses/Create
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Select(t => new { t.Id, Name = t.ApplicationUser.FullName })
            .ToListAsync();

        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(CourseViewModel viewModel)
    {
        // Ders kodunun benzersiz olup olmadığını kontrol et
        if (await _context.Courses.AnyAsync(c => c.Code == viewModel.Code))
        {
            ModelState.AddModelError("Code", "Bu ders kodu zaten kullanılıyor. Lütfen farklı bir kod girin.");
        }

        if (ModelState.IsValid)
        {
            var course = _mapper.Map<Course>(viewModel);
            
            _context.Add(course);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Course created: {CourseTitle} ({CourseCode})", course.Title, course.Code);
            TempData["Success"] = "Ders başarıyla oluşturuldu.";
            
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Select(t => new { t.Id, Name = t.ApplicationUser.FullName })
            .ToListAsync();

        return View(viewModel);
    }

    // GET: Courses/Edit/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        var viewModel = _mapper.Map<CourseViewModel>(course);

        ViewBag.Teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Select(t => new { t.Id, Name = t.ApplicationUser.FullName })
            .ToListAsync();

        return View(viewModel);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, CourseViewModel viewModel)
    {
        if (id != viewModel.Id) return NotFound();

        // Ders kodunun başka bir ders tarafından kullanılıp kullanılmadığını kontrol et
        if (await _context.Courses.AnyAsync(c => c.Code == viewModel.Code && c.Id != id))
        {
            ModelState.AddModelError("Code", "Bu ders kodu başka bir ders tarafından kullanılıyor. Lütfen farklı bir kod girin.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null) return NotFound();

                _mapper.Map(viewModel, course);
                
                _context.Update(course);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Course updated: {CourseTitle} ({CourseCode})", course.Title, course.Code);
                TempData["Success"] = "Ders başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(viewModel.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Teachers = await _context.Teachers
            .Include(t => t.ApplicationUser)
            .Select(t => new { t.Id, Name = t.ApplicationUser.FullName })
            .ToListAsync();

        return View(viewModel);
    }

    // GET: Courses/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (course == null) return NotFound();

        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Course deleted: {CourseTitle} ({CourseCode})", course.Title, course.Code);
            TempData["Success"] = "Ders başarıyla silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Courses/Enroll/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Enroll(int? id)
    {
        if (id == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        var enrolledStudentIds = course.Enrollments.Select(e => e.StudentId).ToList();

        var availableStudents = await _context.Students
            .Include(s => s.ApplicationUser)
            .Where(s => !enrolledStudentIds.Contains(s.Id))
            .Select(s => new { s.Id, Name = s.ApplicationUser.FullName, s.StudentNumber, s.ClassName })
            .ToListAsync();

        ViewBag.Course = course;
        ViewBag.AvailableStudents = availableStudents;

        return View();
    }

    // POST: Courses/Enroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> EnrollStudent(int courseId, int studentId)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Active"
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Öğrenci derse kaydedildi.";
        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }
}
