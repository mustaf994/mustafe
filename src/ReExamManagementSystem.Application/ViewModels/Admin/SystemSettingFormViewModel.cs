using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class SystemSettingFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Key is required.")]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required.")]
    [StringLength(500)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
