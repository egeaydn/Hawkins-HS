using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hawkins_HS.Data;
using Hawkins_HS.Models;

namespace Hawkins_HS.Controllers;

[Authorize]
public class GradesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GradesController> _logger;

    public GradesController(ApplicationDbContext context, ILogger<GradesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Grades/EnterGrades/5 (ExamId)
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> EnterGrades(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.ApplicationUser)
            .Include(e => e.Grades)
                .ThenInclude(g => g.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null) return NotFound();

        // Derse kayıtlı öğrenciler
        var enrolledStudents = exam.Course.Enrollments.Select(e => e.Student).ToList();
        
        // Her öğrenci için not bilgisi
        var studentGrades = enrolledStudents.Select(student => new
        {
            Student = student,
            Grade = exam.Grades.FirstOrDefault(g => g.StudentId == student.Id)
        }).ToList();

        ViewBag.Exam = exam;
        ViewBag.StudentGrades = studentGrades;

        return View();
    }

    // POST: Grades/SaveGrade
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> SaveGrade(int examId, int studentId, decimal score)
    {
        if (score < 0 || score > 100)
        {
            TempData["Error"] = "Not 0 ile 100 arasında olmalıdır.";
            return RedirectToAction(nameof(EnterGrades), new { id = examId });
        }

        // Harf notunu hesapla
        string letterGrade = CalculateLetterGrade(score);

        // Mevcut notu kontrol et
        var existingGrade = await _context.Grades
            .FirstOrDefaultAsync(g => g.ExamId == examId && g.StudentId == studentId);

        if (existingGrade != null)
        {
            // Güncelle
            existingGrade.Score = score;
            existingGrade.Letter = letterGrade;
            existingGrade.GradedAt = DateTime.UtcNow;
            _context.Update(existingGrade);
        }
        else
        {
            // Yeni not ekle
            var grade = new Grade
            {
                ExamId = examId,
                StudentId = studentId,
                Score = score,
                Letter = letterGrade,
                GradedAt = DateTime.UtcNow
            };
            _context.Add(grade);
        }

        await _context.SaveChangesAsync();
        
        var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .FirstOrDefaultAsync(s => s.Id == studentId);
        
        _logger.LogInformation("Grade saved: {StudentName} - Score: {Score}", 
            student?.ApplicationUser.FullName, score);
        
        TempData["Success"] = "Not başarıyla kaydedildi.";
        return RedirectToAction(nameof(EnterGrades), new { id = examId });
    }

    // POST: Grades/DeleteGrade
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteGrade(int id, int examId)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade != null)
        {
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Grade deleted: GradeId {GradeId}", id);
            TempData["Success"] = "Not silindi.";
        }

        return RedirectToAction(nameof(EnterGrades), new { id = examId });
    }

    // Helper method: Harf notu hesaplama
    private string CalculateLetterGrade(decimal score)
    {
        if (score >= 85) return "A";
        if (score >= 70) return "B";
        if (score >= 55) return "C";
        if (score >= 40) return "D";
        return "F";
    }
}
