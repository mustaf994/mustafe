using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccountService _userAccountService;

    public StudentService(IUnitOfWork unitOfWork, IUserAccountService userAccountService)
    {
        _unitOfWork = unitOfWork;
        _userAccountService = userAccountService;
    }

    public async Task<PagedResult<StudentListItemViewModel>> GetPagedAsync(string? search, int? departmentId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Department)
            .Include(s => s.Program)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.FullName.Contains(search) ||
                s.StudentNumber.Contains(search) ||
                s.Email.Contains(search));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderBy(s => s.FullName)
            .Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                StudentNumber = s.StudentNumber,
                FullName = s.FullName,
                Email = s.Email,
                DepartmentName = s.Department.Name,
                ProgramName = s.Program.Name,
                Level = s.Level,
                Status = s.Status
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<int?> GetStudentIdByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        return student?.Id;
    }

    public async Task<StudentFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        var model = new StudentFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<StudentFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id, cancellationToken);
        if (student is null) return null;

        var model = new StudentFormViewModel
        {
            Id = student.Id,
            StudentNumber = student.StudentNumber,
            FullName = student.FullName,
            Gender = student.Gender,
            DateOfBirth = student.DateOfBirth,
            PhoneNumber = student.PhoneNumber,
            Email = student.Email,
            DepartmentId = student.DepartmentId,
            ProgramId = student.ProgramId,
            Level = student.Level,
            AcademicYearId = student.AcademicYearId,
            SemesterId = student.SemesterId,
            EnrollmentDate = student.EnrollmentDate,
            Status = student.Status
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<StudentDetailsViewModel?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Department)
            .Include(s => s.Program)
            .Include(s => s.AcademicYear)
            .Include(s => s.Semester)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student is null) return null;

        var results = await _unitOfWork.Repository<StudentResult>().Query()
            .Include(r => r.Course)
            .Include(r => r.AcademicYear)
            .Include(r => r.Semester)
            .Where(r => r.StudentId == id)
            .OrderByDescending(r => r.AcademicYear.StartDate).ThenBy(r => r.Course.Code)
            .Select(r => new StudentResultRowViewModel
            {
                CourseCode = r.Course.Code,
                CourseName = r.Course.Name,
                AcademicYearName = r.AcademicYear.Name,
                SemesterName = r.Semester.Name,
                Marks = r.Marks,
                Grade = r.Grade,
                GradePoint = r.GradePoint,
                Status = r.Status
            })
            .ToListAsync(cancellationToken);

        return new StudentDetailsViewModel
        {
            Id = student.Id,
            StudentNumber = student.StudentNumber,
            FullName = student.FullName,
            Gender = student.Gender,
            DateOfBirth = student.DateOfBirth,
            PhoneNumber = student.PhoneNumber,
            Email = student.Email,
            DepartmentName = student.Department.Name,
            ProgramName = student.Program.Name,
            Level = student.Level,
            AcademicYearName = student.AcademicYear.Name,
            SemesterName = student.Semester.Name,
            EnrollmentDate = student.EnrollmentDate,
            Status = student.Status,
            Results = results
        };
    }

    public async Task<ServiceResult<string>> CreateAsync(StudentFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<Student>().AnyAsync(s => s.StudentNumber == model.StudentNumber, cancellationToken))
        {
            return ServiceResult<string>.Failure("A student with this student number already exists.");
        }

        var accountResult = await _userAccountService.CreateUserAsync(model.Email, model.FullName, Roles.Student, cancellationToken);
        if (!accountResult.Succeeded)
        {
            return ServiceResult<string>.Failure(accountResult.Errors.ToArray());
        }

        var (userId, temporaryPassword) = accountResult.Data;

        try
        {
            await _unitOfWork.Repository<Student>().AddAsync(new Student
            {
                StudentNumber = model.StudentNumber,
                UserId = userId,
                FullName = model.FullName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                ProgramId = model.ProgramId,
                Level = model.Level,
                AcademicYearId = model.AcademicYearId,
                SemesterId = model.SemesterId,
                EnrollmentDate = model.EnrollmentDate,
                Status = model.Status
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Compensate: the Identity account was created above but the Student
            // row failed to save (e.g. a constraint violation) - don't leave an
            // orphaned login behind.
            await _userAccountService.DeleteUserAsync(userId, cancellationToken);
            throw;
        }

        return ServiceResult<string>.Success(temporaryPassword);
    }

    public async Task<ServiceResult> UpdateAsync(StudentFormViewModel model, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(model.Id, cancellationToken);
        if (student is null) return ServiceResult.Failure("Student not found.");

        if (await _unitOfWork.Repository<Student>().AnyAsync(s => s.StudentNumber == model.StudentNumber && s.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A student with this student number already exists.");
        }

        var profileResult = await _userAccountService.UpdateProfileAsync(student.UserId, model.FullName, model.PhoneNumber, cancellationToken);
        if (!profileResult.Succeeded)
        {
            return profileResult;
        }

        student.StudentNumber = model.StudentNumber;
        student.FullName = model.FullName;
        student.Gender = model.Gender;
        student.DateOfBirth = model.DateOfBirth;
        student.PhoneNumber = model.PhoneNumber ?? string.Empty;
        student.Email = model.Email;
        student.DepartmentId = model.DepartmentId;
        student.ProgramId = model.ProgramId;
        student.Level = model.Level;
        student.AcademicYearId = model.AcademicYearId;
        student.SemesterId = model.SemesterId;
        student.EnrollmentDate = model.EnrollmentDate;
        student.Status = model.Status;

        _unitOfWork.Repository<Student>().Update(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // A student's account should only be usable for logging in while Active.
        await _userAccountService.SetActiveAsync(student.UserId, model.Status == StudentStatus.Active, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id, cancellationToken);
        if (student is null) return ServiceResult.Failure("Student not found.");

        var userId = student.UserId;
        _unitOfWork.Repository<Student>().Remove(student);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This student has academic results, applications or exam records and cannot be deleted.");
        }

        await _userAccountService.DeleteUserAsync(userId, cancellationToken);
        return ServiceResult.Success();
    }

    private async Task PopulateOptionsAsync(StudentFormViewModel model, CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync(cancellationToken);
        var programs = await _unitOfWork.Repository<AcademicProgram>().GetAllAsync(cancellationToken);
        var academicYears = await _unitOfWork.Repository<AcademicYear>().GetAllAsync(cancellationToken);
        var semesters = await _unitOfWork.Repository<Semester>().GetAllAsync(cancellationToken);

        model.DepartmentOptions = departments.OrderBy(d => d.Name).Select(d => new SelectOption { Id = d.Id, Name = d.Name }).ToList();
        model.ProgramOptions = programs.OrderBy(p => p.Name).Select(p => new ProgramOption { Id = p.Id, Name = p.Name, DepartmentId = p.DepartmentId }).ToList();
        model.AcademicYearOptions = academicYears.OrderByDescending(y => y.StartDate).Select(y => new SelectOption { Id = y.Id, Name = y.Name }).ToList();
        model.SemesterOptions = semesters.OrderBy(s => s.Name).Select(s => new SemesterOption { Id = s.Id, Name = s.Name, AcademicYearId = s.AcademicYearId }).ToList();
    }
}
