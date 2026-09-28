using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Infrastructure.Data.Seed;

/// <summary>
/// Applies pending migrations and seeds realistic development/demo data:
/// roles, an administrator and examination officer account, the academic
/// reference data (faculties/departments/programs/courses/terms), grading
/// rules, sample students with real Identity logins, their results, resulting
/// re-exam eligibility, sample applications in different statuses, exam
/// rooms and invigilators. Idempotent - safe to run on every startup.
/// </summary>
public static class DbInitializer
{
    public const string DefaultPassword = "Passw0rd!123";

    private sealed record SeedStudentSpec(string Number, string Name, string Email, Gender Gender);

    private static readonly IReadOnlyList<SeedStudentSpec> ExampleStudents = new[]
    {
        new SeedStudentSpec("CS/2022/001", "Kwame Boateng", "kwame.boateng@student.reexam.edu", Gender.Male),
        new SeedStudentSpec("CS/2022/002", "Ama Serwaa", "ama.serwaa@student.reexam.edu", Gender.Female),
        new SeedStudentSpec("CS/2022/003", "Kojo Mensah", "kojo.mensah@student.reexam.edu", Gender.Male),
        new SeedStudentSpec("CS/2022/004", "Efua Asante", "efua.asante@student.reexam.edu", Gender.Female),
        new SeedStudentSpec("CS/2022/005", "Yaw Owusu", "yaw.owusu@student.reexam.edu", Gender.Male),
        new SeedStudentSpec("CS/2022/006", "Student User", "student@reexam.edu", Gender.Male),
        new SeedStudentSpec("CS/2022/007", "Abena Asamoah", "abena.asamoah@student.reexam.edu", Gender.Female),
        new SeedStudentSpec("CS/2022/008", "Kofi Owusu-Ansah", "kofi.owusuansah@student.reexam.edu", Gender.Male),
        new SeedStudentSpec("CS/2022/009", "Adjoa Nyarko", "adjoa.nyarko@student.reexam.edu", Gender.Female),
        new SeedStudentSpec("CS/2022/010", "Kwabena Darko", "kwabena.darko@student.reexam.edu", Gender.Male),
    };

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        var admin = await SeedUserAsync(userManager, "admin@reexam.edu", "System Administrator", Roles.Administrator);
        var officer = await SeedUserAsync(userManager, "officer@reexam.edu", "Grace Mensah", Roles.ExaminationOfficer);

        // Academic years are seeded after grading rules and the academic structure,
        // so a database that has them finished a full seed. (An earlier failed run
        // could leave faculties behind without years - that case falls through and
        // the stages below reuse what's already there.)
        if (await context.AcademicYears.AnyAsync())
        {
            await SeedAdditionalStudentIfMissingAsync(context, userManager);
            await SeedExampleResultForStudentIfMissingAsync(context, "student@reexam.edu", officer.Id);
            return; // domain data already seeded
        }

        // All-or-nothing, so a failure part-way through can't leave half-seeded data.
        await using var transaction = await context.Database.BeginTransactionAsync();

        var gradingRules = await SeedGradingRulesAsync(context);
        var (departments, programs) = await SeedAcademicStructureAsync(context);
        var (academicYears, semesters) = await SeedAcademicYearsAsync(context);
        var courses = await SeedCoursesAsync(context, departments, academicYears, semesters);
        var students = await SeedStudentsAsync(context, userManager, departments, programs, academicYears, semesters);
        await SeedResultsEligibilityAndApplicationsAsync(context, gradingRules, students, courses, academicYears, semesters, officer.Id);
        await SeedExamInfrastructureAsync(context, departments);
        await SeedSystemSettingsAsync(context);
        await SeedExampleResultForStudentIfMissingAsync(context, "student@reexam.edu", officer.Id);

        await transaction.CommitAsync();
    }

    private sealed record ExampleCourseScenario(string CourseCode, decimal OriginalMark, bool CreateApplication, bool Approve, bool ScheduleExam, bool PublishResult, decimal? ReExamMark = null);

    /// <summary>
    /// Gives one specific already-provisioned student a demo-worthy academic history covering every
    /// distinct state the Student area can show: a passing original result, a course that's eligible
    /// but not yet applied for (Eligible Courses), a pending application (My Applications), an approved
    /// application with a scheduled exam (Exam Schedule), and an approved application with a published
    /// re-exam result (My Results). Idempotent - skipped once that student already has any result.
    /// </summary>
    private static async Task SeedExampleResultForStudentIfMissingAsync(ApplicationDbContext context, string studentEmail, string officerUserId)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Email == studentEmail);
        if (student is null)
        {
            return;
        }

        var gradingRules = await context.GradingRules.ToListAsync();
        var priorYear = await context.AcademicYears.OrderBy(y => y.StartDate).FirstAsync();
        var priorSemester = await context.Semesters.Where(s => s.AcademicYearId == priorYear.Id).OrderBy(s => s.Id).FirstAsync();
        var reExamYear = await context.AcademicYears.OrderByDescending(y => y.StartDate).FirstAsync();
        var reExamSemester = await context.Semesters.Where(s => s.AcademicYearId == reExamYear.Id).OrderBy(s => s.Id).FirstAsync();
        var examRoom = await context.ExamRooms.OrderBy(r => r.Id).FirstAsync();
        var invigilator = await context.Invigilators.OrderBy(i => i.Id).FirstAsync();

        // CS102 is a plain passing result - no eligibility, no application - kept only for
        // Academic Profile's result history to show a mix of outcomes, not just failures.
        var passCourse = await context.Courses.FirstAsync(c => c.Code == "CS102");
        if (!await context.StudentResults.AnyAsync(r => r.StudentId == student.Id && r.CourseId == passCourse.Id))
        {
            var (passGrade, passGradePoint, passStatus) = Grade(gradingRules, 78m);
            context.StudentResults.Add(new StudentResult
            {
                Student = student,
                Course = passCourse,
                AcademicYear = priorYear,
                Semester = priorSemester,
                Marks = 78m,
                Grade = passGrade,
                GradePoint = passGradePoint,
                Status = passStatus
            });
            await context.SaveChangesAsync();
        }

        var scenarios = new[]
        {
            new ExampleCourseScenario("CS101", 41m, CreateApplication: true, Approve: false, ScheduleExam: false, PublishResult: false), // My Applications: Pending
            new ExampleCourseScenario("CS201", 38m, CreateApplication: false, Approve: false, ScheduleExam: false, PublishResult: false), // Eligible Courses: not yet applied
            new ExampleCourseScenario("CS202", 30m, CreateApplication: true, Approve: true, ScheduleExam: true, PublishResult: false), // Exam Schedule: approved + scheduled
            new ExampleCourseScenario("CS301", 45m, CreateApplication: true, Approve: true, ScheduleExam: false, PublishResult: true, ReExamMark: 68m), // My Results: published
        };

        foreach (var scenario in scenarios)
        {
            var course = await context.Courses.FirstAsync(c => c.Code == scenario.CourseCode);
            if (await context.StudentResults.AnyAsync(r => r.StudentId == student.Id && r.CourseId == course.Id))
            {
                continue; // already seeded on a previous run
            }

            var (grade, gradePoint, status) = Grade(gradingRules, scenario.OriginalMark);

            var failResult = new StudentResult
            {
                Student = student,
                Course = course,
                AcademicYear = priorYear,
                Semester = priorSemester,
                Marks = scenario.OriginalMark,
                Grade = grade,
                GradePoint = gradePoint,
                Status = status
            };
            context.StudentResults.Add(failResult);
            await context.SaveChangesAsync();

            var eligibility = new ReExamEligibility
            {
                Student = student,
                StudentResult = failResult,
                Course = course,
                AcademicYear = priorYear,
                Semester = priorSemester,
                Status = EligibilityStatus.Eligible,
                Reason = $"Failed {course.Code} with a mark of {failResult.Marks:0.##}, below the minimum pass mark. Retake permitted."
            };
            context.ReExamEligibilities.Add(eligibility);

            if (!await context.ReExamSubjects.AnyAsync(s => s.CourseId == course.Id && s.AcademicYearId == reExamYear.Id && s.SemesterId == reExamSemester.Id))
            {
                context.ReExamSubjects.Add(new ReExamSubject { Course = course, AcademicYear = reExamYear, Semester = reExamSemester, IsActive = true });
            }

            await context.SaveChangesAsync();

            if (!scenario.CreateApplication)
            {
                continue;
            }

            var application = new ReExamApplication
            {
                Student = student,
                Course = course,
                ReExamEligibility = eligibility,
                AcademicYear = reExamYear,
                Semester = reExamSemester,
                Status = scenario.Approve ? ApplicationStatus.Approved : ApplicationStatus.Pending,
                ReviewedByUserId = scenario.Approve ? officerUserId : null,
                ReviewedAt = scenario.Approve ? DateTime.UtcNow.AddDays(-5) : null
            };
            context.ReExamApplications.Add(application);
            await context.SaveChangesAsync();

            if (scenario.ScheduleExam)
            {
                context.ExamSchedules.Add(new ExamSchedule
                {
                    Course = course,
                    AcademicYear = reExamYear,
                    Semester = reExamSemester,
                    ExamDate = DateTime.UtcNow.Date.AddDays(14),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(11, 0, 0),
                    ExamRoom = examRoom,
                    Invigilator = invigilator,
                    Status = ExamStatus.Scheduled
                });
                await context.SaveChangesAsync();
            }

            if (scenario.PublishResult)
            {
                var (reExamGrade, reExamGradePoint, reExamStatus) = Grade(gradingRules, scenario.ReExamMark!.Value);
                context.ReExamResults.Add(new ReExamResult
                {
                    ReExamApplication = application,
                    Student = student,
                    Course = course,
                    OriginalMark = scenario.OriginalMark,
                    ReExamMark = scenario.ReExamMark,
                    FinalMark = scenario.ReExamMark.Value,
                    Grade = reExamGrade,
                    GradePoint = reExamGradePoint,
                    Status = reExamStatus,
                    IsVerified = true,
                    VerifiedByUserId = officerUserId,
                    VerifiedAt = DateTime.UtcNow.AddDays(-3),
                    IsPublished = true,
                    PublishedByUserId = officerUserId,
                    PublishedAt = DateTime.UtcNow.AddDays(-1)
                });
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext context)
    {
        context.SystemSettings.AddRange(
            new SystemSetting
            {
                Key = "ReExam:FinalMarkPolicy",
                Value = "ReExamMarkReplacesOriginal",
                Description = "University's re-exam final-mark policy, documented here for reference. The current re-exam mark's official grading behavior implements this policy."
            },
            new SystemSetting
            {
                Key = "ReExam:MinimumPassMark",
                Value = "50",
                Description = "Documents the pass threshold; the actual pass/fail boundary is enforced by the configured Grading Rules."
            });

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName)
                {
                    Description = roleName switch
                    {
                        Roles.Administrator => "Full system access.",
                        Roles.ExaminationOfficer => "Manages the re-examination process.",
                        Roles.Student => "Limited access to the student's own information.",
                        _ => null
                    }
                });
            }
        }
    }

    private static async Task SeedAdditionalStudentIfMissingAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var department = await context.Departments.OrderBy(d => d.Id).FirstAsync();
        var program = await context.AcademicPrograms.Where(p => p.DepartmentId == department.Id).OrderBy(p => p.Id).FirstAsync();
        var academicYear = await context.AcademicYears.OrderBy(y => y.StartDate).FirstAsync();
        var semester = await context.Semesters.Where(s => s.AcademicYearId == academicYear.Id).OrderBy(s => s.Id).FirstAsync();

        foreach (var spec in ExampleStudents)
        {
            await SeedOneAdditionalStudentAsync(context, userManager, spec, department, program, academicYear, semester);
        }
    }

    private static async Task SeedOneAdditionalStudentAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SeedStudentSpec spec,
        Department department,
        AcademicProgram program,
        AcademicYear academicYear,
        Semester semester)
    {
        if (await userManager.FindByEmailAsync(spec.Email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = spec.Email,
            Email = spec.Email,
            EmailConfirmed = true,
            FullName = spec.Name,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed user '{spec.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, Roles.Student);

        context.Students.Add(new Student
        {
            StudentNumber = spec.Number,
            UserId = user.Id,
            FullName = spec.Name,
            Gender = spec.Gender,
            DateOfBirth = new DateTime(2002, 3, 15),
            PhoneNumber = "0000000000",
            Email = spec.Email,
            Department = department,
            Program = program,
            Level = 2,
            AcademicYear = academicYear,
            Semester = semester,
            EnrollmentDate = new DateTime(2022, 9, 1),
            Status = StudentStatus.Active
        });

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> SeedUserAsync(UserManager<ApplicationUser> userManager, string email, string fullName, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to seed user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static async Task<List<GradingRule>> SeedGradingRulesAsync(ApplicationDbContext context)
    {
        var existing = await context.GradingRules.ToListAsync();
        if (existing.Count > 0)
        {
            return existing;
        }

        var rules = new List<GradingRule>
        {
            new() { MinMark = 90, MaxMark = 100, Grade = "A", GradePoint = 4.00m, IsPassing = true, Description = "Excellent" },
            new() { MinMark = 80, MaxMark = 89.99m, Grade = "B", GradePoint = 3.00m, IsPassing = true, Description = "Very Good" },
            new() { MinMark = 70, MaxMark = 79.99m, Grade = "C", GradePoint = 2.00m, IsPassing = true, Description = "Good" },
            new() { MinMark = 60, MaxMark = 69.99m, Grade = "D", GradePoint = 1.50m, IsPassing = true, Description = "Satisfactory" },
            new() { MinMark = 50, MaxMark = 59.99m, Grade = "E", GradePoint = 1.00m, IsPassing = true, Description = "Pass" },
            new() { MinMark = 0, MaxMark = 49.99m, Grade = "F", GradePoint = 0.00m, IsPassing = false, Description = "Fail" },
        };

        context.GradingRules.AddRange(rules);
        await context.SaveChangesAsync();
        return rules;
    }

    private static (string grade, decimal gradePoint, ResultStatus status) Grade(IReadOnlyList<GradingRule> rules, decimal mark)
    {
        var rule = rules.First(r => mark >= r.MinMark && mark <= r.MaxMark);
        return (rule.Grade, rule.GradePoint, rule.IsPassing ? ResultStatus.Pass : ResultStatus.Fail);
    }

    private static async Task<(Dictionary<string, Department> departments, Dictionary<string, AcademicProgram> programs)> SeedAcademicStructureAsync(ApplicationDbContext context)
    {
        if (await context.Faculties.AnyAsync())
        {
            return (
                await context.Departments.ToDictionaryAsync(d => d.Code),
                await context.AcademicPrograms.ToDictionaryAsync(p => p.Code));
        }

        var scienceFaculty = new Faculty { Name = "Faculty of Science", Code = "SCI" };
        var engineeringFaculty = new Faculty { Name = "Faculty of Engineering", Code = "ENG" };
        context.Faculties.AddRange(scienceFaculty, engineeringFaculty);

        var csDept = new Department { Name = "Computer Science", Code = "CS", Faculty = scienceFaculty };
        var itDept = new Department { Name = "Information Technology", Code = "IT", Faculty = scienceFaculty };
        var eeeDept = new Department { Name = "Electrical Engineering", Code = "EEE", Faculty = engineeringFaculty };
        context.Departments.AddRange(csDept, itDept, eeeDept);

        var bscCs = new AcademicProgram { Name = "BSc Computer Science", Code = "BSCCS", DurationYears = 4, Department = csDept, Status = ProgramStatus.Active };
        var bscIt = new AcademicProgram { Name = "BSc Information Technology", Code = "BSCIT", DurationYears = 4, Department = itDept, Status = ProgramStatus.Active };
        context.AcademicPrograms.AddRange(bscCs, bscIt);

        await context.SaveChangesAsync();

        var departments = new Dictionary<string, Department> { ["CS"] = csDept, ["IT"] = itDept, ["EEE"] = eeeDept };
        var programs = new Dictionary<string, AcademicProgram> { ["BSCCS"] = bscCs, ["BSCIT"] = bscIt };
        return (departments, programs);
    }

    private static async Task<(Dictionary<string, AcademicYear> years, Dictionary<string, Semester> semesters)> SeedAcademicYearsAsync(ApplicationDbContext context)
    {
        var previousYear = new AcademicYear
        {
            Name = "2024/2025",
            StartDate = new DateTime(2024, 9, 1),
            EndDate = new DateTime(2025, 7, 31),
            IsActive = false
        };
        var currentYear = new AcademicYear
        {
            Name = "2025/2026",
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2026, 7, 31),
            IsActive = true
        };
        context.AcademicYears.AddRange(previousYear, currentYear);
        await context.SaveChangesAsync();

        var prevSem2 = new Semester { Name = "Semester Two", AcademicYear = previousYear, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 6, 30), IsActive = false };
        var currentSem1 = new Semester { Name = "Semester One", AcademicYear = currentYear, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 1, 31), IsActive = true };
        var currentSem2 = new Semester { Name = "Semester Two", AcademicYear = currentYear, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 6, 30), IsActive = false };
        context.Semesters.AddRange(prevSem2, currentSem1, currentSem2);
        await context.SaveChangesAsync();

        var years = new Dictionary<string, AcademicYear> { ["2024/2025"] = previousYear, ["2025/2026"] = currentYear };
        var semesters = new Dictionary<string, Semester>
        {
            ["2024/2025-Two"] = prevSem2,
            ["2025/2026-One"] = currentSem1,
            ["2025/2026-Two"] = currentSem2
        };
        return (years, semesters);
    }

    private static async Task<Dictionary<string, Course>> SeedCoursesAsync(
        ApplicationDbContext context,
        Dictionary<string, Department> departments,
        Dictionary<string, AcademicYear> years,
        Dictionary<string, Semester> semesters)
    {
        var priorYear = years["2024/2025"];
        var priorSemester = semesters["2024/2025-Two"];
        var cs = departments["CS"];

        var courses = new[]
        {
            new Course { Code = "CS101", Name = "Introduction to Programming", CreditHours = 3, Level = 1, Department = cs, AcademicYear = priorYear, Semester = priorSemester },
            new Course { Code = "CS102", Name = "Data Structures", CreditHours = 3, Level = 1, Department = cs, AcademicYear = priorYear, Semester = priorSemester },
            new Course { Code = "CS201", Name = "Database Systems", CreditHours = 3, Level = 2, Department = cs, AcademicYear = priorYear, Semester = priorSemester },
            new Course { Code = "CS202", Name = "Computer Networks", CreditHours = 3, Level = 2, Department = cs, AcademicYear = priorYear, Semester = priorSemester },
            new Course { Code = "CS301", Name = "Operating Systems", CreditHours = 3, Level = 3, Department = cs, AcademicYear = priorYear, Semester = priorSemester },
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        return courses.ToDictionary(c => c.Code);
    }

    private static async Task<List<Student>> SeedStudentsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Department> departments,
        Dictionary<string, AcademicProgram> programs,
        Dictionary<string, AcademicYear> years,
        Dictionary<string, Semester> semesters)
    {
        var cs = departments["CS"];
        var bscCs = programs["BSCCS"];
        var priorYear = years["2024/2025"];
        var priorSemester = semesters["2024/2025-Two"];

        var seedStudents = ExampleStudents;

        var students = new List<Student>();

        foreach (var s in seedStudents)
        {
            var user = new ApplicationUser
            {
                UserName = s.Email,
                Email = s.Email,
                EmailConfirmed = true,
                FullName = s.Name,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed student user '{s.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, Roles.Student);

            var student = new Student
            {
                StudentNumber = s.Number,
                UserId = user.Id,
                FullName = s.Name,
                Gender = s.Gender,
                DateOfBirth = new DateTime(2002, 3, 15),
                PhoneNumber = "0000000000",
                Email = s.Email,
                Department = cs,
                Program = bscCs,
                Level = 2,
                AcademicYear = priorYear,
                Semester = priorSemester,
                EnrollmentDate = new DateTime(2022, 9, 1),
                Status = StudentStatus.Active
            };

            students.Add(student);
        }

        context.Students.AddRange(students);
        await context.SaveChangesAsync();
        return students;
    }

    private static async Task SeedResultsEligibilityAndApplicationsAsync(
        ApplicationDbContext context,
        List<GradingRule> gradingRules,
        List<Student> students,
        Dictionary<string, Course> courses,
        Dictionary<string, AcademicYear> years,
        Dictionary<string, Semester> semesters,
        string officerUserId)
    {
        var priorYear = years["2024/2025"];
        var priorSemester = semesters["2024/2025-Two"];
        var reExamYear = years["2025/2026"];
        var reExamSemester = semesters["2025/2026-One"];

        // (student index, course code, mark)
        var marks = new (int StudentIndex, string CourseCode, decimal Mark)[]
        {
            (0, "CS101", 42m), (0, "CS102", 75m),
            (1, "CS101", 88m), (1, "CS102", 38m),
            (2, "CS201", 45m), (2, "CS102", 91m),
            (3, "CS202", 30m), (3, "CS101", 67m),
            (4, "CS101", 72m), (4, "CS102", 81m),
        };

        var results = new List<StudentResult>();
        foreach (var (studentIndex, courseCode, mark) in marks)
        {
            var (grade, gradePoint, status) = Grade(gradingRules, mark);
            results.Add(new StudentResult
            {
                Student = students[studentIndex],
                Course = courses[courseCode],
                AcademicYear = priorYear,
                Semester = priorSemester,
                Marks = mark,
                Grade = grade,
                GradePoint = gradePoint,
                Status = status
            });
        }

        context.StudentResults.AddRange(results);
        await context.SaveChangesAsync();

        var failedResults = results.Where(r => r.Status == ResultStatus.Fail).ToList();

        var eligibilities = failedResults.Select(r => new ReExamEligibility
        {
            Student = r.Student,
            StudentResult = r,
            Course = r.Course,
            AcademicYear = priorYear,
            Semester = priorSemester,
            Status = EligibilityStatus.Eligible,
            Reason = $"Failed {r.Course.Code} with a mark of {r.Marks:0.##}, below the minimum pass mark. Retake permitted."
        }).ToList();

        context.ReExamEligibilities.AddRange(eligibilities);

        var reExamSubjects = failedResults
            .Select(r => r.Course)
            .DistinctBy(c => c.Id)
            .Select(c => new ReExamSubject { Course = c, AcademicYear = reExamYear, Semester = reExamSemester, IsActive = true })
            .ToList();

        context.ReExamSubjects.AddRange(reExamSubjects);
        await context.SaveChangesAsync();

        // Sample applications across the three review states.
        var applications = new List<ReExamApplication>
        {
            new()
            {
                Student = eligibilities[0].Student,
                Course = eligibilities[0].Course,
                ReExamEligibility = eligibilities[0],
                AcademicYear = reExamYear,
                Semester = reExamSemester,
                Status = ApplicationStatus.Pending
            },
            new()
            {
                Student = eligibilities[1].Student,
                Course = eligibilities[1].Course,
                ReExamEligibility = eligibilities[1],
                AcademicYear = reExamYear,
                Semester = reExamSemester,
                Status = ApplicationStatus.Approved,
                ReviewedByUserId = officerUserId,
                ReviewedAt = DateTime.UtcNow.AddDays(-2)
            },
        };

        if (eligibilities.Count > 2)
        {
            applications.Add(new ReExamApplication
            {
                Student = eligibilities[2].Student,
                Course = eligibilities[2].Course,
                ReExamEligibility = eligibilities[2],
                AcademicYear = reExamYear,
                Semester = reExamSemester,
                Status = ApplicationStatus.Rejected,
                RejectionReason = "Outstanding tuition balance must be cleared before a re-exam application can be approved.",
                ReviewedByUserId = officerUserId,
                ReviewedAt = DateTime.UtcNow.AddDays(-1)
            });
        }

        context.ReExamApplications.AddRange(applications);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExamInfrastructureAsync(ApplicationDbContext context, Dictionary<string, Department> departments)
    {
        var rooms = new[]
        {
            new ExamRoom { RoomNumber = "101", Building = "Block A", Capacity = 50, Location = "Ground Floor", Status = RoomStatus.Available },
            new ExamRoom { RoomNumber = "202", Building = "Block B", Capacity = 30, Location = "Second Floor", Status = RoomStatus.Available },
        };
        context.ExamRooms.AddRange(rooms);

        var invigilators = new[]
        {
            new Invigilator { StaffId = "INV001", FullName = "Dr. Amina Yusuf", Email = "amina.yusuf@reexam.edu", PhoneNumber = "0000000001", Department = departments["CS"], Status = InvigilatorStatus.Active },
            new Invigilator { StaffId = "INV002", FullName = "Mr. John Doe", Email = "john.doe@reexam.edu", PhoneNumber = "0000000002", Department = departments["CS"], Status = InvigilatorStatus.Active },
        };
        context.Invigilators.AddRange(invigilators);

        await context.SaveChangesAsync();
    }
}
