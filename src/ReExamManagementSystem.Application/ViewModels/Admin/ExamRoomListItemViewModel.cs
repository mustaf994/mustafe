using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class ExamRoomListItemViewModel
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public RoomStatus Status { get; set; }
}
