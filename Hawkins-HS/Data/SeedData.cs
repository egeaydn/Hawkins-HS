using Microsoft.AspNetCore.Identity;
using Hawkins_HS.Models;

namespace Hawkins_HS.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminAsync(userManager);

        // Seed Demo Data
        await SeedDemoDataAsync(userManager, context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in RoleConstants.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@hawkins.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "Hawkins Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "P@ssw0rd!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
            }
        }
    }

    private static async Task SeedDemoDataAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        // Skip if data already exists
        if (context.Teachers.Any() || context.Students.Any())
        {
            return;
        }

        // Seed Teachers
        var teachers = new List<Teacher>();
        var teacherData = new[]
        {
            new { FullName = "Dr. Emily Watson", Email = "e.watson@hawkins.local", Department = "Matematik", EmployeeNumber = "T001" },
            new { FullName = "Prof. Michael Brown", Email = "m.brown@hawkins.local", Department = "Fizik", EmployeeNumber = "T002" }
        };

        foreach (var data in teacherData)
        {
            var user = new ApplicationUser
            {
                UserName = data.Email.Split('@')[0],
                Email = data.Email,
                FullName = data.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Teacher@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleConstants.Teacher);

                var teacher = new Teacher
                {
                    ApplicationUserId = user.Id,
                    EmployeeNumber = data.EmployeeNumber,
                    Department = data.Department
                };
                teachers.Add(teacher);
            }
        }

        context.Teachers.AddRange(teachers);
        await context.SaveChangesAsync();

        // Seed Students
        var students = new List<Student>();
        var studentData = new[]
        {
            new { FullName = "Lucas Sinclair", Email = "lucas@hawkins.local", StudentNumber = "S2024001", ClassName = "11-A", Year = 11 },
            new { FullName = "Max Mayfield", Email = "max@hawkins.local", StudentNumber = "S2024002", ClassName = "11-A", Year = 11 },
            new { FullName = "Dustin Henderson", Email = "dustin@hawkins.local", StudentNumber = "S2024003", ClassName = "11-B", Year = 11 },
            new { FullName = "Mike Wheeler", Email = "mike@hawkins.local", StudentNumber = "S2024004", ClassName = "11-B", Year = 11 },
            new { FullName = "Will Byers", Email = "will@hawkins.local", StudentNumber = "S2024005", ClassName = "10-A", Year = 10 },
            new { FullName = "Eleven Hopper", Email = "eleven@hawkins.local", StudentNumber = "S2024006", ClassName = "10-A", Year = 10 }
        };

        foreach (var data in studentData)
        {
            var user = new ApplicationUser
            {
                UserName = data.Email.Split('@')[0],
                Email = data.Email,
                FullName = data.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Student@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleConstants.Student);

                var student = new Student
                {
                    ApplicationUserId = user.Id,
                    StudentNumber = data.StudentNumber,
                    ClassName = data.ClassName,
                    Year = data.Year
                };
                students.Add(student);
            }
        }

        context.Students.AddRange(students);
        await context.SaveChangesAsync();

        // Seed Courses
        var courses = new List<Course>
        {
            new Course
            {
                Title = "İleri Matematik",
                Code = "MAT301",
                Description = "Calculus ve lineer cebir konularını içeren ileri matematik dersi",
                Credits = 4,
                TeacherId = teachers[0].Id
            },
            new Course
            {
                Title = "Fizik I",
                Code = "FIZ201",
                Description = "Mekanik, ısı ve termodinamik",
                Credits = 3,
                TeacherId = teachers[1].Id
            },
            new Course
            {
                Title = "Geometri",
                Code = "MAT201",
                Description = "Düzlem ve uzay geometrisi",
                Credits = 3,
                TeacherId = teachers[0].Id
            },
            new Course
            {
                Title = "Elektrik ve Manyetizma",
                Code = "FIZ301",
                Description = "Elektromanyetik teori ve uygulamalar",
                Credits = 4,
                TeacherId = teachers[1].Id
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // Seed Enrollments
        var enrollments = new List<Enrollment>
        {
            new Enrollment { StudentId = students[0].Id, CourseId = courses[0].Id },
            new Enrollment { StudentId = students[0].Id, CourseId = courses[1].Id },
            new Enrollment { StudentId = students[1].Id, CourseId = courses[0].Id },
            new Enrollment { StudentId = students[1].Id, CourseId = courses[2].Id },
            new Enrollment { StudentId = students[2].Id, CourseId = courses[1].Id },
            new Enrollment { StudentId = students[2].Id, CourseId = courses[3].Id },
            new Enrollment { StudentId = students[3].Id, CourseId = courses[0].Id },
            new Enrollment { StudentId = students[4].Id, CourseId = courses[2].Id },
            new Enrollment { StudentId = students[5].Id, CourseId = courses[2].Id }
        };

        context.Enrollments.AddRange(enrollments);
        await context.SaveChangesAsync();

        // Seed Class Schedules
        var schedules = new List<ClassSchedule>
        {
            new ClassSchedule { CourseId = courses[0].Id, Day = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Room = "A101" },
            new ClassSchedule { CourseId = courses[0].Id, Day = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Room = "A101" },
            new ClassSchedule { CourseId = courses[1].Id, Day = DayOfWeek.Tuesday, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), Room = "B202" },
            new ClassSchedule { CourseId = courses[1].Id, Day = DayOfWeek.Thursday, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), Room = "B202" },
            new ClassSchedule { CourseId = courses[2].Id, Day = DayOfWeek.Monday, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 30, 0), Room = "A102" },
            new ClassSchedule { CourseId = courses[3].Id, Day = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(11, 0, 0), Room = "B201" }
        };

        context.ClassSchedules.AddRange(schedules);
        await context.SaveChangesAsync();

        // Seed Exams
        var exams = new List<Exam>
        {
            new Exam
            {
                CourseId = courses[0].Id,
                Title = "Ara Sınav",
                Description = "İlk 5 haftalık konular",
                ExamDate = DateTime.Now.AddDays(14),
                DurationMinutes = 90,
                ExamType = "Written"
            },
            new Exam
            {
                CourseId = courses[1].Id,
                Title = "Quiz 1",
                Description = "Mekanik temel kavramlar",
                ExamDate = DateTime.Now.AddDays(7),
                DurationMinutes = 45,
                ExamType = "Quiz"
            },
            new Exam
            {
                CourseId = courses[2].Id,
                Title = "Final Sınavı",
                Description = "Tüm dönem konuları",
                ExamDate = DateTime.Now.AddDays(30),
                DurationMinutes = 120,
                ExamType = "Written"
            }
        };

        context.Exams.AddRange(exams);
        await context.SaveChangesAsync();

        // Seed Grades (for past exams)
        var grades = new List<Grade>
        {
            new Grade { ExamId = exams[1].Id, StudentId = students[0].Id, Score = 85.5m, Letter = "B" },
            new Grade { ExamId = exams[1].Id, StudentId = students[2].Id, Score = 92.0m, Letter = "A" }
        };

        context.Grades.AddRange(grades);
        await context.SaveChangesAsync();

        // Seed Announcements
        var adminUser = await userManager.FindByEmailAsync("admin@hawkins.local");
        var announcements = new List<Announcement>
        {
            new Announcement
            {
                Title = "Hoş Geldiniz!",
                Body = "Hawkins Lisesi yönetim sistemine hoş geldiniz. Tüm derslerinizi, sınavlarınızı ve notlarınızı burada takip edebilirsiniz.",
                CreatorId = adminUser!.Id,
                Audience = "All",
                IsImportant = true
            },
            new Announcement
            {
                Title = "Sınav Takvimi Güncellendi",
                Body = "Matematik ara sınav tarihi 2 hafta sonraya alındı.",
                CreatorId = adminUser.Id,
                Audience = "Students",
                IsImportant = false
            }
        };

        context.Announcements.AddRange(announcements);
        await context.SaveChangesAsync();
    }
}
