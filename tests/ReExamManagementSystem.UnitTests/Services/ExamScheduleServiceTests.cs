using ReExamManagementSystem.Application.Services;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Data;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.UnitTests.TestHelpers;

namespace ReExamManagementSystem.UnitTests.Services;

public class ExamScheduleServiceTests
{
    private static async Task<(ApplicationDbContext Context, Course CourseA, Course CourseB, ExamRoom Room, Invigilator Invigilator, int DepartmentId, int AcademicYearId, int SemesterId)> SeedAsync()
    {
        var context = InMemoryDbContextFactory.Create();
        var (_, department, _, academicYear, semester, courseA) = await TestDataBuilder.SeedAcademicStructureAsync(context);

        var courseB = new Course { Code = "CS102", Name = "Data Structures", CreditHours = 3, Level = 1, DepartmentId = department.Id, AcademicYearId = academicYear.Id, SemesterId = semester.Id };
        context.Courses.Add(courseB);

        var room = new ExamRoom { RoomNumber = "101", Building = "Block A", Capacity = 30, Status = RoomStatus.Available };
        context.ExamRooms.Add(room);

        var invigilator = new Invigilator { StaffId = "INV001", FullName = "Dr. Test", Email = "invig@test.edu", PhoneNumber = "000", DepartmentId = department.Id, Status = InvigilatorStatus.Active };
        context.Invigilators.Add(invigilator);

        await context.SaveChangesAsync();

        return (context, courseA, courseB, room, invigilator, department.Id, academicYear.Id, semester.Id);
    }

    private static ExamScheduleFormViewModel BuildForm(int courseId, int academicYearId, int semesterId, int roomId, int invigilatorId, TimeSpan start, TimeSpan end) => new()
    {
        CourseId = courseId,
        AcademicYearId = academicYearId,
        SemesterId = semesterId,
        ExamDate = new DateTime(2026, 1, 15),
        StartTime = start,
        EndTime = end,
        ExamRoomId = roomId,
        InvigilatorId = invigilatorId,
        Status = ExamStatus.Scheduled
    };

    [Fact]
    public async Task CreateAsync_Succeeds_ForANonConflictingSchedule()
    {
        var (context, courseA, _, room, invigilator, _, ay, sem) = await SeedAsync();
        using var _ = context;
        var service = new ExamScheduleService(new UnitOfWork(context));

        var result = await service.CreateAsync(BuildForm(courseA.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0)));

        Assert.True(result.Succeeded);
        Assert.Single(context.ExamSchedules);
    }

    [Fact]
    public async Task CreateAsync_RejectsARoomDoubleBookingAtAnOverlappingTime()
    {
        var (context, courseA, courseB, room, invigilator, departmentId, ay, sem) = await SeedAsync();
        using var _ = context;
        var service = new ExamScheduleService(new UnitOfWork(context));

        var otherInvigilator = new Invigilator { StaffId = "INV002", FullName = "Dr. Other", Email = "other@test.edu", PhoneNumber = "000", DepartmentId = departmentId, Status = InvigilatorStatus.Active };
        context.Invigilators.Add(otherInvigilator);
        await context.SaveChangesAsync();

        var first = await service.CreateAsync(BuildForm(courseA.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0)));
        var conflicting = await service.CreateAsync(BuildForm(courseB.Id, ay, sem, room.Id, otherInvigilator.Id, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)));

        Assert.True(first.Succeeded);
        Assert.False(conflicting.Succeeded);
        Assert.Single(context.ExamSchedules);
    }

    [Fact]
    public async Task CreateAsync_RejectsAnInvigilatorDoubleBookingAtAnOverlappingTime()
    {
        var (context, courseA, courseB, room, invigilator, _, ay, sem) = await SeedAsync();
        using var _ = context;
        var service = new ExamScheduleService(new UnitOfWork(context));

        var otherRoom = new ExamRoom { RoomNumber = "202", Building = "Block B", Capacity = 30, Status = RoomStatus.Available };
        context.ExamRooms.Add(otherRoom);
        await context.SaveChangesAsync();

        var first = await service.CreateAsync(BuildForm(courseA.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0)));
        var conflicting = await service.CreateAsync(BuildForm(courseB.Id, ay, sem, otherRoom.Id, invigilator.Id, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)));

        Assert.True(first.Succeeded);
        Assert.False(conflicting.Succeeded);
        Assert.Single(context.ExamSchedules);
    }

    [Fact]
    public async Task CreateAsync_AllowsBackToBackSchedulesThatDoNotOverlap()
    {
        var (context, courseA, courseB, room, invigilator, _, ay, sem) = await SeedAsync();
        using var _ = context;
        var service = new ExamScheduleService(new UnitOfWork(context));

        var first = await service.CreateAsync(BuildForm(courseA.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0)));
        var second = await service.CreateAsync(BuildForm(courseB.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0)));

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(2, context.ExamSchedules.Count());
    }

    [Fact]
    public async Task CreateAsync_RejectsWhenEndTimeIsNotAfterStartTime()
    {
        var (context, courseA, _, room, invigilator, _, ay, sem) = await SeedAsync();
        using var _ = context;
        var service = new ExamScheduleService(new UnitOfWork(context));

        var result = await service.CreateAsync(BuildForm(courseA.Id, ay, sem, room.Id, invigilator.Id, new TimeSpan(11, 0, 0), new TimeSpan(9, 0, 0)));

        Assert.False(result.Succeeded);
    }
}
