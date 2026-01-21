using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Hawkins_HS.Data;
using Hawkins_HS.Models;
using Hawkins_HS.ViewModels;
using Hawkins_HS.Services;

namespace Hawkins_HS.Controllers;

[Authorize]
public class ExamsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ExamsController> _logger;
    private readonly INotificationService _notificationService;

    public ExamsController(ApplicationDbContext context, IMapper mapper, ILogger<ExamsController> logger, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    // GET: Exams
    public async Task<IActionResult> Index()
    {
        var examsQuery = _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t!.ApplicationUser)
            .AsQueryable();

        // Öğretmen ise sadece kendi derslerinin sınavlarını göster
        string? currentTeacherUserName = null;
        if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity!.Name;
            currentTeacherUserName = userName;
            examsQuery = examsQuery.Where(e => e.Course.Teacher!.ApplicationUser.UserName == userName);
        }

        var exams = await examsQuery
            .OrderByDescending(e => e.ExamDate)
            .ToListAsync();

        var viewModels = _mapper.Map<List<ExamViewModel>>(exams);
        
        // View'a öğretmenin kendi sınavlarını belirtmek için kullanıcı adını gönder
        ViewBag.CurrentTeacherUserName = currentTeacherUserName;
        ViewBag.IsAdmin = User.IsInRole("Admin");
        
        return View(viewModels);
    }

    // GET: Exams/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t!.ApplicationUser)
            .Include(e => e.Grades)
                .ThenInclude(g => g.Student)
                    .ThenInclude(s => s.ApplicationUser)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (exam == null) return NotFound();

        // Öğretmen ise sadece kendi dersinin sınavına erişebilir
        if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity!.Name;
            if (exam.Course.Teacher?.ApplicationUser?.UserName != userName)
            {
                TempData["Error"] = "Bu sınava erişim yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Öğretmen kendi sınavını görebilir, admin her şeyi görebilir
        bool isTeacherOfCourse = false;
        if (User.IsInRole("Admin"))
        {
            isTeacherOfCourse = true;
        }
        else if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity!.Name;
            isTeacherOfCourse = exam.Course.Teacher?.ApplicationUser?.UserName == userName;
        }
        ViewBag.IsTeacherOfCourse = isTeacherOfCourse;

        return View(exam);
    }

    // GET: Exams/Create
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(int? courseId)
    {
        var coursesQuery = _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
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

        var courses = await coursesQuery
            .Select(c => new { 
                c.Id, 
                DisplayText = c.Code + " - " + c.Title
            })
            .ToListAsync();

        ViewBag.Courses = new SelectList(courses, "Id", "DisplayText");

        var viewModel = new ExamViewModel();
        if (courseId.HasValue)
        {
            viewModel.CourseId = courseId.Value;
        }

        return View(viewModel);
    }

    // POST: Exams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(ExamViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var exam = _mapper.Map<Exam>(viewModel);
            
            _context.Add(exam);
            await _context.SaveChangesAsync();
            
            // Bildirim gönder
            await _notificationService.NotifyExamCreatedAsync(exam);
            
            _logger.LogInformation("Exam created: {ExamTitle} for course {CourseId}", exam.Title, exam.CourseId);
            TempData["Success"] = "Sınav başarıyla oluşturuldu.";
            
            return RedirectToAction(nameof(Details), new { id = exam.Id });
        }

        var courses = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .Select(c => new { 
                c.Id, 
                DisplayText = c.Code + " - " + c.Title
            })
            .ToListAsync();

        ViewBag.Courses = new SelectList(courses, "Id", "DisplayText");

        return View(viewModel);
    }

    // GET: Exams/Edit/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t!.ApplicationUser)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        // Öğretmen ise sadece kendi dersinin sınavını düzenleyebilir
        if (User.IsInRole("Teacher"))
        {
            var userName = User.Identity!.Name;
            if (exam.Course.Teacher?.ApplicationUser?.UserName != userName)
            {
                TempData["Error"] = "Bu sınavı düzenleme yetkiniz yok.";
                return RedirectToAction(nameof(Index));
            }
        }

        var viewModel = _mapper.Map<ExamViewModel>(exam);

        var courses = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .Select(c => new { 
                c.Id, 
                DisplayText = c.Code + " - " + c.Title
            })
            .ToListAsync();

        ViewBag.Courses = new SelectList(courses, "Id", "DisplayText", viewModel.CourseId);

        return View(viewModel);
    }

    // POST: Exams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, ExamViewModel viewModel)
    {
        if (id != viewModel.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var exam = await _context.Exams
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Teacher)
                            .ThenInclude(t => t!.ApplicationUser)
                    .FirstOrDefaultAsync(e => e.Id == id);
                if (exam == null) return NotFound();

                // Öğretmen ise sadece kendi dersinin sınavını düzenleyebilir
                if (User.IsInRole("Teacher"))
                {
                    var userName = User.Identity!.Name;
                    if (exam.Course.Teacher?.ApplicationUser?.UserName != userName)
                    {
                        TempData["Error"] = "Bu sınavı düzenleme yetkiniz yok.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _mapper.Map(viewModel, exam);
                
                _context.Update(exam);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Exam updated: {ExamTitle}", exam.Title);
                TempData["Success"] = "Sınav başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(viewModel.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        var courses = await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.ApplicationUser)
            .Select(c => new { 
                c.Id, 
                DisplayText = c.Code + " - " + c.Title
            })
            .ToListAsync();

        ViewBag.Courses = new SelectList(courses, "Id", "DisplayText", viewModel.CourseId);

        return View(viewModel);
    }

    // GET: Exams/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (exam == null) return NotFound();

        return View(exam);
    }

    // POST: Exams/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Exam deleted: {ExamTitle}", exam.Title);
            TempData["Success"] = "Sınav başarıyla silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ExamExists(int id)
    {
        return _context.Exams.Any(e => e.Id == id);
    }
}
