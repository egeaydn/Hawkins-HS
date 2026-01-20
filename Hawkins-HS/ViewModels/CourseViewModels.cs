using System.ComponentModel.DataAnnotations;

namespace Hawkins_HS.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ders adı gereklidir")]
    [Display(Name = "Ders Adı")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ders kodu gereklidir")]
    [Display(Name = "Ders Kodu")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kredi gereklidir")]
    [Range(1, 8, ErrorMessage = "Kredi 1-8 arasında olmalıdır")]
    [Display(Name = "Kredi")]
    public int Credits { get; set; }

    [Display(Name = "Öğretmen")]
    public int? TeacherId { get; set; }

    public string? TeacherName { get; set; }
    public int EnrolledStudents { get; set; }
}

public class ExamViewModel
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public string? CourseName { get; set; }

    [Required(ErrorMessage = "Sınav başlığı gereklidir")]
    [Display(Name = "Sınav Başlığı")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sınav tarihi gereklidir")]
    [Display(Name = "Sınav Tarihi")]
    [DataType(DataType.DateTime)]
    public DateTime ExamDate { get; set; } = DateTime.Now.AddDays(7);

    [Required(ErrorMessage = "Süre gereklidir")]
    [Range(15, 240, ErrorMessage = "Süre 15-240 dakika arasında olmalıdır")]
    [Display(Name = "Süre (Dakika)")]
    public int DurationMinutes { get; set; } = 90;

    [Display(Name = "Sınav Tipi")]
    public string ExamType { get; set; } = "Written";
}

public class GradeViewModel
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string? ExamTitle { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Puan 0-100 arasında olmalıdır")]
    [Display(Name = "Puan")]
    public decimal Score { get; set; }

    [Required]
    [Display(Name = "Harf Notu")]
    public string Letter { get; set; } = string.Empty;

    [Display(Name = "Yorum")]
    public string? Comment { get; set; }

    public DateTime GradedAt { get; set; }
}

public class ScheduleViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }

    [Required]
    [Display(Name = "Gün")]
    public DayOfWeek Day { get; set; }

    [Required]
    [Display(Name = "Başlangıç Saati")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Display(Name = "Bitiş Saati")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Display(Name = "Sınıf")]
    public string Room { get; set; } = string.Empty;
}

public class AnnouncementViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık gereklidir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik gereklidir")]
    [Display(Name = "İçerik")]
    [DataType(DataType.MultilineText)]
    public string Body { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Hedef Kitle")]
    public string Audience { get; set; } = "All";

    [Display(Name = "Önemli")]
    public bool IsImportant { get; set; }

    public string? CreatorName { get; set; }
    public DateTime CreatedAt { get; set; }
}
