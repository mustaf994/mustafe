namespace ReExamManagementSystem.Application.Common;

/// <summary>Framework-agnostic dropdown option. Views turn these into a Microsoft.AspNetCore.Mvc.Rendering.SelectList.</summary>
public class SelectOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
