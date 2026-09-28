using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ExamRoomService : IExamRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamRoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ExamRoomListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ExamRoom>().Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.RoomNumber.Contains(search) || r.Building.Contains(search));
        }

        return await query
            .OrderBy(r => r.Building).ThenBy(r => r.RoomNumber)
            .Select(r => new ExamRoomListItemViewModel
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Building = r.Building,
                Capacity = r.Capacity,
                Location = r.Location,
                Status = r.Status
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ExamRoomFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Repository<ExamRoom>().GetByIdAsync(id, cancellationToken);
        if (room is null) return null;

        return new ExamRoomFormViewModel
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Building = room.Building,
            Capacity = room.Capacity,
            Location = room.Location,
            Status = room.Status
        };
    }

    public async Task<ServiceResult> CreateAsync(ExamRoomFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<ExamRoom>().AnyAsync(r => r.Building == model.Building && r.RoomNumber == model.RoomNumber, cancellationToken))
        {
            return ServiceResult.Failure("A room with this number already exists in this building.");
        }

        await _unitOfWork.Repository<ExamRoom>().AddAsync(new ExamRoom
        {
            RoomNumber = model.RoomNumber,
            Building = model.Building,
            Capacity = model.Capacity,
            Location = model.Location,
            Status = model.Status
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ExamRoomFormViewModel model, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Repository<ExamRoom>().GetByIdAsync(model.Id, cancellationToken);
        if (room is null) return ServiceResult.Failure("Exam room not found.");

        if (await _unitOfWork.Repository<ExamRoom>().AnyAsync(r => r.Building == model.Building && r.RoomNumber == model.RoomNumber && r.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A room with this number already exists in this building.");
        }

        room.RoomNumber = model.RoomNumber;
        room.Building = model.Building;
        room.Capacity = model.Capacity;
        room.Location = model.Location;
        room.Status = model.Status;
        _unitOfWork.Repository<ExamRoom>().Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Repository<ExamRoom>().GetByIdAsync(id, cancellationToken);
        if (room is null) return ServiceResult.Failure("Exam room not found.");

        _unitOfWork.Repository<ExamRoom>().Remove(room);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This room has exam schedules assigned to it and cannot be deleted.");
        }
    }
}
