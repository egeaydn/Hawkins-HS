using FluentValidation;
using Hawkins_HS.ViewModels;

namespace Hawkins_HS.Validators;

public class CourseViewModelValidator : AbstractValidator<CourseViewModel>
{
    public CourseViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ders adı gereklidir")
            .MaximumLength(200).WithMessage("Ders adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Ders kodu gereklidir")
            .MaximumLength(20).WithMessage("Ders kodu en fazla 20 karakter olabilir")
            .Matches("^[A-Z]{3}[0-9]{3}$").WithMessage("Ders kodu formatı: ABC123 (3 harf + 3 rakam)");

        RuleFor(x => x.Credits)
            .InclusiveBetween(1, 8).WithMessage("Kredi 1-8 arasında olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");
    }
}

public class ExamViewModelValidator : AbstractValidator<ExamViewModel>
{
    public ExamViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Sınav başlığı gereklidir")
            .MaximumLength(200).WithMessage("Sınav başlığı en fazla 200 karakter olabilir");

        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Ders seçilmelidir");

        RuleFor(x => x.ExamDate)
            .NotEmpty().WithMessage("Sınav tarihi gereklidir")
            .GreaterThan(DateTime.Now).WithMessage("Sınav tarihi gelecekte olmalıdır");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 240).WithMessage("Sınav süresi 15-240 dakika arasında olmalıdır");

        RuleFor(x => x.ExamType)
            .NotEmpty().WithMessage("Sınav tipi seçilmelidir");
    }
}

public class GradeViewModelValidator : AbstractValidator<GradeViewModel>
{
    public GradeViewModelValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100).WithMessage("Puan 0-100 arasında olmalıdır");

        RuleFor(x => x.Letter)
            .NotEmpty().WithMessage("Harf notu gereklidir")
            .Must(BeValidLetterGrade).WithMessage("Geçerli harf notları: A, B, C, D, F");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Yorum en fazla 500 karakter olabilir");
    }

    private bool BeValidLetterGrade(string letter)
    {
        return new[] { "A", "B", "C", "D", "F" }.Contains(letter?.ToUpper());
    }
}

public class AnnouncementViewModelValidator : AbstractValidator<AnnouncementViewModel>
{
    public AnnouncementViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık gereklidir")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("İçerik gereklidir")
            .MaximumLength(5000).WithMessage("İçerik en fazla 5000 karakter olabilir");

        RuleFor(x => x.Audience)
            .NotEmpty().WithMessage("Hedef kitle seçilmelidir")
            .Must(BeValidAudience).WithMessage("Geçerli hedef kitler: All, Students, Teachers veya Class:<sınıf>");
    }

    private bool BeValidAudience(string audience)
    {
        if (string.IsNullOrEmpty(audience)) return false;
        
        var validAudiences = new[] { "All", "Students", "Teachers" };
        return validAudiences.Contains(audience) || audience.StartsWith("Class:");
    }
}
