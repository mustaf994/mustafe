using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IExamAttendanceService
{
    Task<AttendanceViewModel?> GetForScheduleAsync(int scheduleId, CancellationToken cancellationToken = default);
    Task<ServiceResult> SaveAsync(int scheduleId, IReadOnlyDictionary<int, AttendanceStatus> attendance, string recordedByUserId, CancellationToken cancellationToken = default);
}
