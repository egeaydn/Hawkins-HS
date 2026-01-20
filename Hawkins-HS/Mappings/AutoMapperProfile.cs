using AutoMapper;
using Hawkins_HS.Models;
using Hawkins_HS.ViewModels;

namespace Hawkins_HS.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Course Mappings
        CreateMap<Course, CourseViewModel>()
            .ForMember(dest => dest.TeacherName, 
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.ApplicationUser.FullName : "Atanmamış"))
            .ForMember(dest => dest.EnrolledStudents,
                opt => opt.MapFrom(src => src.Enrollments.Count));

        CreateMap<CourseViewModel, Course>()
            .ForMember(dest => dest.Teacher, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
            .ForMember(dest => dest.ClassSchedules, opt => opt.Ignore())
            .ForMember(dest => dest.Exams, opt => opt.Ignore())
            .ForMember(dest => dest.Attendances, opt => opt.Ignore());

        // Exam Mappings
        CreateMap<Exam, ExamViewModel>()
            .ForMember(dest => dest.CourseName,
                opt => opt.MapFrom(src => src.Course.Title));

        CreateMap<ExamViewModel, Exam>()
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.Grades, opt => opt.Ignore());

        // Grade Mappings
        CreateMap<Grade, GradeViewModel>()
            .ForMember(dest => dest.ExamTitle,
                opt => opt.MapFrom(src => src.Exam.Title))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student.ApplicationUser.FullName));

        CreateMap<GradeViewModel, Grade>()
            .ForMember(dest => dest.Exam, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore());

        // ClassSchedule Mappings
        CreateMap<ClassSchedule, ScheduleViewModel>()
            .ForMember(dest => dest.CourseName,
                opt => opt.MapFrom(src => src.Course.Title));

        CreateMap<ScheduleViewModel, ClassSchedule>()
            .ForMember(dest => dest.Course, opt => opt.Ignore());

        // Announcement Mappings
        CreateMap<Announcement, AnnouncementViewModel>()
            .ForMember(dest => dest.CreatorName,
                opt => opt.MapFrom(src => src.Creator.FullName));

        CreateMap<AnnouncementViewModel, Announcement>()
            .ForMember(dest => dest.Creator, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore());
    }
}
