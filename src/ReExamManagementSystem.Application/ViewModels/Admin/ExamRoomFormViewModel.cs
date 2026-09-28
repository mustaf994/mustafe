using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class ExamRoomFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Room number is required.")]
    [StringLength(20)]
    [Display(Name = "Room Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building is required.")]
    [StringLength(100)]
    public string Building { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
    public int Capacity { get; set; } = 30;

    [StringLength(200)]
    public string? Location { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Available;
}
